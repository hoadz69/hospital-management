using ClinicSaaS.BuildingBlocks.Results;

namespace TemplateService.Application.Templates;

/// <summary>
/// Các lỗi contract ổn định của Template Service.
/// </summary>
public static class TemplateContractErrors
{
    /// <summary>
    /// Lỗi tenant context không khớp tenant trên route.
    /// </summary>
    public static Error TenantMismatch { get; } = new(
        "templates.tenant_mismatch",
        "Tenant context does not match the tenant route parameter.");

    /// <summary>
    /// Tạo lỗi template không tồn tại.
    /// </summary>
    /// <param name="templateKey">Template key không tìm thấy.</param>
    /// <returns>Lỗi not found cho API layer.</returns>
    public static Error NotFound(string templateKey) => new(
        "templates.not_found",
        $"Template '{templateKey}' was not found.");

    /// <summary>
    /// Tạo lỗi validation theo field.
    /// </summary>
    /// <param name="field">Field request bị lỗi.</param>
    /// <param name="message">Thông điệp lỗi an toàn.</param>
    /// <returns>Lỗi validation chứa field tương ứng.</returns>
    public static Error Validation(string field, string message) => new(
        "templates.validation",
        message,
        new Dictionary<string, string[]> { [field] = [message] });
}
