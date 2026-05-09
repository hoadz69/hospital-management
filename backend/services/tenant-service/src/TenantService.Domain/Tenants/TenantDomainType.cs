namespace TenantService.Domain.Tenants;

/// <summary>
/// Loại domain gắn với tenant: subdomain mặc định hoặc custom domain.
/// </summary>
public enum TenantDomainType
{
    /// <summary>
    /// Subdomain mặc định do platform cấp.
    /// </summary>
    DefaultSubdomain = 0,

    /// <summary>
    /// Custom domain do owner/khách hàng cấu hình.
    /// </summary>
    Custom = 1
}
