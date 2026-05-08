namespace ClinicSaaS.BuildingBlocks.Tenancy;

public sealed record TenantContext(string? TenantId, string? Source, bool IsPlatformScope = false)
{
    public bool HasTenant => !string.IsNullOrWhiteSpace(TenantId);

    public static TenantContext Empty { get; } = new(null, null);

    public static TenantContext Platform { get; } = new(null, "endpoint:platform", IsPlatformScope: true);
}
