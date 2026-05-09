namespace ClinicSaaS.Contracts.Tenancy;

/// <summary>
/// Yêu cầu tạo tenant/phòng khám từ Owner Super Admin.
/// </summary>
/// <param name="Slug">Slug định danh tenant, dùng để tạo subdomain mặc định và lookup quản trị.</param>
/// <param name="DisplayName">Tên hiển thị của tenant trên màn quản trị platform.</param>
/// <param name="PlanCode">Mã gói dịch vụ được gán tại thời điểm tạo tenant.</param>
/// <param name="PlanDisplayName">Tên hiển thị của gói dịch vụ nếu caller muốn lưu snapshot.</param>
/// <param name="ClinicName">Tên phòng khám trong hồ sơ tenant.</param>
/// <param name="ContactEmail">Email liên hệ của phòng khám nếu có.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ của phòng khám nếu có.</param>
/// <param name="AddressLine">Địa chỉ hiển thị của phòng khám nếu có.</param>
/// <param name="Specialty">Chuyên khoa chính của phòng khám nếu có.</param>
/// <param name="DefaultDomainName">Domain/subdomain mặc định; nếu bỏ trống Tenant Service dùng slug.</param>
/// <param name="ModuleCodes">Danh sách module cần bật ban đầu; nếu bỏ trống dùng module mặc định của platform.</param>
public sealed record CreateTenantRequest(
    string Slug,
    string DisplayName,
    string PlanCode,
    string? PlanDisplayName,
    string ClinicName,
    string? ContactEmail,
    string? PhoneNumber,
    string? AddressLine,
    string? Specialty,
    string? DefaultDomainName,
    IReadOnlyCollection<string>? ModuleCodes);

/// <summary>
/// Yêu cầu đổi trạng thái vòng đời tenant.
/// </summary>
/// <param name="Status">Trạng thái mới: Draft, Active, Suspended hoặc Archived.</param>
public sealed record UpdateTenantStatusRequest(string Status);

/// <summary>
/// Thông tin hồ sơ phòng khám gắn với tenant.
/// </summary>
/// <param name="ClinicName">Tên phòng khám hiển thị trong quản trị tenant.</param>
/// <param name="ContactEmail">Email liên hệ của phòng khám.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ của phòng khám.</param>
/// <param name="AddressLine">Địa chỉ phòng khám.</param>
/// <param name="Specialty">Chuyên khoa chính của phòng khám.</param>
public sealed record TenantProfileResponse(
    string ClinicName,
    string? ContactEmail,
    string? PhoneNumber,
    string? AddressLine,
    string? Specialty);

/// <summary>
/// Thông tin domain hoặc subdomain đang gắn với tenant.
/// </summary>
/// <param name="Id">Định danh domain record.</param>
/// <param name="DomainName">Domain hiển thị mà owner nhập hoặc hệ thống tạo.</param>
/// <param name="NormalizedDomainName">Domain đã chuẩn hóa để lookup/unique không lệch hoa thường.</param>
/// <param name="DomainType">Loại domain: DefaultSubdomain hoặc Custom.</param>
/// <param name="Status">Trạng thái domain trong vòng đời Tenant MVP.</param>
/// <param name="IsPrimary">Cho biết domain này là domain chính của tenant.</param>
/// <param name="CreatedAtUtc">Thời điểm domain được gắn vào tenant.</param>
/// <param name="VerifiedAtUtc">Thời điểm domain được xác minh, nếu đã xác minh.</param>
public sealed record TenantDomainResponse(
    string Id,
    string DomainName,
    string NormalizedDomainName,
    string DomainType,
    string Status,
    bool IsPrimary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? VerifiedAtUtc);

