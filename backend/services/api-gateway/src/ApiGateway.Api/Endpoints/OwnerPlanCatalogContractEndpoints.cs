using ApiGateway.Application.Tenants;
using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Tenancy;
using ClinicSaaS.Observability.Correlation;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace ApiGateway.Api.Endpoints;

/// <summary>
/// API Gateway routes cho Owner Plan & Module Catalog; gateway chỉ forward sang Tenant Service.
/// </summary>
public static class OwnerPlanCatalogContractEndpoints
{
    /// <summary>
    /// Map route `/api/owner/*` tại API Gateway cho FE `/plans`.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của API Gateway.</param>
    /// <returns>Endpoint route builder sau khi map Owner Plan routes.</returns>
    public static IEndpointRouteBuilder MapOwnerPlanCatalogContractEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/owner")
            .WithTags("Owner Plan Catalog")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin)
            .AddEndpointFilter(RequireOwnerWhenRoleIsPresentAsync);

        group.MapGet("/plans", async (
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.ListOwnerPlansAsync(
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("ApiGatewayOwnerListPlans")
            .WithSummary("Forwards Owner Plan catalog requests to Tenant Service.");

        group.MapGet("/modules", async (
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.ListOwnerModulesAsync(
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("ApiGatewayOwnerListModules")
            .WithSummary("Forwards Owner Module entitlement requests to Tenant Service.");

        group.MapGet("/tenant-plan-assignments", async (
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.ListOwnerTenantPlanAssignmentsAsync(
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("ApiGatewayOwnerListTenantPlanAssignments")
            .WithSummary("Forwards Owner tenant plan assignment requests to Tenant Service.");

        group.MapPost("/tenant-plan-assignments/bulk-change", async (
            BulkChangeTenantPlanRequest request,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.BulkChangeOwnerTenantPlansAsync(
                    request,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.PlansWrite)
            .WithName("ApiGatewayOwnerBulkChangeTenantPlans")
            .WithSummary("Forwards Owner tenant plan bulk-change requests to Tenant Service.");

        return endpoints;
    }

    private static ValueTask<object?> RequireOwnerWhenRoleIsPresentAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContextAccessor>().Current;
        var ownerRoleHeader = context.HttpContext.Request.Headers["X-Owner-Role"].FirstOrDefault();
        var headerHasRole = !string.IsNullOrWhiteSpace(ownerRoleHeader);
        var headerIsOwner = IsOwnerRole(ownerRoleHeader);

        if ((headerHasRole && !headerIsOwner)
            || (userContext.Roles.Count > 0 && !userContext.HasRole(RoleNames.OwnerSuperAdmin)))
        {
            return ValueTask.FromResult<object?>(HttpResults.Problem(
                detail: "Owner Super Admin role is required for cross-tenant plan catalog endpoints.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Owner role required"));
        }

        return next(context);
    }

    private static bool IsOwnerRole(string? role)
    {
        return string.Equals(role, RoleNames.OwnerSuperAdmin, StringComparison.Ordinal)
            || string.Equals(role, "OwnerSuperAdmin", StringComparison.Ordinal);
    }

    private static string? GetCorrelationId(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value)
            ? value as string
            : httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault();
    }

    private static async Task<IResult> ToGatewayResultAsync(
        HttpResponseMessage response,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (response.Headers.Location is not null)
        {
            httpContext.Response.Headers.Location = response.Headers.Location.ToString();
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(body))
        {
            return HttpResults.StatusCode((int)response.StatusCode);
        }

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return HttpResults.Content(body, contentType, statusCode: (int)response.StatusCode);
    }
}
