using ClinicSaaS.BuildingBlocks.SystemEndpoints;

namespace TemplateService.Api.Endpoints;

/// <summary>
/// Endpoint system dùng chung cho Template Service.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Map các endpoint system placeholder của Template Service.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Template Service.</param>
    /// <param name="serviceName">Tên service dùng trong response.</param>
    /// <returns>Endpoint route builder sau khi đã map system endpoints.</returns>
    public static IEndpointRouteBuilder MapSystemEndpoints(
        this IEndpointRouteBuilder endpoints,
        string serviceName)
    {
        return endpoints.MapClinicSaaSSystemEndpoints(serviceName);
    }
}
