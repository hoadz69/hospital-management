using System.Text.RegularExpressions;

namespace TenantService.Domain.Tenants;

/// <summary>
/// Chuẩn hóa slug, domain và module code để giữ invariant trước khi persist.
/// </summary>
public static class TenantNormalization
{
    /// <summary>
    /// Chuẩn hóa chuỗi bắt buộc bằng cách trim và fail nếu giá trị rỗng.
    /// </summary>
    /// <param name="value">Giá trị input cần chuẩn hóa.</param>
    /// <param name="parameterName">Tên tham số dùng trong lỗi validation.</param>
    /// <returns>Giá trị đã trim và không rỗng.</returns>
    public static string Required(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }

    /// <summary>
    /// Chuẩn hóa chuỗi optional bằng cách trim hoặc trả null nếu rỗng.
    /// </summary>
    /// <param name="value">Giá trị input có thể null/rỗng.</param>
    /// <returns>Giá trị đã trim hoặc null nếu input không có nội dung.</returns>
    public static string? Optional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Chuẩn hóa slug tenant về dạng lowercase token an toàn cho URL/subdomain.
    /// </summary>
    /// <param name="value">Slug owner nhập khi tạo tenant.</param>
    /// <returns>Slug đã chuẩn hóa và nằm trong giới hạn độ dài hợp lệ.</returns>
    public static string NormalizeSlug(string value)
    {
        var normalized = NormalizeToken(value, "slug");
        if (normalized.Length is < 3 or > 80)
        {
            throw new ArgumentException("Slug must be between 3 and 80 characters.", nameof(value));
        }

        return normalized;
    }

    /// <summary>
    /// Chuẩn hóa domain/subdomain để lookup và enforce unique trong PostgreSQL.
    /// </summary>
    /// <param name="value">Domain hoặc URL-like input cần chuẩn hóa.</param>
    /// <returns>Domain đã bỏ protocol, trim slash và chuyển lowercase.</returns>
    public static string NormalizeDomain(string value)
    {
        var normalized = Required(value, nameof(value)).ToLowerInvariant();
        normalized = normalized.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim()
            .Trim('/');

        if (normalized.Length is < 3 or > 255)
        {
            throw new ArgumentException("Domain name must be between 3 and 255 characters.", nameof(value));
        }

        return normalized;
    }

    /// <summary>
    /// Chuẩn hóa mã module về dạng token lowercase để lưu trong tenant_modules.
    /// </summary>
    /// <param name="value">Mã module từ request hoặc default module set.</param>
    /// <returns>Mã module đã chuẩn hóa.</returns>
    public static string NormalizeModuleCode(string value)
    {
        return NormalizeToken(value, "moduleCode");
    }

    private static string NormalizeToken(string value, string parameterName)
    {
        var normalized = Required(value, parameterName).ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"\s+", "-");
        normalized = Regex.Replace(normalized, @"[^a-z0-9-]", "-");
        normalized = Regex.Replace(normalized, "-{2,}", "-").Trim('-');

        if (normalized.Length == 0)
        {
            throw new ArgumentException("Normalized value cannot be empty.", parameterName);
        }

        return normalized;
    }
}
