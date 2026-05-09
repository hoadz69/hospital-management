using ApiGateway.Application.Tenants;
using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Tenancy;
using ClinicSaaS.Observability.Correlation;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace ApiGateway.Api.Endpoints;

/// <summary>
/// Minimal API endpoints tenant management tại API Gateway.
/// </summary>
public static class TenantEndpoints
{
    /// <summary>
    /// Map các endpoint platform-scoped và forward request sang Tenant Service.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của API Gateway.</param>
    /// <returns>Endpoint route builder sau khi đã map nhóm `/api/tenants`.</returns>
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin);

        group.MapPost("/", async (
            CreateTenantRequest request,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.CreateTenantAsync(
                    request,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.TenantsWrite)
            .WithName("ApiGatewayCreateTenant")
            .WithSummary("Forwards tenant creation to Tenant Service.");

        group.MapGet("/", async (
            string? status,
            string? search,
            int? limit,
            int? offset,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.ListTenantsAsync(
                    status,
                    search,
                    limit,
                    offset,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.TenantsRead)
            .WithName("ApiGatewayListTenants")
            .WithSummary("Forwards tenant list requests to Tenant Service.");

        group.MapGet("/{tenantId:guid}", async (
            Guid tenantId,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.GetTenantByIdAsync(
                    tenantId,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.TenantsRead)
            .WithName("ApiGatewayGetTenantById")
            .WithSummary("Forwards tenant detail requests to Tenant Service.");

        group.MapPatch("/{tenantId:guid}/status", async (
            Guid tenantId,
            UpdateTenantStatusRequest request,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.UpdateTenantStatusAsync(
                    tenantId,
                    request,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.TenantsWrite)
            .WithName("ApiGatewayUpdateTenantStatus")
            .WithSummary("Forwards tenant status updates to Tenant Service.");

        return endpoints;
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
