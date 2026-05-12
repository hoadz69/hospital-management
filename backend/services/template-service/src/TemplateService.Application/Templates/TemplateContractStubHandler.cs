using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Templates;

namespace TemplateService.Application.Templates;

/// <summary>
/// Handler Application trả contract/stub cho Template Service Wave A.
/// </summary>
public sealed class TemplateContractStubHandler(ITenantContextAccessor tenantContextAccessor)
{
    private static readonly string[] SupportedModes = ["full", "style-only", "content-only"];
    private static readonly TemplateDetailResponse[] Templates =
    [
        new(
            "dental",
            "Dental Care",
            "Dental",
            "Template nha khoa với hero slider, dịch vụ nổi bật và booking CTA.",
            "/assets/templates/dental-preview.webp",
            SupportedModes,
            ["heroSlider", "services", "doctors", "pricing", "testimonials"],
            new Dictionary<string, string>
            {
                ["primary"] = "#0E7C86",
                ["secondary"] = "#2563EB",
                ["radius"] = "8px"
            }),
        new(
            "general",
            "General Clinic",
            "General",
            "Template phòng khám tổng quát cho public website MVP.",
            "/assets/templates/general-preview.webp",
            SupportedModes,
            ["heroSlider", "services", "doctors", "contact"],
            new Dictionary<string, string>
            {
                ["primary"] = "#0F766E",
                ["secondary"] = "#334155",
                ["radius"] = "8px"
            })
    ];

    /// <summary>
    /// Trả template registry dummy toàn platform.
    /// </summary>
    /// <returns>Danh sách template summary cho Owner/Clinic Admin.</returns>
    public TemplateListResponse ListTemplates()
    {
        return new TemplateListResponse(Templates.Select(ToSummary).ToArray());
    }

    /// <summary>
    /// Trả chi tiết template dummy theo key.
    /// </summary>
    /// <param name="templateKey">Khóa template cần lấy.</param>
    /// <returns>Kết quả chứa template detail hoặc lỗi not found.</returns>
    public Result<TemplateDetailResponse> GetTemplate(string templateKey)
    {
        var template = Templates.FirstOrDefault(item =>
            string.Equals(item.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase));

        return template is null
            ? Result<TemplateDetailResponse>.Failure(TemplateContractErrors.NotFound(templateKey))
            : Result<TemplateDetailResponse>.Success(template);
    }

    /// <summary>
    /// Trả response apply template dummy sau khi validate tenant và mode.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="request">Payload apply template.</param>
    /// <returns>Kết quả apply dummy hoặc lỗi validation/not found.</returns>
    public Result<TemplateApplyResponse> ApplyTemplate(Guid tenantId, ApplyTemplateRequest request)
    {
        var validation = ValidateTenantAndRequest(tenantId, request.TemplateKey, request.Mode);
        if (validation is not null)
        {
            return Result<TemplateApplyResponse>.Failure(validation);
        }

        return Result<TemplateApplyResponse>.Success(new TemplateApplyResponse(
            tenantId,
            request.TemplateKey,
            request.Mode,
            DateTimeOffset.UtcNow,
            request.RequestedBy,
            "accepted-stub"));
    }

    /// <summary>
    /// Trả template active dummy của tenant.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <returns>Kết quả chứa active template dummy.</returns>
    public Result<ActiveTemplateResponse> GetActiveTemplate(Guid tenantId)
    {
        var scopeError = ValidateTenantScope(tenantId);
        return scopeError is not null
            ? Result<ActiveTemplateResponse>.Failure(scopeError)
            : Result<ActiveTemplateResponse>.Success(new ActiveTemplateResponse(
                tenantId,
                "dental",
                "style-only",
                DateTimeOffset.UtcNow.AddDays(-1)));
    }

    /// <summary>
    /// Trả diff preview dummy trước khi apply template.
    /// </summary>
    /// <param name="tenantId">Tenant trên route cần khớp với context.</param>
    /// <param name="request">Payload apply template dùng để preview.</param>
    /// <returns>Kết quả chứa vùng thay đổi dự kiến.</returns>
    public Result<TemplateDiffResponse> PreviewDiff(Guid tenantId, ApplyTemplateRequest request)
    {
        var validation = ValidateTenantAndRequest(tenantId, request.TemplateKey, request.Mode);
        if (validation is not null)
        {
            return Result<TemplateDiffResponse>.Failure(validation);
        }

        string[] changedAreas = request.Mode switch
        {
            "style-only" => ["brandColors", "themeTokens"],
            "content-only" => ["homepageSections", "sampleCopy"],
            _ => ["brandColors", "themeTokens", "homepageSections", "sampleCopy", "sliders"]
        };

        return Result<TemplateDiffResponse>.Success(new TemplateDiffResponse(
            tenantId,
            request.TemplateKey,
            request.Mode,
            changedAreas));
    }

    private Error? ValidateTenantAndRequest(Guid tenantId, string templateKey, string mode)
    {
        var scopeError = ValidateTenantScope(tenantId);
        if (scopeError is not null)
        {
            return scopeError;
        }

        if (!SupportedModes.Contains(mode, StringComparer.OrdinalIgnoreCase))
        {
            return TemplateContractErrors.Validation("mode", "Apply mode must be full, style-only, or content-only.");
        }

        return Templates.Any(item => string.Equals(item.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase))
            ? null
            : TemplateContractErrors.NotFound(templateKey);
    }

    private Error? ValidateTenantScope(Guid tenantId)
    {
        return string.Equals(
            tenantContextAccessor.Current.TenantId,
            tenantId.ToString(),
            StringComparison.OrdinalIgnoreCase)
            ? null
            : TemplateContractErrors.TenantMismatch;
    }

    private static TemplateSummaryResponse ToSummary(TemplateDetailResponse template)
    {
        return new TemplateSummaryResponse(
            template.TemplateKey,
            template.Name,
            template.Specialty,
            template.PreviewImageUrl,
            template.SupportedModes);
    }
}
