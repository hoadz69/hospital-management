using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Options;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace ClinicSaaS.BuildingBlocks.SystemEndpoints;

public static class SystemEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapClinicSaaSSystemEndpoints(
        this IEndpointRouteBuilder endpoints,
        string serviceName)
    {
        var group = endpoints.MapGroup("/api/_system").WithTags("System");

        group.MapGet("/openapi-placeholder", () => HttpResults.Ok(new
            {
                service = serviceName,
                status = "configured",
                tenantScope = TenantEndpointScope.Platform.ToString(),
                openApi = new
                {
                    documentName = "v1",
                    json = "/openapi/v1.json",
                    swaggerUi = "/swagger"
                },
                note = "OpenAPI JSON and Swagger UI are enabled for this service."
            }))
            .AllowPlatformScope()
            .WithName(ToEndpointName(serviceName, "OpenApiPlaceholder"))
            .WithSummary("Reports the real OpenAPI endpoint configured for this service.");

        group.MapGet("/tenant-context", (
            ITenantContextAccessor tenantContextAccessor,
            HttpContext httpContext) => HttpResults.Ok(new
            {
                service = serviceName,
                endpointScope = TenantEndpointScope.Tenant.ToString(),
                tenantContext = new
                {
                    tenantContextAccessor.Current.HasTenant,
                    tenantContextAccessor.Current.TenantId,
                    tenantContextAccessor.Current.Source,
                    tenantContextAccessor.Current.IsPlatformScope
                },
                supportedSources = new[]
                {
                    TenantContextMiddleware.TenantHeaderName,
                    $"JWT claim {TenantContextMiddleware.TenantClaimName}"
                },
                authRbacPlaceholder = GetAuthRbacPlaceholder(httpContext)
            }))
            .RequireTenantContext()
            .RequireRole(RoleNames.ClinicAdmin)
            .RequirePermission(PermissionCodes.UsersRead)
            .WithName(ToEndpointName(serviceName, "TenantContext"))
            .WithSummary("Returns the resolved tenant context for a tenant-scoped request.");

        group.MapGet("/auth-rbac-placeholder", (
            HttpContext httpContext,
            IUserContextAccessor userContextAccessor) => HttpResults.Ok(new
            {
                service = serviceName,
                status = "placeholder",
                endpointScope = TenantEndpointScope.Platform.ToString(),
                enforcementMode = "metadata-only-placeholder-not-enforced",
                userContext = userContextAccessor.Current,
                availableRoles = RoleNames.All,
                availablePermissions = PermissionCodes.All,
                endpointRequirement = GetAuthRbacPlaceholder(httpContext),
                sampleTenantRequirement = new AuthRbacRequirement(
                    RoleNames.ClinicAdmin,
                    PermissionCodes.UsersRead,
                    RequiresTenant: true)
            }))
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin)
            .RequirePermission(PermissionCodes.TenantsRead)
            .WithName(ToEndpointName(serviceName, "AuthRbacPlaceholder"))
            .WithSummary("Returns RBAC metadata placeholders without real auth provider enforcement.");

        group.MapGet("/postgres-placeholder", (IOptions<PostgreSqlOptions> options) => HttpResults.Ok(new
            {
                service = serviceName,
                tenantScope = TenantEndpointScope.Platform.ToString(),
                options.Value.Enabled,
                options.Value.Provider,
                hasConnectionString = !string.IsNullOrWhiteSpace(options.Value.ConnectionString),
                options.Value.ConnectionStringEnvironmentVariable
            }))
            .AllowPlatformScope()
            .WithName(ToEndpointName(serviceName, "PostgreSqlPlaceholder"))
            .WithSummary("Returns PostgreSQL placeholder configuration without opening a database connection.");

        return endpoints;
    }

    private static AuthRbacPlaceholderContext? GetAuthRbacPlaceholder(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(AuthRbacPlaceholderMiddleware.ContextItemKey, out var value)
            ? value as AuthRbacPlaceholderContext
            : null;
    }

    private static string ToEndpointName(string serviceName, string suffix)
    {
        var normalized = string.Concat(serviceName
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant));

        return $"{normalized}{suffix}";
    }
}
