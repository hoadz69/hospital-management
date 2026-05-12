using ClinicSaaS.BuildingBlocks.Results;

namespace WebsiteCmsService.Application.Website;

/// <summary>
/// Các lỗi contract ổn định của Website CMS Service.
/// </summary>
public static class WebsiteCmsContractErrors
{
    /// <summary>
    /// Lỗi tenant context không khớp tenant trên route.
    /// </summary>
    public static Error TenantMismatch { get; } = new(
        "website_cms.tenant_mismatch",
        "Tenant context does not match the tenant route parameter.");

    /// <summary>
    /// Tạo lỗi not found theo resource.
    /// </summary>
    /// <param name="resource">Resource không tìm thấy.</param>
    /// <returns>Lỗi not found cho API layer.</returns>
    public static Error NotFound(string resource) => new(
        "website_cms.not_found",
        $"{resource} was not found.");

    /// <summary>
    /// Tạo lỗi validation theo field.
    /// </summary>
    /// <param name="field">Field request bị lỗi.</param>
    /// <param name="message">Thông điệp lỗi an toàn.</param>
    /// <returns>Lỗi validation chứa field tương ứng.</returns>
    public static Error Validation(string field, string message) => new(
        "website_cms.validation",
        message,
        new Dictionary<string, string[]> { [field] = [message] });
}
