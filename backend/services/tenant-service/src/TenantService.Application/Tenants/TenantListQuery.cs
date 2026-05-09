using TenantService.Domain.Tenants;

namespace TenantService.Application.Tenants;

/// <summary>
/// Query filter cho danh sách tenant của Owner Super Admin.
/// </summary>
/// <param name="Status">Trạng thái tenant cần lọc; null nghĩa là không lọc theo trạng thái.</param>
/// <param name="Search">Từ khóa tìm kiếm đã trim; null nghĩa là không tìm kiếm text.</param>
/// <param name="Limit">Số bản ghi tối đa cần trả về.</param>
/// <param name="Offset">Vị trí bắt đầu trang trong tập kết quả.</param>
public sealed record TenantListQuery(
    TenantStatus? Status,
    string? Search,
    int Limit,
    int Offset);

/// <summary>
/// Trang kết quả tenant ở tầng Application trước khi map sang shared contract.
/// </summary>
/// <param name="Items">Danh sách tenant trong trang hiện tại.</param>
/// <param name="Total">Tổng số tenant khớp filter.</param>
/// <param name="Limit">Số bản ghi tối đa trong trang.</param>
/// <param name="Offset">Vị trí bắt đầu trang trong tập kết quả.</param>
public sealed record TenantListPage(
    IReadOnlyList<Tenant> Items,
    int Total,
    int Limit,
    int Offset);
