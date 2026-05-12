using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.WebsiteCms;
using WebsiteCmsService.Application.Website;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace WebsiteCmsService.Api.Endpoints;

/// <summary>
/// Minimal API endpoints cho Website CMS contract/stub Wave A.
/// </summary>
public static class WebsiteCmsEndpoints
{
    /// <summary>
    /// Map endpoint settings, sliders, pages và publish history.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Website CMS Service API.</param>
    /// <returns>Endpoint route builder sau khi đã map Website CMS endpoints.</returns>
    public static IEndpointRouteBuilder MapWebsiteCmsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants/{tenantId:guid}/website")
            .WithTags("Website CMS")
            .RequireTenantContext()
            .RequireRole(RoleNames.ClinicAdmin);

        group.MapGet("/settings", (Guid tenantId, WebsiteCmsContractStubHandler handler) => ToResult(handler.GetSettings(tenantId)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("WebsiteCmsServiceGetSettings")
            .WithSummary("Gets website settings from contract stub.");

        group.MapPut("/settings", (
            Guid tenantId,
            UpsertWebsiteSettingsRequest request,
            WebsiteCmsContractStubHandler handler) => ToResult(handler.UpsertSettings(tenantId, request)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("WebsiteCmsServiceUpsertSettings")
            .WithSummary("Updates website settings in contract stub mode.");

        group.MapGet("/sliders", (Guid tenantId, WebsiteCmsContractStubHandler handler) => ToResult(handler.ListSliders(tenantId)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("WebsiteCmsServiceListSliders")
            .WithSummary("Lists homepage sliders from contract stub.");

        group.MapPost("/sliders", (
            Guid tenantId,
            UpsertSliderRequest request,
            WebsiteCmsContractStubHandler handler) => ToCreatedSliderResult(handler.UpsertSlider(tenantId, null, request)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("WebsiteCmsServiceCreateSlider")
            .WithSummary("Creates a homepage slider in contract stub mode.");

        group.MapPut("/sliders/{slideId:guid}", (
            Guid tenantId,
            Guid slideId,
            UpsertSliderRequest request,
            WebsiteCmsContractStubHandler handler) => ToResult(handler.UpsertSlider(tenantId, slideId, request)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("WebsiteCmsServiceUpdateSlider")
            .WithSummary("Updates a homepage slider in contract stub mode.");

        group.MapDelete("/sliders/{slideId:guid}", (
            Guid tenantId,
            Guid slideId,
            WebsiteCmsContractStubHandler handler) => ToResult(handler.DeleteSlider(tenantId, slideId)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("WebsiteCmsServiceDeleteSlider")
            .WithSummary("Disables a homepage slider in contract stub mode.");

        group.MapGet("/pages", (Guid tenantId, WebsiteCmsContractStubHandler handler) => ToResult(handler.ListPages(tenantId)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("WebsiteCmsServiceListPages")
            .WithSummary("Lists website pages from contract stub.");

        group.MapGet("/pages/{pageKey}", (
            Guid tenantId,
            string pageKey,
            WebsiteCmsContractStubHandler handler) => ToResult(handler.GetPage(tenantId, pageKey)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("WebsiteCmsServiceGetPage")
            .WithSummary("Gets one website page from contract stub.");

        group.MapPut("/pages/{pageKey}", (
            Guid tenantId,
            string pageKey,
            UpsertWebsitePageRequest request,
            WebsiteCmsContractStubHandler handler) => ToResult(handler.UpsertPage(tenantId, pageKey, request)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("WebsiteCmsServiceUpsertPage")
            .WithSummary("Updates one website page in contract stub mode.");

        group.MapPost("/publish", (Guid tenantId, WebsiteCmsContractStubHandler handler) => ToResult(handler.Publish(tenantId)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("WebsiteCmsServicePublishWebsite")
            .WithSummary("Queues website publish in contract stub mode.");

        group.MapGet("/publish-history", (Guid tenantId, WebsiteCmsContractStubHandler handler) => ToResult(handler.GetPublishHistory(tenantId)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("WebsiteCmsServiceGetPublishHistory")
            .WithSummary("Gets website publish history from contract stub.");

        return endpoints;
    }

    private static IResult ToCreatedSliderResult(Result<SliderResponse> result)
    {
        return result.IsSuccess && result.Value is not null
            ? HttpResults.Created($"/api/tenants/{result.Value.TenantId}/website/sliders/{result.Value.Id}", result.Value)
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
            "website_cms.validation" => HttpResults.ValidationProblem(ToValidationDetails(error)),
            "website_cms.not_found" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status404NotFound, title: "Website CMS resource not found"),
            "website_cms.tenant_mismatch" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden, title: "Tenant scope mismatch"),
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
