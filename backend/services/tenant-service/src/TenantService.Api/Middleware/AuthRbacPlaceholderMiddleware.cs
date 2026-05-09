namespace TenantService.Api.Middleware;

/// <summary>
/// Wrapper RBAC placeholder để Tenant Service giữ namespace riêng nhưng dùng implementation chung.
/// </summary>
/// <param name="next">Middleware kế tiếp trong ASP.NET Core pipeline.</param>
public sealed class AuthRbacPlaceholderMiddleware(RequestDelegate next) :
    ClinicSaaS.BuildingBlocks.Authorization.AuthRbacPlaceholderMiddleware(next)
{
}
