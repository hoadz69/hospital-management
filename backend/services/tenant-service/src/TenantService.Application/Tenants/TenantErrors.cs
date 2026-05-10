using ClinicSaaS.BuildingBlocks.Results;

namespace TenantService.Application.Tenants;

/// <summary>
/// Factory lỗi nghiệp vụ chuẩn cho Tenant Service.
/// </summary>
public static class TenantErrors
{
    /// <summary>
    /// Tên key dùng trong <see cref="Error.Details"/> để chứa danh sách field bị conflict;
    /// API layer đọc key này để build `extensions.fields` trong ProblemDetails 409.
    /// </summary>
    public const string FieldsDetailKey = "fields";

    /// <summary>
    /// Mã field conflict cho slug tenant; FE dùng làm literal type `TenantConflictField`.
    /// </summary>
    public const string FieldSlug = "slug";

    /// <summary>
    /// Mã field conflict cho domain mặc định của tenant; FE dùng làm literal type `TenantConflictField`.
    /// </summary>
    public const string FieldDefaultDomainName = "defaultDomainName";

    /// <summary>
    /// Mã field conflict cho email liên hệ tenant; reserve cho future unique constraint.
    /// </summary>
    public const string FieldContactEmail = "contactEmail";

    /// <summary>
    /// Tạo lỗi validation cho request tenant không hợp lệ.
    /// </summary>
    /// <param name="details">Danh sách lỗi theo tên field/request property.</param>
    /// <returns>Error chuẩn để API layer map sang validation problem.</returns>
    public static Error Validation(IReadOnlyDictionary<string, string[]> details)
        => new("tenants.validation", "Tenant request is invalid.", details);

    /// <summary>
    /// Tạo lỗi không tìm thấy tenant.
    /// </summary>
    /// <param name="tenantId">Định danh tenant caller đang truy vấn hoặc cập nhật.</param>
    /// <returns>Error chuẩn để API layer map sang 404.</returns>
    public static Error NotFound(Guid tenantId)
        => new("tenants.not_found", $"Tenant '{tenantId}' was not found.");

    /// <summary>
    /// Tạo lỗi xung đột dữ liệu tenant như slug/domain bị trùng.
    /// </summary>
    /// <param name="message">Thông điệp mô tả xung đột nghiệp vụ an toàn để trả về client.</param>
    /// <returns>Error chuẩn để API layer map sang 409.</returns>
    public static Error Conflict(string message)
        => new("tenants.conflict", message);

    /// <summary>
    /// Tạo lỗi xung đột dữ liệu tenant kèm danh sách field bị trùng để FE định vị input lỗi
    /// mà không phải regex parse `detail`. Field codes dùng các hằng số `Field*` trên type này.
    /// </summary>
    /// <param name="message">Thông điệp mô tả xung đột nghiệp vụ an toàn để trả về client.</param>
    /// <param name="fields">Danh sách field bị conflict, ví dụ ["slug"], ["defaultDomainName"].</param>
    /// <returns>Error chuẩn có <see cref="Error.Details"/>["fields"] = các field code; API layer
    /// đọc key này để build `extensions.fields` trong ProblemDetails 409.</returns>
    public static Error Conflict(string message, IReadOnlyList<string> fields)
    {
        var details = new Dictionary<string, string[]>
        {
            [FieldsDetailKey] = fields.ToArray()
        };
        return new("tenants.conflict", message, details);
    }
}
