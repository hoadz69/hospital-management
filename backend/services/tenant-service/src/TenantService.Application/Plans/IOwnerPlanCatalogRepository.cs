using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;

namespace TenantService.Application.Plans;

/// <summary>
/// Port persistence cho Owner Plan & Module Catalog thuộc Tenant Service.
/// </summary>
public interface IOwnerPlanCatalogRepository
{
    /// <summary>
    /// Đọc danh sách plan catalog từ PostgreSQL.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Response plan catalog giữ nguyên contract FE hiện tại.</returns>
    Task<OwnerPlanCatalogResponse> ListPlansAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Đọc matrix module entitlement theo từng plan từ PostgreSQL.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Response module catalog giữ nguyên contract FE hiện tại.</returns>
    Task<OwnerModuleCatalogResponse> ListModulesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Đọc assignment plan hiện tại của tenant phục vụ Owner Super Admin bulk-change.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Danh sách assignment join từ tenant và bảng owner-plan persistence.</returns>
    Task<TenantPlanAssignmentListResponse> ListTenantPlanAssignmentsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Ghi bulk-change plan vào PostgreSQL trong một transaction và tạo audit rows.
    /// </summary>
    /// <param name="request">Payload đã qua validation application boundary.</param>
    /// <param name="actorUserId">Actor/user id nếu auth placeholder resolve được.</param>
    /// <param name="now">Thời điểm UTC dùng đồng nhất trong transaction.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả bulk-change hoặc lỗi not-found/conflict có cấu trúc.</returns>
    Task<Result<BulkChangeTenantPlanResponse>> BulkChangeTenantPlansAsync(
        BulkChangeTenantPlanRequest request,
        string? actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
