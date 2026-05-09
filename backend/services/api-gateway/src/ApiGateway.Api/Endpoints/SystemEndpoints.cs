using ClinicSaaS.BuildingBlocks.SystemEndpoints;

namespace ApiGateway.Api.Endpoints;

/// <summary>
/// System endpoint wrapper của API Gateway.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Map system endpoints dùng chung cho API Gateway.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của API Gateway.</param>
    /// <param name="serviceName">Tên service trả về trong response system endpoints.</param>
    /// <returns>Endpoint route builder sau khi map system endpoints.</returns>
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints, string serviceName)
    {
        return endpoints.MapClinicSaaSSystemEndpoints(serviceName);
    }
}
