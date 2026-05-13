using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using TenantService.Application.Domains;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace TenantService.Api.Endpoints;

/// <summary>
/// Minimal API endpoints cho Domain DNS/SSL state do Tenant Service persist.
/// </summary>
public static class TenantDomainOperationsEndpoints
{
    /// <summary>
    /// Map cac endpoint FE dung de bo mock DNS retry va SSL pending.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder cua Tenant Service API.</param>
    /// <returns>Endpoint route builder sau khi map nhom domain operations.</returns>
    public static IEndpointRouteBuilder MapTenantDomainOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants/{tenantId:guid}")
            .WithTags("Tenant Domain Operations")
            .RequireTenantContext()
            .RequireRole(RoleNames.OwnerSuperAdmin)
            .AddEndpointFilter(EnsureTenantRouteMatchesContextAsync)
            .AddEndpointFilter(RequireOwnerWhenRoleIsPresentAsync);

        group.MapGet("/domains", (
            Guid tenantId,
            TenantDomainOperationsHandler handler,
            CancellationToken cancellationToken) => ToResultAsync(handler.ListDomainsAsync(tenantId, cancellationToken)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("TenantServiceListTenantDomainDnsSslStates")
            .WithSummary("Lists persisted DNS/SSL state for tenant domains.");

        group.MapPost("/domains/{domainId:guid}/dns-retry", (
            Guid tenantId,
            Guid domainId,
            TenantDomainOperationsHandler handler,
            CancellationToken cancellationToken) => ToResultAsync(handler.RetryDnsAsync(tenantId, domainId, cancellationToken)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("TenantServiceRetryTenantDomainDns")
            .WithSummary("Persists DNS retry request and returns updated DNS/SSL state.");

        group.MapGet("/domains/{domainId:guid}/ssl-status", (
            Guid tenantId,
            Guid domainId,
            TenantDomainOperationsHandler handler,
            CancellationToken cancellationToken) => ToResultAsync(handler.GetSslStatusAsync(tenantId, domainId, cancellationToken)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("TenantServiceGetTenantDomainSslStatus")
            .WithSummary("Returns persisted SSL state for one tenant domain.");

        return endpoints;
    }

    private static ValueTask<object?> EnsureTenantRouteMatchesContextAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var routeTenantId = context.HttpContext.Request.RouteValues["tenantId"]?.ToString();
        var tenantContextAccessor = context.HttpContext.RequestServices.GetRequiredService<ITenantContextAccessor>();

        if (!string.Equals(routeTenantId, tenantContextAccessor.Current.TenantId, StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult<object?>(HttpResults.Problem(
                detail: "Tenant context does not match the tenant route parameter.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Tenant scope mismatch"));
        }

        return next(context);
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
                detail: "Owner Super Admin role is required for tenant domain DNS/SSL endpoints.",
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

    private static IResult ToResult<T>(Result<T> result)
    {
        return result.IsSuccess && result.Value is not null
            ? HttpResults.Ok(result.Value)
            : ToErrorResult(result.Error);
    }

    private static async Task<IResult> ToResultAsync<T>(Task<Result<T>> resultTask)
    {
        return ToResult(await resultTask);
    }

    private static IResult ToErrorResult(Error error)
    {
        return error.Code switch
        {
            "domains.validation" => HttpResults.ValidationProblem(ToValidationDetails(error)),
            "domains.not_found" => HttpResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Domain not found"),
            _ => HttpResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: error.Code)
        };
    }

    private static IDictionary<string, string[]> ToValidationDetails(Error error)
    {
        return error.Details is null
            ? new Dictionary<string, string[]> { ["request"] = [error.Message] }
            : error.Details.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
