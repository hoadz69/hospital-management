using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.WebsiteCms;

namespace WebsiteCmsService.Application.Website;

/// <summary>
/// Handler Application trả contract/stub cho Website CMS Service Wave A.
/// </summary>
public sealed class WebsiteCmsContractStubHandler(ITenantContextAccessor tenantContextAccessor)
{
    private static readonly string[] AllowedPageKeys = ["home", "about", "services", "doctors", "pricing", "contact", "faq", "blog"];

    /// <summary>
    /// Trả settings website dummy của tenant.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa settings dummy hoặc lỗi tenant mismatch.</returns>
    public Result<WebsiteSettingsResponse> GetSettings(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<WebsiteSettingsResponse>.Failure(scopeError)
            : Result<WebsiteSettingsResponse>.Success(BuildSettings(tenantId));
    }

    /// <summary>
    /// Trả settings response sau khi validate request cập nhật.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="request">Payload settings từ FE builder.</param>
    /// <returns>Kết quả chứa settings đã merge dummy.</returns>
    public Result<WebsiteSettingsResponse> UpsertSettings(Guid tenantId, UpsertWebsiteSettingsRequest request)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<WebsiteSettingsResponse>.Failure(scopeError);
        }

        if (string.IsNullOrWhiteSpace(request.ThemeKey))
        {
            return Result<WebsiteSettingsResponse>.Failure(WebsiteCmsContractErrors.Validation("themeKey", "Theme key is required."));
        }

        return Result<WebsiteSettingsResponse>.Success(new WebsiteSettingsResponse(
            tenantId,
            request.ThemeKey.Trim(),
            request.LogoUrl,
            request.FaviconUrl,
            request.BrandColors,
            request.ContactInfo,
            request.SocialLinks,
            request.SeoMeta,
            request.CustomDomain,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Trả danh sách slider dummy của tenant.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa danh sách slider dummy.</returns>
    public Result<SliderListResponse> ListSliders(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<SliderListResponse>.Failure(scopeError)
            : Result<SliderListResponse>.Success(new SliderListResponse([BuildSlider(tenantId, Guid.Parse("33333333-3333-4333-8333-333333333333"))]));
    }

    /// <summary>
    /// Trả slider dummy sau khi tạo hoặc cập nhật.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="slideId">Định danh slide; null khi tạo mới.</param>
    /// <param name="request">Payload slider từ FE.</param>
    /// <returns>Kết quả chứa slider dummy hoặc lỗi validation.</returns>
    public Result<SliderResponse> UpsertSlider(Guid tenantId, Guid? slideId, UpsertSliderRequest request)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<SliderResponse>.Failure(scopeError);
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result<SliderResponse>.Failure(WebsiteCmsContractErrors.Validation("title", "Slider title is required."));
        }

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            return Result<SliderResponse>.Failure(WebsiteCmsContractErrors.Validation("imageUrl", "Slider imageUrl is required."));
        }

        return Result<SliderResponse>.Success(new SliderResponse(
            slideId ?? Guid.NewGuid(),
            tenantId,
            request.Title.Trim(),
            request.Subtitle,
            request.ImageUrl,
            request.CtaText,
            request.CtaUrl,
            request.Order,
            request.IsEnabled));
    }

    /// <summary>
    /// Trả trạng thái delete dummy cho slider.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="slideId">Slide cần xóa.</param>
    /// <returns>Kết quả chứa slide đã xóa dummy.</returns>
    public Result<SliderResponse> DeleteSlider(Guid tenantId, Guid slideId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<SliderResponse>.Failure(scopeError)
            : Result<SliderResponse>.Success(BuildSlider(tenantId, slideId) with { IsEnabled = false });
    }

    /// <summary>
    /// Trả danh sách page dummy của tenant.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa các page dummy.</returns>
    public Result<WebsitePageListResponse> ListPages(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<WebsitePageListResponse>.Failure(scopeError)
            : Result<WebsitePageListResponse>.Success(new WebsitePageListResponse(AllowedPageKeys.Take(3).Select(key => BuildPage(tenantId, key)).ToArray()));
    }

    /// <summary>
    /// Trả page dummy theo key.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="pageKey">Page key cần lấy.</param>
    /// <returns>Kết quả chứa page dummy hoặc lỗi not found.</returns>
    public Result<WebsitePageResponse> GetPage(Guid tenantId, string pageKey)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<WebsitePageResponse>.Failure(scopeError);
        }

        return IsAllowedPage(pageKey)
            ? Result<WebsitePageResponse>.Success(BuildPage(tenantId, pageKey))
            : Result<WebsitePageResponse>.Failure(WebsiteCmsContractErrors.NotFound("Website page"));
    }

    /// <summary>
    /// Trả page response sau khi validate payload cập nhật.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="pageKey">Page key cần cập nhật.</param>
    /// <param name="request">Payload page từ FE builder.</param>
    /// <returns>Kết quả chứa page đã cập nhật dummy.</returns>
    public Result<WebsitePageResponse> UpsertPage(Guid tenantId, string pageKey, UpsertWebsitePageRequest request)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return Result<WebsitePageResponse>.Failure(scopeError);
        }

        if (!IsAllowedPage(pageKey))
        {
            return Result<WebsitePageResponse>.Failure(WebsiteCmsContractErrors.NotFound("Website page"));
        }

        if (request.Sections.Count == 0)
        {
            return Result<WebsitePageResponse>.Failure(WebsiteCmsContractErrors.Validation("sections", "At least one section is required."));
        }

        return Result<WebsitePageResponse>.Success(new WebsitePageResponse(
            tenantId,
            pageKey,
            request.Sections,
            request.PublishStatus,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Trả publish response dummy.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa publish status dummy.</returns>
    public Result<WebsitePublishResponse> Publish(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<WebsitePublishResponse>.Failure(scopeError)
            : Result<WebsitePublishResponse>.Success(new WebsitePublishResponse(
                tenantId,
                "queued",
                $"draft-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Trả lịch sử publish dummy.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa lịch sử publish dummy.</returns>
    public Result<WebsitePublishHistoryResponse> GetPublishHistory(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<WebsitePublishHistoryResponse>.Failure(scopeError)
            : Result<WebsitePublishHistoryResponse>.Success(new WebsitePublishHistoryResponse(
            [
                new WebsitePublishHistoryItem("draft-202605100001", "published", DateTimeOffset.UtcNow.AddDays(-1), "owner-admin")
            ]));
    }

    private Error? ValidateTenantScope(Guid tenantId)
    {
        return string.Equals(
            tenantContextAccessor.Current.TenantId,
            tenantId.ToString(),
            StringComparison.OrdinalIgnoreCase)
            ? null
            : WebsiteCmsContractErrors.TenantMismatch;
    }

    private static WebsiteSettingsResponse BuildSettings(Guid tenantId)
    {
        return new WebsiteSettingsResponse(
            tenantId,
            "dental",
            "/assets/demo/logo.svg",
            "/assets/demo/favicon.ico",
            new Dictionary<string, string>
            {
                ["primary"] = "#0E7C86",
                ["secondary"] = "#2563EB",
                ["surface"] = "#F8FAFC"
            },
            new WebsiteContactInfo("1900-0000", "hello@demo.clinicos.local", "123 Clinic Street", null),
            new Dictionary<string, string>
            {
                ["facebook"] = "https://social.example/clinic"
            },
            new WebsiteSeoMeta("Demo Clinic", "Website CMS contract stub for Clinic SaaS.", ["clinic", "healthcare"]),
            "demo.clinicos.local",
            DateTimeOffset.UtcNow);
    }

    private static SliderResponse BuildSlider(Guid tenantId, Guid slideId)
    {
        return new SliderResponse(
            slideId,
            tenantId,
            "Chăm sóc sức khỏe toàn diện",
            "Đặt lịch nhanh với đội ngũ bác sĩ giàu kinh nghiệm.",
            "/assets/demo/slider-1.webp",
            "Đặt lịch",
            "/booking",
            1,
            true);
    }

    private static WebsitePageResponse BuildPage(Guid tenantId, string pageKey)
    {
        return new WebsitePageResponse(
            tenantId,
            pageKey,
            [new WebsitePageSection("hero", "HeroPrimary", new Dictionary<string, string> { ["title"] = "Demo Clinic" })],
            "draft",
            DateTimeOffset.UtcNow);
    }

    private static bool IsAllowedPage(string pageKey)
    {
        return AllowedPageKeys.Contains(pageKey, StringComparer.OrdinalIgnoreCase);
    }
}
