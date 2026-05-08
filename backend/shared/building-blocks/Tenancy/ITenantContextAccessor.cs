namespace ClinicSaaS.BuildingBlocks.Tenancy;

public interface ITenantContextAccessor
{
    TenantContext Current { get; }

    bool HasTenant { get; }

    void SetCurrent(TenantContext tenantContext);
}
