using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;
using TenantService.Domain.Tenants;
using ContractModuleCodes = ClinicSaaS.Contracts.Tenancy.ModuleCodes;

namespace TenantService.Application.Tenants;

/// <summary>
/// Use case tạo tenant/phòng khám mới từ Owner Super Admin.
/// </summary>
public sealed class CreateTenantHandler
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>
    /// Khởi tạo handler với repository persistence của Tenant Service.
    /// </summary>
    /// <param name="tenantRepository">Repository dùng để lưu tenant và các bảng con.</param>
    public CreateTenantHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// Validate request, dựng domain model và persist tenant cùng profile/domain/module trong một transaction repository.
    /// </summary>
    /// <param name="request">Payload tạo tenant từ Owner Super Admin.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối hoặc host dừng.</param>
    /// <returns>Kết quả chứa tenant response khi tạo thành công hoặc lỗi validation/conflict.</returns>
    public async Task<Result<TenantResponse>> HandleAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation.Count > 0)
        {
            return Result<TenantResponse>.Failure(TenantErrors.Validation(validation));
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            var plan = new PlanReference(
                TenantNormalization.Required(request.PlanCode, nameof(request.PlanCode)),
                TenantNormalization.Optional(request.PlanDisplayName));
            var moduleCodes = request.ModuleCodes is { Count: > 0 }
                ? request.ModuleCodes
                : ContractModuleCodes.DefaultTenantModules;
            var defaultDomainName = string.IsNullOrWhiteSpace(request.DefaultDomainName)
                ? request.Slug
                : request.DefaultDomainName;
            var tenant = Tenant.Create(
                request.Slug,
                request.DisplayName,
                plan,
                new ClinicProfileDraft(
                    request.ClinicName,
                    request.ContactEmail,
                    request.PhoneNumber,
                    request.AddressLine,
                    request.Specialty),
                defaultDomainName,
                moduleCodes,
                now);

            var createResult = await _tenantRepository.CreateAsync(tenant, cancellationToken);
            return createResult.IsSuccess && createResult.Value is not null
                ? Result<TenantResponse>.Success(TenantResponseMapper.ToResponse(createResult.Value))
                : Result<TenantResponse>.Failure(createResult.Error);
        }
        catch (ArgumentException ex)
        {
            return Result<TenantResponse>.Failure(TenantErrors.Validation(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "request"] = [ex.Message]
            }));
        }
    }

    private static Dictionary<string, string[]> Validate(CreateTenantRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        AddRequired(errors, nameof(request.Slug), request.Slug);
        AddRequired(errors, nameof(request.DisplayName), request.DisplayName);
        AddRequired(errors, nameof(request.PlanCode), request.PlanCode);
        AddRequired(errors, nameof(request.ClinicName), request.ClinicName);

        return errors;
    }

    private static void AddRequired(IDictionary<string, string[]> errors, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[name] = ["Value is required."];
        }
    }
}
