namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Truy cập tenant context trong vòng đời request hiện tại.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Tenant context hiện tại của request.
    /// </summary>
    TenantContext Current { get; }

    /// <summary>
    /// Cho biết request hiện tại có tenant id hợp lệ hay không.
    /// </summary>
    bool HasTenant { get; }

    /// <summary>
    /// Gán tenant context cho request hiện tại.
    /// </summary>
    /// <param name="tenantContext">Tenant context đã resolve từ middleware hoặc test.</param>
    void SetCurrent(TenantContext tenantContext);
}
