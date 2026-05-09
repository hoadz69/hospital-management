namespace TenantService.Domain.Tenants;

/// <summary>
/// Trạng thái vòng đời tenant do Owner Super Admin quản lý.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant mới tạo, chưa active vận hành.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Tenant đang hoạt động.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tenant bị tạm ngưng.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Tenant đã lưu trữ và không còn vận hành.
    /// </summary>
    Archived = 3
}
