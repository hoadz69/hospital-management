namespace IdentityService.Api.Middleware;

public sealed class AuthRbacPlaceholderMiddleware(RequestDelegate next) :
    ClinicSaaS.BuildingBlocks.Authorization.AuthRbacPlaceholderMiddleware(next)
{
}
