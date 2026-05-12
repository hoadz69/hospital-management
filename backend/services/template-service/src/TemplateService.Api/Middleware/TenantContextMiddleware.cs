namespace TemplateService.Api.Middleware;

/// <summary>
/// Wrapper middleware tenant context để Template Service giữ namespace riêng nhưng dùng implementation chung.
/// </summary>
/// <param name="next">Middleware kế tiếp trong ASP.NET Core pipeline.</param>
public sealed class TenantContextMiddleware(RequestDelegate next) :
    ClinicSaaS.BuildingBlocks.Tenancy.TenantContextMiddleware(next)
{
}
