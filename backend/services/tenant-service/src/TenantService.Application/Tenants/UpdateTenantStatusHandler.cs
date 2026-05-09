using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;

namespace TenantService.Application.Tenants;

/// <summary>
/// Use case đổi trạng thái tenant, chưa kích hoạt side effect billing/domain trong Phase 2.
/// </summary>
public sealed class UpdateTenantStatusHandler
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>
    /// Khởi tạo handler với repository cập nhật tenant.
    /// </summary>
    /// <param name="tenantRepository">Repository dùng để cập nhật trạng thái tenant.</param>
    public UpdateTenantStatusHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// Đổi trạng thái tenant theo request của Owner Super Admin.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần cập nhật.</param>
    /// <param name="request">Payload chứa trạng thái mới.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối hoặc host dừng.</param>
    /// <returns>Kết quả chứa tenant sau cập nhật hoặc lỗi validation/not found.</returns>
    public async Task<Result<TenantResponse>> HandleAsync(
        Guid tenantId,
        UpdateTenantStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TenantStatusParser.TryParse(request.Status, out var status))
        {
            return Result<TenantResponse>.Failure(TenantErrors.Validation(new Dictionary<string, string[]>
            {
                [nameof(request.Status)] = ["Status must be one of: Draft, Active, Suspended, Archived."]
            }));
        }

        var updateResult = await _tenantRepository.UpdateStatusAsync(
            tenantId,
            status,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return updateResult.IsSuccess && updateResult.Value is not null
            ? Result<TenantResponse>.Success(TenantResponseMapper.ToResponse(updateResult.Value))
            : Result<TenantResponse>.Failure(updateResult.Error);
    }
}
