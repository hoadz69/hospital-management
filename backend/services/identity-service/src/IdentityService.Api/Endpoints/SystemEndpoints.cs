using ClinicSaaS.BuildingBlocks.SystemEndpoints;

namespace IdentityService.Api.Endpoints;

/// <summary>
/// System endpoint wrapper của Identity Service.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Map system endpoints dùng chung cho Identity Service.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Identity Service.</param>
    /// <param name="serviceName">Tên service trả về trong response system endpoints.</param>
    /// <returns>Endpoint route builder sau khi map system endpoints.</returns>
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints, string serviceName)
    {
        return endpoints.MapClinicSaaSSystemEndpoints(serviceName);
    }
}
