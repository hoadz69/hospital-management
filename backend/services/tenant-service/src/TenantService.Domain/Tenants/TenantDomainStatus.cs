namespace TenantService.Domain.Tenants;

/// <summary>
/// Trạng thái domain trong Tenant MVP, chưa bao gồm DNS/SSL verification thật.
/// </summary>
public enum TenantDomainStatus
{
    /// <summary>
    /// Domain mới gắn, chưa active/verify.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Domain đang active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Domain bị tạm ngưng theo tenant hoặc quản trị platform.
    /// </summary>
    Suspended = 2
}