/// <summary>
/// Module được bật/tắt theo tenant trong giai đoạn MVP.
/// </summary>
/// <param name="ModuleCode">Mã module được bật/tắt cho tenant.</param>
/// <param name="IsEnabled">Cho biết module đang bật hay tắt.</param>
/// <param name="SourcePlanCode">Mã gói là nguồn bật module, nếu có.</param>
/// <param name="CreatedAtUtc">Thời điểm module được tạo cho tenant.</param>
/// <param name="UpdatedAtUtc">Thời điểm module được cập nhật gần nhất.</param>
public sealed record TenantModuleResponse(
    string ModuleCode,
    bool IsEnabled,
    string? SourcePlanCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

/// <summary>
/// Response chi tiết tenant dùng cho quản trị platform.
/// </summary>
/// <param name="Id">Định danh tenant.</param>
/// <param name="Slug">Slug định danh tenant.</param>
/// <param name="DisplayName">Tên hiển thị tenant.</param>
/// <param name="Status">Trạng thái vòng đời tenant.</param>
/// <param name="PlanCode">Mã gói dịch vụ đang lưu trên tenant.</param>
/// <param name="PlanDisplayName">Tên hiển thị của gói dịch vụ nếu có.</param>
/// <param name="Profile">Hồ sơ phòng khám thuộc tenant.</param>
/// <param name="Domains">Danh sách domain/subdomain gắn với tenant.</param>
/// <param name="Modules">Danh sách module đang cấu hình cho tenant.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo tenant.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật tenant gần nhất.</param>
/// <param name="ActivatedAtUtc">Thời điểm tenant được active lần đầu nếu có.</param>
/// <param name="SuspendedAtUtc">Thời điểm tenant bị suspend gần nhất nếu có.</param>
/// <param name="ArchivedAtUtc">Thời điểm tenant được archive nếu có.</param>
public sealed record TenantResponse(
    string Id,
    string Slug,
    string DisplayName,
    string Status,
    string PlanCode,
    string? PlanDisplayName,
    TenantProfileResponse Profile,
    IReadOnlyList<TenantDomainResponse> Domains,
    IReadOnlyList<TenantModuleResponse> Modules,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? SuspendedAtUtc,
    DateTimeOffset? ArchivedAtUtc);

/// <summary>
/// Response rút gọn cho danh sách tenant.
/// </summary>
/// <param name="Id">Định danh tenant.</param>
/// <param name="Slug">Slug định danh tenant.</param>
/// <param name="DisplayName">Tên hiển thị tenant.</param>
/// <param name="Status">Trạng thái vòng đời tenant.</param>
/// <param name="PlanCode">Mã gói dịch vụ đang lưu trên tenant.</param>
/// <param name="ClinicName">Tên phòng khám của tenant.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo tenant.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật tenant gần nhất.</param>
public sealed record TenantSummaryResponse(
    string Id,
    string Slug,
    string DisplayName,
    string Status,
    string PlanCode,
    string? ClinicName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

/// <summary>
/// Trang kết quả danh sách tenant có phân trang đơn giản.
/// </summary>
/// <param name="Items">Danh sách tenant trong trang hiện tại.</param>
/// <param name="Total">Tổng số tenant khớp filter.</param>
/// <param name="Limit">Số bản ghi tối đa được trả về.</param>
/// <param name="Offset">Vị trí bắt đầu của trang trong tập kết quả.</param>
public sealed record TenantListResponse(
    IReadOnlyList<TenantSummaryResponse> Items,
    int Total,
    int Limit,
    int Offset);

/// <summary>
/// Mã trạng thái tenant được chia sẻ giữa API Gateway và Tenant Service.
/// </summary>
public static class TenantStatusCodes
{
    /// <summary>
    /// Tenant mới tạo, chưa active vận hành.
    /// </summary>
    public const string Draft = "Draft";

    /// <summary>
    /// Tenant đang hoạt động.
    /// </summary>
    public const string Active = "Active";

    /// <summary>
    /// Tenant bị tạm ngưng.
    /// </summary>
    public const string Suspended = "Suspended";

    /// <summary>
    /// Tenant đã lưu trữ và không còn vận hành.
    /// </summary>
    public const string Archived = "Archived";

    public static readonly string[] All =
    [
        Draft,
        Active,
        Suspended,
        Archived
    ];
}
