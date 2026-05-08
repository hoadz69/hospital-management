using ClinicSaaS.BuildingBlocks.Tenancy;

namespace IdentityService.Api.Middleware;

public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContextAccessor)
    {
        var headerTenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        var claimTenantId = context.User.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrWhiteSpace(headerTenantId))
        {
            tenantContextAccessor.SetCurrent(new TenantContext(headerTenantId, "X-Tenant-Id"));
        }
        else if (!string.IsNullOrWhiteSpace(claimTenantId))
        {
            tenantContextAccessor.SetCurrent(new TenantContext(claimTenantId, "jwt:tenant_id"));
        }

        return next(context);
    }
}
