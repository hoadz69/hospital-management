using ClinicSaaS.BuildingBlocks.Results;

namespace TenantService.Application.Tenants;

/// <summary>
/// Factory lỗi nghiệp vụ chuẩn cho Tenant Service.
/// </summary>
public static class TenantErrors
{
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
}
