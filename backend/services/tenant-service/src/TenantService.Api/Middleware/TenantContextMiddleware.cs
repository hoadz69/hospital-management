namespace TenantService.Api.Middleware;

public sealed class TenantContextMiddleware(RequestDelegate next) :
    ClinicSaaS.BuildingBlocks.Tenancy.TenantContextMiddleware(next)
{
}
