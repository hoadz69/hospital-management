using ClinicSaaS.BuildingBlocks.SystemEndpoints;

namespace WebsiteCmsService.Api.Endpoints;

/// <summary>
/// Endpoint system dùng chung cho Website CMS Service.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Map các endpoint system placeholder của Website CMS Service.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Website CMS Service.</param>
    /// <param name="serviceName">Tên service dùng trong response.</param>
    /// <returns>Endpoint route builder sau khi đã map system endpoints.</returns>
    public static IEndpointRouteBuilder MapSystemEndpoints(
        this IEndpointRouteBuilder endpoints,
        string serviceName)
    {
        return endpoints.MapClinicSaaSSystemEndpoints(serviceName);
    }
}
