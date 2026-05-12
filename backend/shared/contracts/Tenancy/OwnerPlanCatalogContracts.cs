namespace ClinicSaaS.Contracts.Tenancy;

/// <summary>
/// Mã plan chuẩn dùng cho Owner Admin quản lý catalog gói dịch vụ và tenant assignment.
/// </summary>
public static class PlanCodes
{
    /// <summary>
    /// Gói cơ bản cho phòng khám nhỏ.
    /// </summary>
    public const string Starter = "starter";

    /// <summary>
    /// Gói tăng trưởng cho phòng khám vận hành nhiều module hơn.
    /// </summary>
    public const string Growth = "growth";

    /// <summary>
    /// Gói cao nhất cho phòng khám lớn hoặc đa chi nhánh.
    /// </summary>
    public const string Premium = "premium";

    /// <summary>
    /// Tập mã plan hợp lệ hiện tại.
    /// </summary>
    public static readonly string[] All =
    [
        Starter,
        Growth,
        Premium
    ];
}

/// <summary>
/// Một card plan trong Owner Admin Plan & Module Catalog.
/// </summary>
/// <param name="Code">Mã plan ổn định để FE chọn và gửi bulk-change.</param>
/// <param name="Name">Tên hiển thị của plan.</param>
/// <param name="Price">Giá MRR placeholder theo USD/tháng trong wave contract.</param>
/// <param name="Description">Mô tả ngắn giúp Owner Admin so sánh plan.</param>
/// <param name="TenantCount">Số tenant đang dùng plan trong dữ liệu stub.</param>
/// <param name="Tone">Tone UI tương ứng với mock FE hiện tại.</param>
/// <param name="Popular">Cho biết plan có được đánh dấu phổ biến hay không.</param>
public sealed record OwnerPlanCatalogItemResponse(
    string Code,
    string Name,
    decimal Price,
    string Description,
    int TenantCount,
    string Tone,
    bool Popular);

/// <summary>
/// Response danh sách plan catalog cho Owner Admin.
/// </summary>
/// <param name="Items">Danh sách plan trả về cho màn `/plans`.</param>
public sealed record OwnerPlanCatalogResponse(IReadOnlyList<OwnerPlanCatalogItemResponse> Items);

/// <summary>
/// Một dòng module entitlement trong matrix plan.
/// </summary>
/// <param name="Id">Mã module ổn định cho FE render row key.</param>
/// <param name="Name">Tên module hiển thị.</param>
/// <param name="Category">Nhóm module trong catalog.</param>
/// <param name="Starter">Entitlement của module trong gói Starter: bool hoặc limit string.</param>
/// <param name="Growth">Entitlement của module trong gói Growth: bool hoặc limit string.</param>
/// <param name="Premium">Entitlement của module trong gói Premium: bool hoặc limit string.</param>
public sealed record OwnerModuleCatalogItemResponse(
    string Id,
    string Name,
    string Category,
    object Starter,
    object Growth,
    object Premium);

/// <summary>
/// Response matrix module entitlement cho Owner Admin.
/// </summary>
/// <param name="Items">Danh sách module entitlement theo từng plan.</param>
public sealed record OwnerModuleCatalogResponse(IReadOnlyList<OwnerModuleCatalogItemResponse> Items);

/// <summary>
/// Snapshot plan assignment của một tenant trong Owner Admin.
/// </summary>
/// <param name="Id">Định danh tenant hoặc assignment dùng làm row key.</param>
/// <param name="Slug">Slug tenant hiển thị cho Owner Super Admin.</param>
/// <param name="CurrentPlan">Mã plan hiện tại.</param>
/// <param name="CurrentPlanName">Tên plan hiện tại.</param>
/// <param name="CurrentMrr">MRR hiện tại của tenant trong dữ liệu stub.</param>
/// <param name="NextRenewal">Ngày gia hạn kế tiếp ở dạng display string khớp FE mock.</param>
/// <param name="Selected">Trạng thái chọn mặc định trong màn mock-first.</param>
/// <param name="TargetPlan">Plan mục tiêu mặc định khi bulk-change.</param>
public sealed record TenantPlanAssignmentResponse(
    string Id,
    string Slug,
    string CurrentPlan,
    string CurrentPlanName,
    decimal CurrentMrr,
    string NextRenewal,
    bool Selected,
    string TargetPlan);

/// <summary>
/// Response danh sách tenant plan assignment cho Owner Admin.
/// </summary>
/// <param name="Items">Danh sách tenant assignment phục vụ bảng bulk-change.</param>
public sealed record TenantPlanAssignmentListResponse(IReadOnlyList<TenantPlanAssignmentResponse> Items);

/// <summary>
/// Request bulk-change plan cho nhiều tenant ở phạm vi Owner Super Admin.
/// </summary>
/// <param name="SelectedTenantIds">Danh sách tenant được chọn để đổi plan.</param>
/// <param name="TargetPlan">Plan mục tiêu cần áp dụng.</param>
/// <param name="EffectiveAt">Thời điểm hiệu lực placeholder; wave này hỗ trợ `next_renewal`.</param>
/// <param name="AuditReason">Lý do audit bắt buộc để không tạo thay đổi cross-tenant âm thầm.</param>
public sealed record BulkChangeTenantPlanRequest(
    IReadOnlyCollection<string> SelectedTenantIds,
    string TargetPlan,
    string EffectiveAt,
    string AuditReason);

/// <summary>
/// Response bulk-change plan cho Owner Admin.
/// </summary>
/// <param name="ChangedCount">Số tenant hợp lệ được nhận thay đổi trong stub.</param>
/// <param name="MrrDiff">Chênh lệch MRR dự kiến dựa trên target plan và assignment hiện tại.</param>
/// <param name="Status">Trạng thái xử lý placeholder.</param>
/// <param name="Message">Thông điệp ngắn an toàn để FE hiển thị.</param>
/// <param name="EffectiveAt">Thời điểm hiệu lực đã được normalize.</param>
/// <param name="AuditReason">Lý do audit caller gửi lên.</param>
public sealed record BulkChangeTenantPlanResponse(
    int ChangedCount,
    decimal MrrDiff,
    string Status,
    string Message,
    string EffectiveAt,
    string AuditReason);
