using ClinicSaaS.BuildingBlocks.Results;
using TenantService.Domain.Tenants;

namespace TenantService.Application.Tenants;

/// <summary>
/// Port persistence cho tenant lifecycle; implementation phải giữ transaction boundary cho multi-table write.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Lưu tenant mới cùng profile, domain và modules trong cùng transaction.
    /// </summary>
    /// <param name="tenant">Aggregate tenant đã được domain factory dựng và chuẩn hóa.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa tenant đã lưu hoặc lỗi conflict khi slug/domain bị trùng.</returns>
    Task<Result<Tenant>> CreateAsync(Tenant tenant, CancellationToken cancellationToken);

    /// <summary>
    /// Đọc tenant đầy đủ theo id.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần đọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Tenant nếu tồn tại; null nếu không tìm thấy.</returns>
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Liệt kê tenant theo filter platform-scoped.
    /// </summary>
    /// <param name="query">Filter và phân trang đã validate ở Application layer.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Trang tenant khớp filter.</returns>
    Task<TenantListPage> ListAsync(TenantListQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Cập nhật trạng thái vòng đời tenant.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần cập nhật.</param>
    /// <param name="status">Trạng thái mới đã parse hợp lệ.</param>
    /// <param name="now">Thời điểm cập nhật theo UTC để ghi các mốc lifecycle.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa tenant sau cập nhật hoặc lỗi not found.</returns>
    Task<Result<Tenant>> UpdateStatusAsync(Guid tenantId, TenantStatus status, DateTimeOffset now, CancellationToken cancellationToken);
}
