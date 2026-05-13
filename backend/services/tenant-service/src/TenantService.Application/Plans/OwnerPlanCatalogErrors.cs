using ClinicSaaS.BuildingBlocks.Results;

namespace TenantService.Application.Plans;

/// <summary>
/// Factory lỗi nghiệp vụ cho Owner Plan & Module Catalog.
/// </summary>
public static class OwnerPlanCatalogErrors
{
    /// <summary>
    /// Tạo lỗi validation cho payload owner-plan không hợp lệ.
    /// </summary>
    /// <param name="details">Danh sách lỗi theo field/request property.</param>
    /// <returns>Error chuẩn để API layer map sang validation problem.</returns>
    public static Error Validation(IReadOnlyDictionary<string, string[]> details)
        => new("plans.validation", "Owner plan bulk-change request is invalid.", details);

    /// <summary>
    /// Tạo lỗi không tìm thấy plan hoặc tenant assignment trong DB thật.
    /// </summary>
    /// <param name="message">Thông điệp an toàn trả về client.</param>
    /// <returns>Error chuẩn để API layer map sang 404.</returns>
    public static Error NotFound(string message)
        => new("plans.not_found", message);
}
