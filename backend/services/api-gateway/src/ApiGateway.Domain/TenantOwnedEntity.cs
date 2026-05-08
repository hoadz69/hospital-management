namespace ApiGateway.Domain;

public abstract class TenantOwnedEntity
{
    public string TenantId { get; protected init; } = string.Empty;
}
