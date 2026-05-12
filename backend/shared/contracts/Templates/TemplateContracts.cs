namespace ClinicSaaS.Contracts.Templates;

/// <summary>
/// Request áp template cho website tenant theo một trong ba mode đã chốt.
/// </summary>
/// <param name="TemplateKey">Khóa template trong registry toàn platform.</param>
/// <param name="Mode">Mode apply: `full`, `style-only` hoặc `content-only`.</param>
/// <param name="RequestedBy">Actor/user thực hiện apply, dùng cho audit sau này.</param>
public sealed record ApplyTemplateRequest(string TemplateKey, string Mode, string? RequestedBy);

/// <summary>
/// Response tóm tắt template trong thư viện global.
/// </summary>
/// <param name="TemplateKey">Khóa template ổn định cho API/FE.</param>
/// <param name="Name">Tên template hiển thị.</param>
/// <param name="Specialty">Chuyên khoa/phân khúc phù hợp.</param>
/// <param name="PreviewImageUrl">URL ảnh preview placeholder.</param>
/// <param name="SupportedModes">Các mode apply được hỗ trợ.</param>
public sealed record TemplateSummaryResponse(
    string TemplateKey,
    string Name,
    string Specialty,
    string PreviewImageUrl,
    IReadOnlyList<string> SupportedModes);

/// <summary>
/// Response chi tiết template để FE preview trước khi apply.
/// </summary>
/// <param name="TemplateKey">Khóa template ổn định cho API/FE.</param>
/// <param name="Name">Tên template hiển thị.</param>
/// <param name="Specialty">Chuyên khoa/phân khúc phù hợp.</param>
/// <param name="Description">Mô tả ngắn cho dialog chọn template.</param>
/// <param name="PreviewImageUrl">URL ảnh preview placeholder.</param>
/// <param name="SupportedModes">Các mode apply được hỗ trợ.</param>
/// <param name="DefaultModules">Các module homepage mặc định của template.</param>
/// <param name="ThemeTokens">Token style mẫu của template.</param>
public sealed record TemplateDetailResponse(
    string TemplateKey,
    string Name,
    string Specialty,
    string Description,
    string PreviewImageUrl,
    IReadOnlyList<string> SupportedModes,
    IReadOnlyList<string> DefaultModules,
    IReadOnlyDictionary<string, string> ThemeTokens);

/// <summary>
/// Response danh sách template registry toàn platform.
/// </summary>
/// <param name="Items">Các template hiện có trong registry dummy.</param>
public sealed record TemplateListResponse(IReadOnlyList<TemplateSummaryResponse> Items);

/// <summary>
/// Response sau khi áp template ở trạng thái stub.
/// </summary>
/// <param name="TenantId">Tenant nhận template.</param>
/// <param name="TemplateKey">Template đã được chọn.</param>
/// <param name="Mode">Mode apply đã dùng.</param>
/// <param name="AppliedAtUtc">Thời điểm apply dummy.</param>
/// <param name="AppliedBy">Actor/user thực hiện apply nếu có.</param>
/// <param name="Status">Trạng thái apply dummy.</param>
public sealed record TemplateApplyResponse(
    Guid TenantId,
    string TemplateKey,
    string Mode,
    DateTimeOffset AppliedAtUtc,
    string? AppliedBy,
    string Status);

/// <summary>
/// Response template active hiện tại của tenant.
/// </summary>
/// <param name="TenantId">Tenant đang được truy vấn.</param>
/// <param name="TemplateKey">Template active placeholder.</param>
/// <param name="Mode">Mode apply gần nhất.</param>
/// <param name="AppliedAtUtc">Thời điểm apply gần nhất.</param>
public sealed record ActiveTemplateResponse(
    Guid TenantId,
    string TemplateKey,
    string Mode,
    DateTimeOffset AppliedAtUtc);

/// <summary>
/// Response diff preview trước khi apply template.
/// </summary>
/// <param name="TenantId">Tenant đang preview diff.</param>
/// <param name="TemplateKey">Template dùng để so sánh.</param>
/// <param name="Mode">Mode apply dự kiến.</param>
/// <param name="ChangedAreas">Các vùng CMS/style/content dự kiến bị thay đổi.</param>
public sealed record TemplateDiffResponse(
    Guid TenantId,
    string TemplateKey,
    string Mode,
    IReadOnlyList<string> ChangedAreas);
