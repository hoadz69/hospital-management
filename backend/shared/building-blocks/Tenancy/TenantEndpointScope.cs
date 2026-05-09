namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Phạm vi tenant bắt buộc của một endpoint.
/// </summary>
public enum TenantEndpointScope
{
    /// <summary>
    /// Endpoint chưa khai báo rõ scope, middleware không tự chặn tenant context.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Endpoint thuộc platform và không yêu cầu tenant id.
    /// </summary>
    Platform = 1,

    /// <summary>
    /// Endpoint thuộc tenant và bắt buộc có tenant context hợp lệ.
    /// </summary>
    Tenant = 2
}
