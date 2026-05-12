using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Tenancy;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace ApiGateway.Api.Endpoints;

/// <summary>
/// API Gateway contract/stub routes cho Owner Plan & Module Catalog, phục vụ FE A8 mock-first.
/// </summary>
public static class OwnerPlanCatalogContractEndpoints
{
    private const string EffectiveAtNextRenewal = "next_renewal";

    private static readonly OwnerPlanCatalogItemResponse[] Plans =
    [
        new(PlanCodes.Starter, "Starter", 49, "Cho phong kham nho, toi da 2 bac si", 84, "info", false),
        new(PlanCodes.Growth, "Growth", 129, "Pho bien nhat cho phong kham 3-10 bac si", 132, "neutral", true),
        new(PlanCodes.Premium, "Premium", 299, "Phong kham lon, da chi nhanh va van hanh nang cao", 32, "warning", false)
    ];

    private static readonly OwnerModuleCatalogItemResponse[] Modules =
    [
        new("public-website", "Public Website", "Website", "limit-3-pages", "unlimited", "unlimited"),
        new("booking-online", "Booking Online", "Booking", "100/month", "1000/month", "unlimited"),
        new("doctor-schedule", "Doctor Schedule", "Clinic Ops", true, true, true),
        new("patient-records", "Patient Records", "Customer", false, true, true),
        new("apso-eprescribe", "APSO E-prescribe", "Records", false, false, true),
        new("custom-domain", "Custom domain", "Domain", false, true, true),
        new("ssl-auto-renew", "SSL auto-renew", "Domain", true, true, true),
        new("telehealth", "Telehealth", "Care", false, "limit-50/m", "unlimited"),
        new("reports-analytics", "Reports analytics", "Analytics", "basic", "advanced", "advanced + export"),
        new("multi-branch", "Multi-branch", "Operations", false, false, true),
        new("api-access", "API access", "Platform", false, false, true),
        new("priority-support", "Priority support", "Support", false, "email", "phone+email 24/7")
    ];

    private static readonly TenantPlanAssignmentResponse[] Assignments =
    [
        new("tenant-aurora-dental", "aurora-dental", PlanCodes.Premium, "Premium", 299, "01/06/2026", true, PlanCodes.Premium),
        new("tenant-river-eye", "river-eye", PlanCodes.Growth, "Growth", 129, "08/06/2026", true, PlanCodes.Premium),
        new("tenant-ndtp-pediatric", "ndtp-pediatric", PlanCodes.Premium, "Premium", 299, "03/06/2026", true, PlanCodes.Premium),
        new("tenant-nova-skin", "nova-skin", PlanCodes.Starter, "Starter", 49, "05/06/2026", false, PlanCodes.Growth),
        new("tenant-phuc-lam-ent", "phuc-lam-ent", PlanCodes.Growth, "Growth", 129, "28/05/2026", false, PlanCodes.Growth),
        new("tenant-hongduc-obgyn", "hongduc-obgyn", PlanCodes.Premium, "Premium", 299, "26/05/2026", false, PlanCodes.Growth)
    ];

    /// <summary>
    /// Map route contract `/api/owner/*` tại API Gateway cho FE `/plans`.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder của API Gateway.</param>
    /// <returns>Endpoint route builder sau khi map Owner Plan routes.</returns>
    public static IEndpointRouteBuilder MapOwnerPlanCatalogContractEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/owner")
            .WithTags("Owner Plan Catalog Contract")
            .AllowPlatformScope()
            .RequireRole(RoleNames.OwnerSuperAdmin)
            .AddEndpointFilter(RequireOwnerWhenRoleIsPresentAsync);

        group.MapGet("/plans", () => HttpResults.Ok(new OwnerPlanCatalogResponse(Plans)))
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("ApiGatewayOwnerListPlans")
            .WithSummary("Returns Owner Plan catalog contract stub through API Gateway.");

        group.MapGet("/modules", () => HttpResults.Ok(new OwnerModuleCatalogResponse(Modules)))
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("ApiGatewayOwnerListModules")
            .WithSummary("Returns Owner Module entitlement matrix contract stub through API Gateway.");

        group.MapGet("/tenant-plan-assignments", () => HttpResults.Ok(new TenantPlanAssignmentListResponse(Assignments)))
            .RequirePermission(PermissionCodes.PlansRead)
            .WithName("ApiGatewayOwnerListTenantPlanAssignments")
            .WithSummary("Returns Owner tenant plan assignment contract stub through API Gateway.");

        group.MapPost("/tenant-plan-assignments/bulk-change", (BulkChangeTenantPlanRequest request) =>
            ValidateBulkChangeRequest(request) is { } validation
                ? HttpResults.ValidationProblem(validation)
                : HttpResults.Ok(BuildBulkChangeResponse(request)))
            .RequirePermission(PermissionCodes.PlansWrite)
            .WithName("ApiGatewayOwnerBulkChangeTenantPlans")
            .WithSummary("Returns Owner tenant plan bulk-change contract stub through API Gateway.");

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

    private static BulkChangeTenantPlanResponse BuildBulkChangeResponse(BulkChangeTenantPlanRequest request)
    {
        var selectedIds = request.SelectedTenantIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetPrice = GetPlanPrice(request.TargetPlan);
        var selectedAssignments = Assignments
            .Where(assignment => selectedIds.Contains(assignment.Id))
            .ToArray();
        var changedCount = selectedAssignments.Length;
        var mrrDiff = selectedAssignments.Sum(assignment => targetPrice - assignment.CurrentMrr);
        var status = changedCount == 0 ? "noop" : "accepted-stub";
        var message = changedCount == 0
            ? "No matching tenants were found in the contract stub."
            : $"{changedCount} tenant plan changes accepted for next renewal.";

        return new BulkChangeTenantPlanResponse(
            changedCount,
            mrrDiff,
            status,
            message,
            EffectiveAtNextRenewal,
            request.AuditReason.Trim());
    }

    private static IDictionary<string, string[]>? ValidateBulkChangeRequest(BulkChangeTenantPlanRequest request)
    {
        var details = new Dictionary<string, string[]>();

        if (request.SelectedTenantIds is null || request.SelectedTenantIds.Count == 0)
        {
            details["selectedTenantIds"] = ["At least one tenant id is required."];
        }

        if (string.IsNullOrWhiteSpace(request.TargetPlan) || !PlanCodes.All.Contains(request.TargetPlan, StringComparer.Ordinal))
        {
            details["targetPlan"] = ["Target plan must be starter, growth or premium."];
        }

        if (!string.Equals(request.EffectiveAt, EffectiveAtNextRenewal, StringComparison.Ordinal))
        {
            details["effectiveAt"] = ["Only next_renewal is supported in this contract stub."];
        }

        if (string.IsNullOrWhiteSpace(request.AuditReason))
        {
            details["auditReason"] = ["Audit reason is required for cross-tenant plan changes."];
        }

        return details.Count == 0 ? null : details;
    }

    private static decimal GetPlanPrice(string planCode)
    {
        return Plans.First(plan => string.Equals(plan.Code, planCode, StringComparison.Ordinal)).Price;
    }
}
