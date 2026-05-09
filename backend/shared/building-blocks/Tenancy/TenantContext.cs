namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Tenant context đã resolve cho request hiện tại.
/// </summary>
/// <param name="TenantId">Định danh tenant đã resolve; null khi request không thuộc tenant cụ thể.</param>
/// <param name="Source">Nguồn resolve tenant như header, JWT claim hoặc platform endpoint.</param>
/// <param name="IsPlatformScope">Cho biết request thuộc phạm vi platform và được phép không có tenant id.</param>
public sealed record TenantContext(string? TenantId, string? Source, bool IsPlatformScope = false)
{
    /// <summary>
    /// Cho biết context hiện tại có tenant id hợp lệ hay không.
    /// </summary>
    public bool HasTenant => !string.IsNullOrWhiteSpace(TenantId);

    /// <summary>
    /// Context rỗng khi request chưa resolve tenant.
    /// </summary>
    public static TenantContext Empty { get; } = new(null, null);

    /// <summary>
    /// Context platform cho endpoint được phép thao tác cross-tenant.
    /// </summary>
    public static TenantContext Platform { get; } = new(null, "endpoint:platform", IsPlatformScope: true);
}
