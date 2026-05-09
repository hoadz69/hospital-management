namespace ApiGateway.Domain;

/// <summary>
/// Base entity placeholder nếu API Gateway cần model tenant-owned nội bộ về sau.
/// </summary>
public abstract class TenantOwnedEntity
{
    /// <summary>
    /// Tenant sở hữu entity nếu API Gateway có dữ liệu nội bộ tenant-owned về sau.
    /// </summary>
    public string TenantId { get; protected init; } = string.Empty;
}
