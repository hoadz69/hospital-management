using TenantService.Domain.Tenants;

namespace TenantService.Application.Tenants;

/// <summary>
/// Parser trạng thái tenant dùng chung cho query và command boundary.
/// </summary>
public static class TenantStatusParser
{
    /// <summary>
    /// Parse chuỗi trạng thái tenant theo enum domain, không phân biệt hoa thường.
    /// </summary>
    /// <param name="value">Chuỗi trạng thái từ query string hoặc request body.</param>
    /// <param name="status">Trạng thái tenant đã parse nếu input hợp lệ.</param>
    /// <returns>`true` nếu input là trạng thái hợp lệ; ngược lại là `false`.</returns>
    public static bool TryParse(string? value, out TenantStatus status)
    {
        return Enum.TryParse(value, ignoreCase: true, out status)
            && Enum.IsDefined(status);
    }
}
