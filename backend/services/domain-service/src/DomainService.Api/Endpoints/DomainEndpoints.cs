using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Domains;
using DomainService.Application.Domains;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace DomainService.Api.Endpoints;

/// <summary>
/// Minimal API endpoints cho Domain Service contract/stub Wave A.
/// </summary>
public static class DomainEndpoints
{
    /// <summary>
    /// Map endpoint domain tenant-scoped phục vụ FE Wave A.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Domain Service API.</param>
    /// <returns>Endpoint route builder sau khi đã map domain endpoints.</returns>
    public static IEndpointRouteBuilder MapDomainEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants/{tenantId:guid}")
            .WithTags("Domains")
            .RequireTenantContext()
            .RequireRole(RoleNames.ClinicAdmin);

        group.MapGet("/domains", (
            Guid tenantId,
            DomainContractStubHandler handler) => ToResult(handler.ListDomains(tenantId)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("DomainServiceListDomains")
            .WithSummary("Lists tenant domains from Domain Service contract stub.");

        group.MapGet("/domains/{domainId:guid}", (
            Guid tenantId,
            Guid domainId,
            DomainContractStubHandler handler) => ToResult(handler.GetDomain(tenantId, domainId)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("DomainServiceGetDomain")
            .WithSummary("Gets one tenant domain from Domain Service contract stub.");

        group.MapPost("/domains", (
            Guid tenantId,
            RegisterDomainRequest request,
            DomainContractStubHandler handler) => ToCreatedResult(handler.RegisterDomain(tenantId, request)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("DomainServiceRegisterDomain")
            .WithSummary("Registers a tenant domain in contract stub mode.");

        group.MapPost("/domains/{domainId:guid}/verify", (
            Guid tenantId,
            Guid domainId,
            DomainContractStubHandler handler) => ToResult(handler.VerifyDomain(tenantId, domainId)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("DomainServiceVerifyDomain")
            .WithSummary("Starts dummy DNS verification for a tenant domain.");

        group.MapGet("/domains/{domainId:guid}/verify-status", (
            Guid tenantId,
            Guid domainId,
            DomainContractStubHandler handler) => ToResult(handler.VerifyDomain(tenantId, domainId)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("DomainServiceGetVerifyStatus")
            .WithSummary("Gets dummy DNS verification status for polling.");

        group.MapGet("/domains/{domainId:guid}/ssl-status", (
            Guid tenantId,
            Guid domainId,
            DomainContractStubHandler handler) => ToResult(handler.GetDomain(tenantId, domainId)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("DomainServiceGetSslStatus")
            .WithSummary("Gets dummy SSL status for a tenant domain.");

        group.MapPost("/publish", (
            Guid tenantId,
            DomainContractStubHandler handler) => ToResult(handler.Publish(tenantId)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("DomainServicePublishTenantWebsite")
            .WithSummary("Queues dummy website publish for tenant domain.");

        return endpoints;
    }

    private static IResult ToCreatedResult(Result<DomainResponse> result)
    {
        return result.IsSuccess && result.Value is not null
            ? HttpResults.Created($"/api/tenants/{result.Value.TenantId}/domains/{result.Value.Id}", result.Value)
            : ToErrorResult(result.Error);
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
            "domains.validation" => HttpResults.ValidationProblem(ToValidationDetails(error)),
            "domains.conflict" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status409Conflict, title: "Domain conflict"),
            "domains.not_found" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status404NotFound, title: "Domain not found"),
            "domains.tenant_mismatch" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden, title: "Tenant scope mismatch"),
            _ => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest, title: error.Code)
        };
    }

    private static IDictionary<string, string[]> ToValidationDetails(Error error)
    {
        return error.Details is null
            ? new Dictionary<string, string[]> { ["request"] = [error.Message] }
            : error.Details.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
