namespace ApiGateway.Api.Middleware;

public sealed class AuthRbacPlaceholderMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Items["AuthRbacPlaceholder"] = "Auth/RBAC integration is intentionally not wired in this skeleton.";
        return next(context);
    }
}
