using ClinicSaaS.BuildingBlocks.Results;

namespace TenantService.Application.Domains;

/// <summary>
/// Factory loi nghiep vu cho Domain DNS/SSL API trong Tenant Service.
/// </summary>
public static class TenantDomainOperationErrors
{
    /// <summary>
    /// Tao loi validation cho tenant/domain id khong hop le.
    /// </summary>
    /// <param name="details">Danh sach loi theo field request.</param>
    /// <returns>Error de API layer map sang 400.</returns>
    public static Error Validation(IReadOnlyDictionary<string, string[]> details)
        => new("domains.validation", "Domain DNS/SSL request is invalid.", details);

    /// <summary>
    /// Tao loi khong tim thay tenant.
    /// </summary>
    /// <param name="tenantId">Tenant id caller dang truy van.</param>
    /// <returns>Error de API layer map sang 404.</returns>
    public static Error TenantNotFound(Guid tenantId)
        => new("domains.not_found", $"Tenant '{tenantId}' was not found.");

    /// <summary>
    /// Tao loi khong tim thay domain trong tenant.
    /// </summary>
    /// <param name="domainId">Domain id caller dang truy van.</param>
    /// <returns>Error de API layer map sang 404.</returns>
    public static Error DomainNotFound(Guid domainId)
        => new("domains.not_found", $"Domain '{domainId}' was not found for this tenant.");
}
