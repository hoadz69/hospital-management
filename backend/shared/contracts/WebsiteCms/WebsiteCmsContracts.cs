namespace ClinicSaaS.Contracts.WebsiteCms;

/// <summary>
/// Request cập nhật settings website cho một tenant.
/// </summary>
/// <param name="ThemeKey">Khóa theme/template style đang active.</param>
/// <param name="LogoUrl">URL logo hiển thị trên public website.</param>
/// <param name="FaviconUrl">URL favicon nếu có.</param>
/// <param name="BrandColors">Bảng màu brand dùng cho theme.</param>
/// <param name="ContactInfo">Thông tin liên hệ public của phòng khám.</param>
/// <param name="SocialLinks">Danh sách social link public.</param>
/// <param name="SeoMeta">Metadata SEO mặc định.</param>
/// <param name="CustomDomain">Domain public đang chọn nếu có.</param>
public sealed record UpsertWebsiteSettingsRequest(
    string ThemeKey,
    string? LogoUrl,
    string? FaviconUrl,
    IReadOnlyDictionary<string, string> BrandColors,
    WebsiteContactInfo ContactInfo,
    IReadOnlyDictionary<string, string> SocialLinks,
    WebsiteSeoMeta SeoMeta,
    string? CustomDomain);

/// <summary>
/// Response settings website của một tenant.
/// </summary>
/// <param name="TenantId">Tenant sở hữu settings.</param>
/// <param name="ThemeKey">Khóa theme/template style đang active.</param>
/// <param name="LogoUrl">URL logo hiển thị trên public website.</param>
/// <param name="FaviconUrl">URL favicon nếu có.</param>
/// <param name="BrandColors">Bảng màu brand dùng cho theme.</param>
/// <param name="ContactInfo">Thông tin liên hệ public của phòng khám.</param>
/// <param name="SocialLinks">Danh sách social link public.</param>
/// <param name="SeoMeta">Metadata SEO mặc định.</param>
/// <param name="CustomDomain">Domain public đang chọn nếu có.</param>
/// <param name="UpdatedAtUtc">Thời điểm settings được cập nhật dummy.</param>
public sealed record WebsiteSettingsResponse(
    Guid TenantId,
    string ThemeKey,
    string? LogoUrl,
    string? FaviconUrl,
    IReadOnlyDictionary<string, string> BrandColors,
    WebsiteContactInfo ContactInfo,
    IReadOnlyDictionary<string, string> SocialLinks,
    WebsiteSeoMeta SeoMeta,
    string? CustomDomain,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Thông tin liên hệ public của phòng khám.
/// </summary>
/// <param name="Phone">Số điện thoại public placeholder.</param>
/// <param name="Email">Email public placeholder.</param>
/// <param name="Address">Địa chỉ public placeholder.</param>
/// <param name="MapUrl">URL bản đồ nếu có.</param>
public sealed record WebsiteContactInfo(string? Phone, string? Email, string? Address, string? MapUrl);

/// <summary>
/// Metadata SEO mặc định của public website.
/// </summary>
/// <param name="Title">Tiêu đề SEO mặc định.</param>
/// <param name="Description">Mô tả SEO mặc định.</param>
/// <param name="Keywords">Từ khóa SEO dạng danh sách.</param>
public sealed record WebsiteSeoMeta(string Title, string Description, IReadOnlyList<string> Keywords);

/// <summary>
/// Request tạo hoặc cập nhật một slide homepage.
/// </summary>
/// <param name="Title">Tiêu đề slide.</param>
/// <param name="Subtitle">Mô tả phụ của slide.</param>
/// <param name="ImageUrl">URL ảnh slide.</param>
/// <param name="CtaText">Text CTA nếu có.</param>
/// <param name="CtaUrl">URL CTA nếu có.</param>
/// <param name="Order">Thứ tự hiển thị.</param>
/// <param name="IsEnabled">Cho biết slide có đang bật không.</param>
public sealed record UpsertSliderRequest(
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? CtaText,
    string? CtaUrl,
    int Order,
    bool IsEnabled);

/// <summary>
/// Response slide homepage của tenant.
/// </summary>
/// <param name="Id">Định danh slide.</param>
/// <param name="TenantId">Tenant sở hữu slide.</param>
/// <param name="Title">Tiêu đề slide.</param>
/// <param name="Subtitle">Mô tả phụ của slide.</param>
/// <param name="ImageUrl">URL ảnh slide.</param>
/// <param name="CtaText">Text CTA nếu có.</param>
/// <param name="CtaUrl">URL CTA nếu có.</param>
/// <param name="Order">Thứ tự hiển thị.</param>
/// <param name="IsEnabled">Cho biết slide có đang bật không.</param>
public sealed record SliderResponse(
    Guid Id,
    Guid TenantId,
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? CtaText,
    string? CtaUrl,
    int Order,
    bool IsEnabled);

/// <summary>
/// Response danh sách slide homepage.
/// </summary>
/// <param name="Items">Các slide của tenant.</param>
public sealed record SliderListResponse(IReadOnlyList<SliderResponse> Items);

/// <summary>
/// Request cập nhật nội dung một page CMS.
/// </summary>
/// <param name="Sections">Danh sách section JSON-like đã được client chuẩn hóa.</param>
/// <param name="PublishStatus">Trạng thái draft/published/conflict mong muốn.</param>
public sealed record UpsertWebsitePageRequest(
    IReadOnlyList<WebsitePageSection> Sections,
    string PublishStatus);

/// <summary>
/// Response page CMS của tenant.
/// </summary>
/// <param name="TenantId">Tenant sở hữu page.</param>
/// <param name="PageKey">Khóa page, ví dụ `home`, `about`, `services`.</param>
/// <param name="Sections">Danh sách section JSON-like đã được client chuẩn hóa.</param>
/// <param name="PublishStatus">Trạng thái draft/published/conflict.</param>
/// <param name="LastModifiedAtUtc">Thời điểm cập nhật dummy gần nhất.</param>
public sealed record WebsitePageResponse(
    Guid TenantId,
    string PageKey,
    IReadOnlyList<WebsitePageSection> Sections,
    string PublishStatus,
    DateTimeOffset LastModifiedAtUtc);

/// <summary>
/// Section nội dung CMS dạng contract trung lập để FE builder ghép sau.
/// </summary>
/// <param name="SectionKey">Khóa section trong page.</param>
/// <param name="ComponentType">Loại component render public website.</param>
/// <param name="Content">Payload key-value placeholder của section.</param>
public sealed record WebsitePageSection(
    string SectionKey,
    string ComponentType,
    IReadOnlyDictionary<string, string> Content);

/// <summary>
/// Response danh sách page CMS của tenant.
/// </summary>
/// <param name="Items">Các page của tenant.</param>
public sealed record WebsitePageListResponse(IReadOnlyList<WebsitePageResponse> Items);

/// <summary>
/// Response publish website dummy.
/// </summary>
/// <param name="TenantId">Tenant được publish.</param>
/// <param name="Status">Trạng thái publish dummy.</param>
/// <param name="Version">Version publish placeholder.</param>
/// <param name="PublishedAtUtc">Thời điểm publish dummy.</param>
public sealed record WebsitePublishResponse(
    Guid TenantId,
    string Status,
    string Version,
    DateTimeOffset PublishedAtUtc);

/// <summary>
/// Response một lần publish trong lịch sử.
/// </summary>
/// <param name="Version">Version publish placeholder.</param>
/// <param name="Status">Trạng thái lần publish.</param>
/// <param name="PublishedAtUtc">Thời điểm publish.</param>
/// <param name="PublishedBy">Actor/user publish nếu có.</param>
public sealed record WebsitePublishHistoryItem(
    string Version,
    string Status,
    DateTimeOffset PublishedAtUtc,
    string? PublishedBy);

/// <summary>
/// Response lịch sử publish website.
/// </summary>
/// <param name="Items">Các lần publish gần nhất.</param>
public sealed record WebsitePublishHistoryResponse(IReadOnlyList<WebsitePublishHistoryItem> Items);
