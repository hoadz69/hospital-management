using ClinicSaaS.Contracts.Tenancy;
using TenantService.Application.Plans;
using Xunit;

namespace TenantService.Tests;

/// <summary>
/// Unit test cho Owner Plan & Module Catalog contract/stub của Tenant Service.
/// </summary>
public sealed class OwnerPlanCatalogStubHandlerTests
{
    /// <summary>
    /// Xác nhận plan catalog trả đủ ba gói mà FE A8 đang mock-first.
    /// </summary>
    [Fact]
    public void ListPlans_ReturnsStarterGrowthPremium()
    {
        var handler = new OwnerPlanCatalogStubHandler();

        var result = handler.ListPlans();

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { PlanCodes.Starter, PlanCodes.Growth, PlanCodes.Premium },
            result.Value!.Items.Select(plan => plan.Code));
    }

    /// <summary>
    /// Xác nhận module matrix có đủ 12 row theo mock FE A8 để không lệch số dòng.
    /// </summary>
    [Fact]
    public void ListModules_ReturnsTwelveRows()
    {
        var handler = new OwnerPlanCatalogStubHandler();

        var result = handler.ListModules();

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.Items.Count);
    }

    /// <summary>
    /// Xác nhận bulk-change yêu cầu audit reason và chỉ nhận hiệu lực `next_renewal`.
    /// </summary>
    [Fact]
    public void BulkChangeTenantPlans_MissingAuditReason_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogStubHandler();
        var request = new BulkChangeTenantPlanRequest(
            ["tenant-river-eye"],
            PlanCodes.Premium,
            "next_renewal",
            string.Empty);

        var result = handler.BulkChangeTenantPlans(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("auditReason", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change không nhận danh sách tenant rỗng để tránh thao tác cross-tenant mơ hồ.
    /// </summary>
    [Fact]
    public void BulkChangeTenantPlans_EmptySelectedTenantIds_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogStubHandler();
        var request = new BulkChangeTenantPlanRequest(
            [],
            PlanCodes.Premium,
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = handler.BulkChangeTenantPlans(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("selectedTenantIds", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change chỉ nhận plan nằm trong catalog hiện tại.
    /// </summary>
    [Fact]
    public void BulkChangeTenantPlans_InvalidTargetPlan_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogStubHandler();
        var request = new BulkChangeTenantPlanRequest(
            ["tenant-river-eye"],
            "enterprise",
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = handler.BulkChangeTenantPlans(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("targetPlan", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change chưa cho đổi hiệu lực ngoài kỳ gia hạn tiếp theo.
    /// </summary>
    [Fact]
    public void BulkChangeTenantPlans_UnsupportedEffectiveAt_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogStubHandler();
        var request = new BulkChangeTenantPlanRequest(
            ["tenant-river-eye"],
            PlanCodes.Premium,
            "immediately",
            "Owner approved upgrade at renewal.");

        var result = handler.BulkChangeTenantPlans(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("effectiveAt", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change tính MRR diff dự kiến dựa trên current MRR và target plan.
    /// </summary>
    [Fact]
    public void BulkChangeTenantPlans_SelectedGrowthToPremium_ReturnsMrrDiff()
    {
        var handler = new OwnerPlanCatalogStubHandler();
        var request = new BulkChangeTenantPlanRequest(
            ["tenant-river-eye"],
            PlanCodes.Premium,
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = handler.BulkChangeTenantPlans(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.ChangedCount);
        Assert.Equal(170, result.Value.MrrDiff);
        Assert.Equal("accepted-stub", result.Value.Status);
    }
}
