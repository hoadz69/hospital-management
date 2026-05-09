using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Tenancy;
using TenantService.Application.Tenants;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace TenantService.Api.Endpoints;

/// <summary>
/// Minimal API endpoints cho tenant lifecycle trong Tenant Service.
/// </summary>
public static class TenantEndpoints
{
    /// <summary>
    /// Map các endpoint tạo/list/get/update status tenant ở phạm vi platform.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Tenant Service API.</param>
    /// <returns>Endpoint route builder sau khi đã map nhóm `/api/tenants`.</returns>
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin);

        group.MapPost("/", async (
            CreateTenantRequest request,
            CreateTenantHandler handler,
            CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.IsSuccess && result.Value is not null
                    ? HttpResults.Created($"/api/tenants/{result.Value.Id}", result.Value)
                    : ToErrorResult(result.Error);
            })
            .RequirePermission(PermissionCodes.TenantsWrite)
            .WithName("TenantServiceCreateTenant")
            .WithSummary("Creates a tenant in Tenant Service.");

        group.MapGet("/", async (
            string? status,
            string? search,
            int? limit,
            int? offset,
            ListTenantsHandler handler,
            CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(status, search, limit, offset, cancellationToken);
                return result.IsSuccess && result.Value is not null
                    ? HttpResults.Ok(result.Value)
                    : ToErrorResult(result.Error);
            })
            .RequirePermission(PermissionCodes.TenantsRead)
            .WithName("TenantServiceListTenants")
            .WithSummary("Lists tenants for Owner Super Admin.");

        group.MapGet("/{tenantId:guid}", async (
            Guid tenantId,
            GetTenantByIdHandler handler,
            CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(tenantId, cancellationToken);
                return result.IsSuccess && result.Value is not null
                    ? HttpResults.Ok(result.Value)
                    : ToErrorResult(result.Error);
            })
            .RequirePermission(PermissionCodes.TenantsRead)
            .WithName("TenantServiceGetTenantById")
            .WithSummary("Gets one tenant by id.");

        group.MapPatch("/{tenantId:guid}/status", async (
            Guid tenantId,
            UpdateTenantStatusRequest request,
            UpdateTenantStatusHandler handler,
            CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(tenantId, request, cancellationToken);
                return result.IsSuccess && result.Value is not null
                    ? HttpResults.Ok(result.Value)
                    : ToErrorResult(result.Error);
            })
            .RequirePermission(PermissionCodes.TenantsWrite)
            .WithName("TenantServiceUpdateTenantStatus")
            .WithSummary("Updates tenant status without billing/domain side effects.");

        return endpoints;
    }

    private static IResult ToErrorResult(Error error)
    {
        return error.Code switch
        {
            "tenants.validation" => HttpResults.ValidationProblem(ToValidationDetails(error)),
            "tenants.conflict" => HttpResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Tenant conflict"),
            "tenants.not_found" => HttpResults.Problem(
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Tenant not found"),
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
