namespace IdentityService.Api.Middleware;

public sealed class TenantContextMiddleware(RequestDelegate next) :
    ClinicSaaS.BuildingBlocks.Tenancy.TenantContextMiddleware(next)
{
}
