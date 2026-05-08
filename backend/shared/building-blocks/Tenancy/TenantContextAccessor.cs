namespace ClinicSaaS.BuildingBlocks.Tenancy;

public sealed class TenantContextAccessor : ITenantContextAccessor
{
    public TenantContext Current { get; private set; } = TenantContext.Empty;

    public bool HasTenant => Current.HasTenant;

    public void SetCurrent(TenantContext tenantContext)
    {
        Current = tenantContext;
    }
}
