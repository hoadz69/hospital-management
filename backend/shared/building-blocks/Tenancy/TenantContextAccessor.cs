namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Lưu tenant context đã resolve cho các layer sau middleware.
/// </summary>
public sealed class TenantContextAccessor : ITenantContextAccessor
{
    /// <summary>
    /// Tenant context hiện tại của request.
    /// </summary>
    public TenantContext Current { get; private set; } = TenantContext.Empty;

    /// <summary>
    /// Cho biết request hiện tại có tenant id hợp lệ hay không.
    /// </summary>
    public bool HasTenant => Current.HasTenant;

    /// <summary>
    /// Gán tenant context đã resolve cho request hiện tại.
    /// </summary>
    /// <param name="tenantContext">Tenant context được middleware hoặc test thiết lập.</param>
    public void SetCurrent(TenantContext tenantContext)
    {
        Current = tenantContext;
    }
}
