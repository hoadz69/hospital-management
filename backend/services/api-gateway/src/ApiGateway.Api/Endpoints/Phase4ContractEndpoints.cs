using ApiGateway.Application.Tenants;
using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Domains;
using ClinicSaaS.Contracts.Templates;
using ClinicSaaS.Contracts.WebsiteCms;
using ClinicSaaS.Observability.Correlation;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace ApiGateway.Api.Endpoints;

/// <summary>
/// API Gateway contract/stub routes cho Phase 4 Wave A, giúp FE ghép endpoint shape trước khi forward thật.
/// </summary>
public static class Phase4ContractEndpoints
{
    private static readonly Guid GatewayDomainId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly Guid GatewaySliderId = Guid.Parse("55555555-5555-4555-8555-555555555555");

    /// <summary>
    /// Map Domain, Template và Website CMS contract routes tại gateway.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của API Gateway.</param>
    /// <returns>Endpoint route builder sau khi map các route contract Phase 4.</returns>
    public static IEndpointRouteBuilder MapPhase4ContractEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapDomainContract(endpoints);
        MapTemplateContract(endpoints);
        MapWebsiteCmsContract(endpoints);

        return endpoints;
    }

    private static void MapDomainContract(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants/{tenantId:guid}")
            .WithTags("Phase 4 Domain Contract")
            .RequireTenantContext()
            .RequireRole(RoleNames.OwnerSuperAdmin)
            .AddEndpointFilter(EnsureTenantRouteMatchesContextAsync)
            .AddEndpointFilter(RequireOwnerWhenRoleIsPresentAsync);

        group.MapGet("/domains", async (
            Guid tenantId,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.ListTenantDomainDnsSslStatesAsync(
                    tenantId,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("ApiGatewayPhase4ListDomains")
            .WithSummary("Forwards tenant domain DNS/SSL state requests to Tenant Service.");

        group.MapGet("/domains/{domainId:guid}", (Guid tenantId, Guid domainId) => HttpResults.Ok(BuildDomain(tenantId, domainId)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("ApiGatewayPhase4GetDomain")
            .WithSummary("Returns one Domain Service contract stub through API Gateway.");

        group.MapPost("/domains", (Guid tenantId, RegisterDomainRequest request) =>
            HttpResults.Created($"/api/tenants/{tenantId}/domains/{GatewayDomainId}", BuildDomain(
                tenantId,
                GatewayDomainId,
                string.IsNullOrWhiteSpace(request.DomainName) ? null : request.DomainName.Trim(),
                request.IsPrimary)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("ApiGatewayPhase4RegisterDomain")
            .WithSummary("Returns registered domain contract stub through API Gateway.");

        group.MapPost("/domains/{domainId:guid}/verify", (Guid tenantId, Guid domainId) =>
            HttpResults.Ok(BuildVerification(domainId)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("ApiGatewayPhase4VerifyDomain")
            .WithSummary("Returns dummy DNS verify status through API Gateway.");

        group.MapGet("/domains/{domainId:guid}/verify-status", (Guid tenantId, Guid domainId) =>
            HttpResults.Ok(BuildVerification(domainId)))
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("ApiGatewayPhase4GetDomainVerifyStatus")
            .WithSummary("Returns dummy DNS verify polling status through API Gateway.");

        group.MapPost("/domains/{domainId:guid}/dns-retry", async (
            Guid tenantId,
            Guid domainId,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.RetryTenantDomainDnsAsync(
                    tenantId,
                    domainId,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("ApiGatewayPhase4RetryDomainDns")
            .WithSummary("Forwards DNS retry requests to Tenant Service persistence.");

        group.MapGet("/domains/{domainId:guid}/ssl-status", async (
            Guid tenantId,
            Guid domainId,
            ITenantServiceClient tenantServiceClient,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                using var response = await tenantServiceClient.GetTenantDomainSslStatusAsync(
                    tenantId,
                    domainId,
                    GetCorrelationId(httpContext),
                    cancellationToken);

                return await ToGatewayResultAsync(response, httpContext, cancellationToken);
            })
            .RequirePermission(PermissionCodes.DomainsRead)
            .WithName("ApiGatewayPhase4GetDomainSslStatus")
            .WithSummary("Forwards tenant domain SSL status requests to Tenant Service.");

        group.MapPost("/publish", (Guid tenantId) => HttpResults.Ok(new DomainPublishResponse(
            tenantId,
            "queued",
            BuildDomainName(tenantId),
            DateTimeOffset.UtcNow)))
            .RequirePermission(PermissionCodes.DomainsWrite)
            .WithName("ApiGatewayPhase4PublishDomain")
            .WithSummary("Returns dummy publish status through API Gateway.");
    }

    private static void MapTemplateContract(IEndpointRouteBuilder endpoints)
    {
        var templates = endpoints.MapGroup("/api/templates")
            .WithTags("Phase 4 Template Contract")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin);

        templates.MapGet("/", () => HttpResults.Ok(new TemplateListResponse([BuildTemplateSummary()])))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("ApiGatewayPhase4ListTemplates")
            .WithSummary("Returns Template Service registry contract stub through API Gateway.");

        templates.MapGet("/{templateKey}", (string templateKey) => HttpResults.Ok(BuildTemplateDetail(templateKey)))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("ApiGatewayPhase4GetTemplate")
            .WithSummary("Returns Template Service detail contract stub through API Gateway.");

        var tenantTemplates = endpoints.MapGroup("/api/tenants/{tenantId:guid}/template")
            .WithTags("Phase 4 Template Contract")
            .RequireTenantContext()
            .RequireRole(RoleNames.ClinicAdmin)
            .AddEndpointFilter(EnsureTenantRouteMatchesContextAsync);

        tenantTemplates.MapPost("/apply", (Guid tenantId, ApplyTemplateRequest request) => HttpResults.Ok(new TemplateApplyResponse(
            tenantId,
            request.TemplateKey,
            request.Mode,
            DateTimeOffset.UtcNow,
            request.RequestedBy,
            "accepted-stub")))
            .RequirePermission(PermissionCodes.TemplatesWrite)
            .WithName("ApiGatewayPhase4ApplyTemplate")
            .WithSummary("Returns Template Service apply contract stub through API Gateway.");

        tenantTemplates.MapGet("/active", (Guid tenantId) => HttpResults.Ok(new ActiveTemplateResponse(
            tenantId,
            "dental",
            "style-only",
            DateTimeOffset.UtcNow.AddDays(-1))))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("ApiGatewayPhase4GetActiveTemplate")
            .WithSummary("Returns active template contract stub through API Gateway.");

        tenantTemplates.MapPost("/preview-diff", (Guid tenantId, ApplyTemplateRequest request) => HttpResults.Ok(new TemplateDiffResponse(
            tenantId,
            request.TemplateKey,
            request.Mode,
            request.Mode == "content-only" ? ["homepageSections", "sampleCopy"] : ["brandColors", "themeTokens", "homepageSections"])))
            .RequirePermission(PermissionCodes.TemplatesRead)
            .WithName("ApiGatewayPhase4PreviewTemplateDiff")
            .WithSummary("Returns Template Service diff contract stub through API Gateway.");
    }

    private static void MapWebsiteCmsContract(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tenants/{tenantId:guid}/website")
            .WithTags("Phase 4 Website CMS Contract")
            .RequireTenantContext()
            .RequireRole(RoleNames.ClinicAdmin)
            .AddEndpointFilter(EnsureTenantRouteMatchesContextAsync);

        group.MapGet("/settings", (Guid tenantId) => HttpResults.Ok(BuildSettings(tenantId)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("ApiGatewayPhase4GetWebsiteSettings")
            .WithSummary("Returns Website CMS settings contract stub through API Gateway.");

        group.MapPut("/settings", (Guid tenantId, UpsertWebsiteSettingsRequest request) => HttpResults.Ok(new WebsiteSettingsResponse(
            tenantId,
            request.ThemeKey,
            request.LogoUrl,
            request.FaviconUrl,
            request.BrandColors,
            request.ContactInfo,
            request.SocialLinks,
            request.SeoMeta,
            request.CustomDomain,
            DateTimeOffset.UtcNow)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("ApiGatewayPhase4UpsertWebsiteSettings")
            .WithSummary("Returns Website CMS settings update contract stub through API Gateway.");

        group.MapGet("/sliders", (Guid tenantId) => HttpResults.Ok(new SliderListResponse([BuildSlider(tenantId, GatewaySliderId)])))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("ApiGatewayPhase4ListSliders")
            .WithSummary("Returns Website CMS sliders contract stub through API Gateway.");

        group.MapPost("/sliders", (Guid tenantId, UpsertSliderRequest request) =>
            HttpResults.Created($"/api/tenants/{tenantId}/website/sliders/{GatewaySliderId}", BuildSlider(tenantId, GatewaySliderId, request)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("ApiGatewayPhase4CreateSlider")
            .WithSummary("Returns Website CMS slider create contract stub through API Gateway.");

        group.MapPut("/sliders/{slideId:guid}", (Guid tenantId, Guid slideId, UpsertSliderRequest request) => HttpResults.Ok(BuildSlider(tenantId, slideId, request)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("ApiGatewayPhase4UpdateSlider")
            .WithSummary("Returns Website CMS slider update contract stub through API Gateway.");

        group.MapDelete("/sliders/{slideId:guid}", (Guid tenantId, Guid slideId) => HttpResults.Ok(BuildSlider(tenantId, slideId) with { IsEnabled = false }))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("ApiGatewayPhase4DeleteSlider")
            .WithSummary("Returns Website CMS slider delete contract stub through API Gateway.");

        group.MapGet("/pages", (Guid tenantId) => HttpResults.Ok(new WebsitePageListResponse([BuildPage(tenantId, "home"), BuildPage(tenantId, "about")])))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("ApiGatewayPhase4ListPages")
            .WithSummary("Returns Website CMS pages contract stub through API Gateway.");

        group.MapGet("/pages/{pageKey}", (Guid tenantId, string pageKey) => HttpResults.Ok(BuildPage(tenantId, pageKey)))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("ApiGatewayPhase4GetPage")
            .WithSummary("Returns Website CMS page contract stub through API Gateway.");

        group.MapPut("/pages/{pageKey}", (Guid tenantId, string pageKey, UpsertWebsitePageRequest request) => HttpResults.Ok(new WebsitePageResponse(
            tenantId,
            pageKey,
            request.Sections,
            request.PublishStatus,
            DateTimeOffset.UtcNow)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("ApiGatewayPhase4UpsertPage")
            .WithSummary("Returns Website CMS page update contract stub through API Gateway.");

        group.MapPost("/publish", (Guid tenantId) => HttpResults.Ok(new WebsitePublishResponse(
            tenantId,
            "queued",
            $"draft-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            DateTimeOffset.UtcNow)))
            .RequirePermission(PermissionCodes.WebsiteCmsWrite)
            .WithName("ApiGatewayPhase4PublishWebsiteCms")
            .WithSummary("Returns Website CMS publish contract stub through API Gateway.");

        group.MapGet("/publish-history", () => HttpResults.Ok(new WebsitePublishHistoryResponse(
            [new WebsitePublishHistoryItem("draft-202605100001", "published", DateTimeOffset.UtcNow.AddDays(-1), "owner-admin")])))
            .RequirePermission(PermissionCodes.WebsiteCmsRead)
            .WithName("ApiGatewayPhase4GetPublishHistory")
            .WithSummary("Returns Website CMS publish history contract stub through API Gateway.");
    }

    private static DomainResponse BuildDomain(Guid tenantId, Guid domainId, string? domainName = null, bool isPrimary = true)
    {
        var resolvedDomain = string.IsNullOrWhiteSpace(domainName) ? BuildDomainName(tenantId) : domainName;

        return new DomainResponse(
            domainId,
            tenantId,
            resolvedDomain,
            resolvedDomain.Trim().TrimEnd('.').ToLowerInvariant(),
            "DefaultSubdomain",
            "Pending",
            isPrimary,
            "Pending",
            "cname.clinicos.local",
            null,
            DateTimeOffset.UtcNow,
            "Gateway contract stub: Domain Service forwarding is pending.");
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

    private static DomainVerificationResponse BuildVerification(Guid domainId)
    {
        return new DomainVerificationResponse(
            domainId,
            "Pending",
            "Pending",
            DateTimeOffset.UtcNow.AddMinutes(2),
            "Gateway contract stub accepted DNS verify request.");
    }

    private static string BuildDomainName(Guid tenantId) => $"{tenantId.ToString("N")[..8]}.clinicos.local";

    private static TemplateSummaryResponse BuildTemplateSummary()
    {
        return new TemplateSummaryResponse(
            "dental",
            "Dental Care",
            "Dental",
            "/assets/templates/dental-preview.webp",
            ["full", "style-only", "content-only"]);
    }

    private static TemplateDetailResponse BuildTemplateDetail(string templateKey)
    {
        return new TemplateDetailResponse(
            templateKey,
            templateKey.Equals("dental", StringComparison.OrdinalIgnoreCase) ? "Dental Care" : "General Clinic",
            templateKey.Equals("dental", StringComparison.OrdinalIgnoreCase) ? "Dental" : "General",
            "Gateway contract stub for template preview.",
            $"/assets/templates/{templateKey}-preview.webp",
            ["full", "style-only", "content-only"],
            ["heroSlider", "services", "doctors"],
            new Dictionary<string, string>
            {
                ["primary"] = "#0E7C86",
                ["secondary"] = "#2563EB",
                ["radius"] = "8px"
            });
    }

    private static WebsiteSettingsResponse BuildSettings(Guid tenantId)
    {
        return new WebsiteSettingsResponse(
            tenantId,
            "dental",
            "/assets/demo/logo.svg",
            "/assets/demo/favicon.ico",
            new Dictionary<string, string>
            {
                ["primary"] = "#0E7C86",
                ["secondary"] = "#2563EB"
            },
            new WebsiteContactInfo("1900-0000", "hello@demo.clinicos.local", "123 Clinic Street", null),
            new Dictionary<string, string> { ["facebook"] = "https://social.example/clinic" },
            new WebsiteSeoMeta("Demo Clinic", "Gateway contract stub for Website CMS.", ["clinic", "healthcare"]),
            "demo.clinicos.local",
            DateTimeOffset.UtcNow);
    }

    private static SliderResponse BuildSlider(Guid tenantId, Guid slideId, UpsertSliderRequest? request = null)
    {
        return new SliderResponse(
            slideId,
            tenantId,
            request?.Title ?? "Chăm sóc sức khỏe toàn diện",
            request?.Subtitle ?? "Đặt lịch nhanh với đội ngũ bác sĩ giàu kinh nghiệm.",
            request?.ImageUrl ?? "/assets/demo/slider-1.webp",
            request?.CtaText ?? "Đặt lịch",
            request?.CtaUrl ?? "/booking",
            request?.Order ?? 1,
            request?.IsEnabled ?? true);
    }

    private static WebsitePageResponse BuildPage(Guid tenantId, string pageKey)
    {
        return new WebsitePageResponse(
            tenantId,
            pageKey,
            [new WebsitePageSection("hero", "HeroPrimary", new Dictionary<string, string> { ["title"] = "Demo Clinic" })],
            "draft",
            DateTimeOffset.UtcNow);
    }
}
