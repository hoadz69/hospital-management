using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Tenancy;
using TenantService.Application.Plans;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace TenantService.Api.Endpoints;

/// <summary>
/// Minimal API endpoints cho Owner Plan & Module Catalog contract/stub trong Tenant Service.
/// </summary>
public static class OwnerPlanCatalogEndpoints
{
    /// <summary>
    /// Map các endpoint platform-scoped và cross-tenant dành riêng cho Owner Super Admin.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Tenant Service API.</param>
    /// <returns>Endpoint route builder sau khi map nhóm `/api/owner`.</returns>
    public static IEndpointRouteBuilder MapOwnerPlanCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/owner")
            .WithTags("Owner Plan Catalog")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin)
            .AddEndpointFilter(RequireOwnerWhenRoleIsPresentAsync);

        group.MapGet("/plans", (
            OwnerPlanCatalogStubHandler handler) => ToResult(handler.ListPlans()))
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("TenantServiceOwnerListPlans")
            .WithSummary("Lists Owner Admin plan catalog from Tenant Service contract stub.");

        group.MapGet("/modules", (
            OwnerPlanCatalogStubHandler handler) => ToResult(handler.ListModules()))
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("TenantServiceOwnerListModules")
            .WithSummary("Lists Owner Admin module entitlement matrix from Tenant Service contract stub.");

        group.MapGet("/tenant-plan-assignments", (
            OwnerPlanCatalogStubHandler handler) => ToResult(handler.ListTenantPlanAssignments()))
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("TenantServiceOwnerListTenantPlanAssignments")
            .WithSummary("Lists cross-tenant plan assignments for Owner Super Admin.");

        group.MapPost("/tenant-plan-assignments/bulk-change", (
            BulkChangeTenantPlanRequest request,
            OwnerPlanCatalogStubHandler handler) => ToResult(handler.BulkChangeTenantPlans(request)))
            .RequirePermission(PermissionCodes.PlansWrite)
            .WithName("TenantServiceOwnerBulkChangeTenantPlans")
            .WithSummary("Accepts cross-tenant plan bulk-change in contract stub mode.");

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

    private static IResult ToResult<T>(Result<T> result)
    {
        return result.IsSuccess && result.Value is not null
            ? HttpResults.Ok(result.Value)
            : ToErrorResult(result.Error);
    }

    private static IResult ToErrorResult(Error error)
    {
        return error.Code switch
        {
            "plans.validation" => HttpResults.ValidationProblem(ToValidationDetails(error)),
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
