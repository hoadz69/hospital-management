using ClinicSaaS.Contracts.Tenancy;
using TenantService.Domain.Tenants;

namespace TenantService.Application.Tenants;

/// <summary>
/// Mapper từ domain tenant sang shared response contract.
/// </summary>
public static class TenantResponseMapper
{
    /// <summary>
    /// Map aggregate tenant đầy đủ sang response chi tiết.
    /// </summary>
    /// <param name="tenant">Aggregate tenant cần trả về API.</param>
    /// <returns>Response chi tiết gồm profile, domains, modules và lifecycle timestamps.</returns>
    public static TenantResponse ToResponse(Tenant tenant)
    {
        return new TenantResponse(
            tenant.Id.ToString(),
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status.ToString(),
            tenant.Plan.PlanCode,
            tenant.Plan.DisplayName,
            new TenantProfileResponse(
                tenant.Profile.ClinicName,
                tenant.Profile.ContactEmail,
                tenant.Profile.PhoneNumber,
                tenant.Profile.AddressLine,
                tenant.Profile.Specialty),
            tenant.Domains.Select(ToDomainResponse).ToArray(),
            tenant.Modules.Select(ToModuleResponse).ToArray(),
            tenant.CreatedAtUtc,
            tenant.UpdatedAtUtc,
            tenant.ActivatedAtUtc,
            tenant.SuspendedAtUtc,
            tenant.ArchivedAtUtc);
    }

    /// <summary>
    /// Map aggregate tenant sang response rút gọn cho danh sách.
    /// </summary>
    /// <param name="tenant">Aggregate tenant hoặc projection tenant cần đưa vào danh sách.</param>
    /// <returns>Response rút gọn cho một dòng danh sách tenant.</returns>
    public static TenantSummaryResponse ToSummaryResponse(Tenant tenant)
    {
        return new TenantSummaryResponse(
            tenant.Id.ToString(),
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status.ToString(),
            tenant.Plan.PlanCode,
            tenant.Profile.ClinicName,
            tenant.CreatedAtUtc,
            tenant.UpdatedAtUtc);
    }

    private static TenantDomainResponse ToDomainResponse(TenantDomain domain)
    {
        return new TenantDomainResponse(
            domain.Id.ToString(),
            domain.DomainName,
            domain.NormalizedDomainName,
            domain.DomainType.ToString(),
            domain.Status.ToString(),
            domain.IsPrimary,
            domain.CreatedAtUtc,
            domain.VerifiedAtUtc);
    }

    private static TenantModuleResponse ToModuleResponse(TenantModule module)
    {
        return new TenantModuleResponse(
            module.ModuleCode,
            module.IsEnabled,
            module.SourcePlanCode,
            module.CreatedAtUtc,
            module.UpdatedAtUtc);
    }
}
