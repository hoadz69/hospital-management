namespace IdentityService.Domain;

/// <summary>
/// Base entity placeholder cho dữ liệu identity thuộc tenant.
/// </summary>
public abstract class TenantOwnedEntity
{
    /// <summary>
    /// Tenant sở hữu entity identity.
    /// </summary>
    public string TenantId { get; protected init; } = string.Empty;
}
