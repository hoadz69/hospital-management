using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Templates;
using TemplateService.Application.Templates;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace TemplateService.Api.Endpoints;

/// <summary>
/// Minimal API endpoints cho Template Service contract/stub Wave A.
/// </summary>
public static class TemplateEndpoints
{
    /// <summary>
    /// Map endpoint template registry và apply mode.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của Template Service API.</param>
    /// <returns>Endpoint route builder sau khi đã map template endpoints.</returns>
    public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var templates = endpoints.MapGroup("/api/templates")
            .WithTags("Templates")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin);

        templates.MapGet("/", (TemplateContractStubHandler handler) => HttpResults.Ok(handler.ListTemplates()))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("TemplateServiceListTemplates")
            .WithSummary("Lists template registry from Template Service contract stub.");

        templates.MapGet("/{templateKey}", (
            string templateKey,
            TemplateContractStubHandler handler) => ToResult(handler.GetTemplate(templateKey)))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("TemplateServiceGetTemplate")
            .WithSummary("Gets template detail from Template Service contract stub.");

        var tenantTemplates = endpoints.MapGroup("/api/tenants/{tenantId:guid}/template")
            .WithTags("Templates")
            .RequireTenantContext()
            .RequireRole(RoleNames.ClinicAdmin);

        tenantTemplates.MapPost("/apply", (
            Guid tenantId,
            ApplyTemplateRequest request,
            TemplateContractStubHandler handler) => ToResult(handler.ApplyTemplate(tenantId, request)))
            .RequirePermission(PermissionCodes.TemplatesWrite)
            .WithName("TemplateServiceApplyTemplate")
            .WithSummary("Applies a template in contract stub mode.");

        tenantTemplates.MapGet("/active", (
            Guid tenantId,
            TemplateContractStubHandler handler) => ToResult(handler.GetActiveTemplate(tenantId)))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("TemplateServiceGetActiveTemplate")
            .WithSummary("Gets active tenant template from contract stub.");

        tenantTemplates.MapPost("/preview-diff", (
            Guid tenantId,
            ApplyTemplateRequest request,
            TemplateContractStubHandler handler) => ToResult(handler.PreviewDiff(tenantId, request)))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("TemplateServicePreviewTemplateDiff")
            .WithSummary("Previews template apply diff in contract stub mode.");

        return endpoints;
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
            "templates.validation" => HttpResults.ValidationProblem(ToValidationDetails(error)),
            "templates.not_found" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status404NotFound, title: "Template not found"),
            "templates.tenant_mismatch" => HttpResults.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden, title: "Tenant scope mismatch"),
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
