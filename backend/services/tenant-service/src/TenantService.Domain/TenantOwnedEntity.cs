namespace TenantService.Domain;

/// <summary>
/// Base entity placeholder cho dữ liệu thuộc tenant trong Tenant Service.
/// </summary>
public abstract class TenantOwnedEntity
{
    /// <summary>
    /// Tenant sở hữu entity trong Tenant Service.
    /// </summary>
    public string TenantId { get; protected init; } = string.Empty;
}
