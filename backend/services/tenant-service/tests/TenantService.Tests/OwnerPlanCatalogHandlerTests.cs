using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.Contracts.Security;
using ClinicSaaS.Contracts.Tenancy;
using TenantService.Application.Plans;
using Xunit;

namespace TenantService.Tests;

/// <summary>
/// Unit test cho Owner Plan & Module Catalog handler khi đã chuyển sang repository persistence thật.
/// </summary>
public sealed class OwnerPlanCatalogHandlerTests
{
    /// <summary>
    /// Xác nhận handler đọc plan catalog qua repository port, không dùng static stub.
    /// </summary>
    [Fact]
    public async Task ListPlansAsync_ReturnsRepositoryPlans()
    {
        var repository = new FakeOwnerPlanCatalogRepository();
        var handler = new OwnerPlanCatalogHandler(repository, new FakeUserContextAccessor());

        var result = await handler.ListPlansAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { PlanCodes.Starter, PlanCodes.Growth, PlanCodes.Premium },
            result.Value!.Items.Select(plan => plan.Code));
        Assert.True(repository.ListPlansCalled);
    }

    /// <summary>
    /// Xác nhận handler đọc matrix module qua repository port.
    /// </summary>
    [Fact]
    public async Task ListModulesAsync_ReturnsRepositoryModules()
    {
        var repository = new FakeOwnerPlanCatalogRepository();
        var handler = new OwnerPlanCatalogHandler(repository, new FakeUserContextAccessor());

        var result = await handler.ListModulesAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.Items.Count);
        Assert.True(repository.ListModulesCalled);
    }

    /// <summary>
    /// Xác nhận bulk-change yêu cầu audit reason và chỉ nhận hiệu lực `next_renewal`.
    /// </summary>
    [Fact]
    public async Task BulkChangeTenantPlansAsync_MissingAuditReason_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogHandler(
            new FakeOwnerPlanCatalogRepository(),
            new FakeUserContextAccessor());
        var request = new BulkChangeTenantPlanRequest(
            [Guid.NewGuid().ToString()],
            PlanCodes.Premium,
            "next_renewal",
            string.Empty);

        var result = await handler.BulkChangeTenantPlansAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("auditReason", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change không nhận danh sách tenant rỗng để tránh thao tác cross-tenant mơ hồ.
    /// </summary>
    [Fact]
    public async Task BulkChangeTenantPlansAsync_EmptySelectedTenantIds_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogHandler(
            new FakeOwnerPlanCatalogRepository(),
            new FakeUserContextAccessor());
        var request = new BulkChangeTenantPlanRequest(
            [],
            PlanCodes.Premium,
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = await handler.BulkChangeTenantPlansAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("selectedTenantIds", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change chỉ nhận UUID tenant id do assignment API trả về.
    /// </summary>
    [Fact]
    public async Task BulkChangeTenantPlansAsync_InvalidTenantId_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogHandler(
            new FakeOwnerPlanCatalogRepository(),
            new FakeUserContextAccessor());
        var request = new BulkChangeTenantPlanRequest(
            ["tenant-river-eye"],
            PlanCodes.Premium,
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = await handler.BulkChangeTenantPlansAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("selectedTenantIds", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change chỉ nhận plan nằm trong catalog hiện tại.
    /// </summary>
    [Fact]
    public async Task BulkChangeTenantPlansAsync_InvalidTargetPlan_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogHandler(
            new FakeOwnerPlanCatalogRepository(),
            new FakeUserContextAccessor());
        var request = new BulkChangeTenantPlanRequest(
            [Guid.NewGuid().ToString()],
            "enterprise",
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = await handler.BulkChangeTenantPlansAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("targetPlan", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận bulk-change chưa cho đổi hiệu lực ngoài kỳ gia hạn tiếp theo.
    /// </summary>
    [Fact]
    public async Task BulkChangeTenantPlansAsync_UnsupportedEffectiveAt_ReturnsValidation()
    {
        var handler = new OwnerPlanCatalogHandler(
            new FakeOwnerPlanCatalogRepository(),
            new FakeUserContextAccessor());
        var request = new BulkChangeTenantPlanRequest(
            [Guid.NewGuid().ToString()],
            PlanCodes.Premium,
            "immediately",
            "Owner approved upgrade at renewal.");

        var result = await handler.BulkChangeTenantPlansAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("plans.validation", result.Error.Code);
        Assert.Contains("effectiveAt", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xác nhận request hợp lệ được chuyển xuống repository và actor user id được propagate.
    /// </summary>
    [Fact]
    public async Task BulkChangeTenantPlansAsync_ValidRequest_DelegatesToRepository()
    {
        var repository = new FakeOwnerPlanCatalogRepository();
        var tenantId = Guid.NewGuid().ToString();
        var handler = new OwnerPlanCatalogHandler(
            repository,
            new FakeUserContextAccessor("owner-1"));
        var request = new BulkChangeTenantPlanRequest(
            [tenantId],
            PlanCodes.Premium,
            "next_renewal",
            "Owner approved upgrade at renewal.");

        var result = await handler.BulkChangeTenantPlansAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("accepted-db", result.Value!.Status);
        Assert.Equal("owner-1", repository.LastActorUserId);
        Assert.Same(request, repository.LastBulkChangeRequest);
    }

    private sealed class FakeOwnerPlanCatalogRepository : IOwnerPlanCatalogRepository
    {
        public bool ListPlansCalled { get; private set; }

        public bool ListModulesCalled { get; private set; }

        public BulkChangeTenantPlanRequest? LastBulkChangeRequest { get; private set; }

        public string? LastActorUserId { get; private set; }

        public Task<OwnerPlanCatalogResponse> ListPlansAsync(CancellationToken cancellationToken)
        {
            ListPlansCalled = true;
            return Task.FromResult(new OwnerPlanCatalogResponse([
                new(PlanCodes.Starter, "Starter", 49, "Starter", 0, "info", false),
                new(PlanCodes.Growth, "Growth", 129, "Growth", 0, "neutral", true),
                new(PlanCodes.Premium, "Premium", 299, "Premium", 0, "warning", false)
            ]));
        }

        public Task<OwnerModuleCatalogResponse> ListModulesAsync(CancellationToken cancellationToken)
        {
            ListModulesCalled = true;
            return Task.FromResult(new OwnerModuleCatalogResponse(Enumerable.Range(1, 12)
                .Select(index => new OwnerModuleCatalogItemResponse(
                    $"module-{index}",
                    $"Module {index}",
                    "Platform",
                    true,
                    true,
                    true))
                .ToArray()));
        }

        public Task<TenantPlanAssignmentListResponse> ListTenantPlanAssignmentsAsync(CancellationToken cancellationToken)
            => Task.FromResult(new TenantPlanAssignmentListResponse([]));

        public Task<Result<BulkChangeTenantPlanResponse>> BulkChangeTenantPlansAsync(
            BulkChangeTenantPlanRequest request,
            string? actorUserId,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            LastBulkChangeRequest = request;
            LastActorUserId = actorUserId;
            return Task.FromResult(Result<BulkChangeTenantPlanResponse>.Success(new BulkChangeTenantPlanResponse(
                request.SelectedTenantIds.Count,
                170,
                "accepted-db",
                "Persisted.",
                request.EffectiveAt,
                request.AuditReason)));
        }
    }

    private sealed class FakeUserContextAccessor : IUserContextAccessor
    {
        public FakeUserContextAccessor(string? userId = null)
        {
            Current = new UserContext(
                userId,
                null,
                null,
                [],
                [],
                userId is not null,
                "test");
        }

        public UserContext Current { get; private set; }

        public void SetCurrent(UserContext userContext)
        {
            Current = userContext;
        }
    }
}
