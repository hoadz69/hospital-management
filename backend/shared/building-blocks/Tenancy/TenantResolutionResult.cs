namespace ClinicSaaS.BuildingBlocks.Tenancy;

public sealed record TenantResolutionResult(bool IsResolved, TenantContext TenantContext, string? FailureReason)
{
    public static TenantResolutionResult Resolved(string tenantId, string source)
    {
        return new(true, new TenantContext(tenantId, source), null);
    }

    public static TenantResolutionResult NotResolved(string reason)
    {
        return new(false, TenantContext.Empty, reason);
    }
}
