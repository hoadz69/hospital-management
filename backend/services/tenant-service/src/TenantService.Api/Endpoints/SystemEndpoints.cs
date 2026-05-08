using ClinicSaaS.BuildingBlocks.SystemEndpoints;

namespace TenantService.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints, string serviceName)
    {
        return endpoints.MapClinicSaaSSystemEndpoints(serviceName);
    }
}
