using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.Contracts.Tenancy;

namespace TenantService.Application.Plans;

/// <summary>
/// Handler Application cho Owner Plan & Module Catalog dùng PostgreSQL persistence thật.
/// </summary>
public sealed class OwnerPlanCatalogHandler
{
    private const string EffectiveAtNextRenewal = "next_renewal";

    private readonly IOwnerPlanCatalogRepository _repository;
    private readonly IUserContextAccessor _userContextAccessor;

    /// <summary>
    /// Khởi tạo handler với repository Dapper/Npgsql và user context placeholder.
    /// </summary>
    /// <param name="repository">Repository owner-plan persistence.</param>
    /// <param name="userContextAccessor">Accessor chứa actor/user context hiện tại.</param>
    public OwnerPlanCatalogHandler(
        IOwnerPlanCatalogRepository repository,
        IUserContextAccessor userContextAccessor)
    {
        _repository = repository;
        _userContextAccessor = userContextAccessor;
    }

    /// <summary>
    /// Trả plan cards cho Owner Admin `/plans`.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa danh sách plan catalog từ PostgreSQL.</returns>
    public async Task<Result<OwnerPlanCatalogResponse>> ListPlansAsync(CancellationToken cancellationToken)
        => Result<OwnerPlanCatalogResponse>.Success(await _repository.ListPlansAsync(cancellationToken));

    /// <summary>
    /// Trả matrix module entitlement theo từng plan.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa danh sách module entitlement từ PostgreSQL.</returns>
    public async Task<Result<OwnerModuleCatalogResponse>> ListModulesAsync(CancellationToken cancellationToken)
        => Result<OwnerModuleCatalogResponse>.Success(await _repository.ListModulesAsync(cancellationToken));

    /// <summary>
    /// Trả danh sách assignment plan hiện tại của tenant cho Owner Super Admin.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa assignment thật cho bảng bulk-change.</returns>
    public async Task<Result<TenantPlanAssignmentListResponse>> ListTenantPlanAssignmentsAsync(CancellationToken cancellationToken)
        => Result<TenantPlanAssignmentListResponse>.Success(await _repository.ListTenantPlanAssignmentsAsync(cancellationToken));

    /// <summary>
    /// Nhận bulk-change plan và ghi PostgreSQL transaction + audit.
    /// </summary>
    /// <param name="request">Payload bulk-change do Owner Admin gửi lên.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa số tenant đổi plan, MRR diff và audit reason đã ghi.</returns>
    public Task<Result<BulkChangeTenantPlanResponse>> BulkChangeTenantPlansAsync(
        BulkChangeTenantPlanRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateBulkChangeRequest(request);
        if (validation is not null)
        {
            return Task.FromResult(Result<BulkChangeTenantPlanResponse>.Failure(validation));
        }

        return _repository.BulkChangeTenantPlansAsync(
            request,
            _userContextAccessor.Current.UserId,
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    private static Error? ValidateBulkChangeRequest(BulkChangeTenantPlanRequest request)
    {
        var details = new Dictionary<string, string[]>();

        if (request.SelectedTenantIds is null || request.SelectedTenantIds.Count == 0)
        {
            details["selectedTenantIds"] = ["At least one tenant id is required."];
        }
        else
        {
            var invalidIds = request.SelectedTenantIds
                .Where(id => !Guid.TryParse(id, out _))
                .ToArray();
            if (invalidIds.Length > 0)
            {
                details["selectedTenantIds"] = ["Tenant ids must be UUID values returned by the assignment API."];
            }
        }

        if (string.IsNullOrWhiteSpace(request.TargetPlan) || !PlanCodes.All.Contains(request.TargetPlan, StringComparer.Ordinal))
        {
            details["targetPlan"] = ["Target plan must be starter, growth or premium."];
        }

        if (!string.Equals(request.EffectiveAt, EffectiveAtNextRenewal, StringComparison.Ordinal))
        {
            details["effectiveAt"] = ["Only next_renewal is supported for owner plan changes."];
        }

        if (string.IsNullOrWhiteSpace(request.AuditReason))
        {
            details["auditReason"] = ["Audit reason is required for cross-tenant plan changes."];
        }

        return details.Count == 0 ? null : OwnerPlanCatalogErrors.Validation(details);
    }
}
