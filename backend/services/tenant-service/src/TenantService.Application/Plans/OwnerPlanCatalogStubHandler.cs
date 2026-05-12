using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;

namespace TenantService.Application.Plans;

/// <summary>
/// Handler Application cho Owner Plan & Module Catalog contract/stub, chưa ghi PostgreSQL hoặc Billing Service.
/// </summary>
public sealed class OwnerPlanCatalogStubHandler
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
    /// Trả plan cards cho Owner Admin `/plans`.
    /// </summary>
    /// <returns>Kết quả chứa danh sách plan catalog stub.</returns>
    public Result<OwnerPlanCatalogResponse> ListPlans()
        => Result<OwnerPlanCatalogResponse>.Success(new OwnerPlanCatalogResponse(Plans));

    /// <summary>
    /// Trả matrix module entitlement theo từng plan.
    /// </summary>
    /// <returns>Kết quả chứa danh sách module entitlement stub.</returns>
    public Result<OwnerModuleCatalogResponse> ListModules()
        => Result<OwnerModuleCatalogResponse>.Success(new OwnerModuleCatalogResponse(Modules));

    /// <summary>
    /// Trả danh sách assignment plan hiện tại của tenant cho Owner Super Admin.
    /// </summary>
    /// <returns>Kết quả chứa assignment stub cho bảng bulk-change.</returns>
    public Result<TenantPlanAssignmentListResponse> ListTenantPlanAssignments()
        => Result<TenantPlanAssignmentListResponse>.Success(new TenantPlanAssignmentListResponse(Assignments));

    /// <summary>
    /// Nhận bulk-change plan ở chế độ contract/stub và tính MRR diff dự kiến.
    /// </summary>
    /// <param name="request">Payload bulk-change do Owner Admin gửi lên.</param>
    /// <returns>Kết quả chứa số tenant đổi plan, MRR diff và audit reason đã nhận.</returns>
    public Result<BulkChangeTenantPlanResponse> BulkChangeTenantPlans(BulkChangeTenantPlanRequest request)
    {
        var validation = ValidateBulkChangeRequest(request);
        if (validation is not null)
        {
            return Result<BulkChangeTenantPlanResponse>.Failure(validation);
        }

        var selectedIds = request.SelectedTenantIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetPrice = GetPlanPrice(request.TargetPlan);
        var selectedAssignments = Assignments
            .Where(assignment => selectedIds.Contains(assignment.Id))
            .ToArray();
        var mrrDiff = selectedAssignments.Sum(assignment => targetPrice - assignment.CurrentMrr);
        var changedCount = selectedAssignments.Length;
        var status = changedCount == 0 ? "noop" : "accepted-stub";
        var message = changedCount == 0
            ? "No matching tenants were found in the contract stub."
            : $"{changedCount} tenant plan changes accepted for next renewal.";

        return Result<BulkChangeTenantPlanResponse>.Success(new BulkChangeTenantPlanResponse(
            changedCount,
            mrrDiff,
            status,
            message,
            EffectiveAtNextRenewal,
            request.AuditReason.Trim()));
    }

    private static Error? ValidateBulkChangeRequest(BulkChangeTenantPlanRequest request)
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

        return details.Count == 0
            ? null
            : new Error("plans.validation", "Owner plan bulk-change request is invalid.", details);
    }

    private static decimal GetPlanPrice(string planCode)
    {
        return Plans.First(plan => string.Equals(plan.Code, planCode, StringComparison.Ordinal)).Price;
    }
}
