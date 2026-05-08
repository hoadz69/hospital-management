using ClinicSaaS.BuildingBlocks.Options;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using Microsoft.Extensions.Options;

namespace TenantService.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints, string serviceName)
    {
        var group = endpoints.MapGroup("/api/_system").WithTags("System");

        group.MapGet("/openapi-placeholder", () => Results.Ok(new
        {
            service = serviceName,
            status = "placeholder",
            note = "OpenAPI contract generation will be enabled when API contracts are finalized."
        }));

        group.MapGet("/tenant-context", (ITenantContextAccessor tenantContextAccessor) => Results.Ok(new
        {
            tenantContextAccessor.Current.HasTenant,
            tenantContextAccessor.Current.TenantId,
            tenantContextAccessor.Current.Source,
            tenantContextAccessor.Current.IsPlatformScope
        }));

        group.MapGet("/auth-rbac-placeholder", () => Results.Ok(new
        {
            status = "placeholder",
            roles = RoleNames.All,
            permissions = PermissionCodes.All,
            sampleRequirement = new AuthRbacRequirement(
                RoleNames.ClinicAdmin,
                PermissionCodes.UsersRead,
                RequiresTenant: true)
        }));

        group.MapGet("/postgres-placeholder", (IOptions<PostgreSqlOptions> options) => Results.Ok(new
        {
            options.Value.Enabled,
            options.Value.Provider,
            hasConnectionString = !string.IsNullOrWhiteSpace(options.Value.ConnectionString),
            options.Value.ConnectionStringEnvironmentVariable
        }));

        return endpoints;
    }
}
