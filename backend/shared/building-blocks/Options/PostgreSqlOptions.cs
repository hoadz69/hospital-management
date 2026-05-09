namespace ClinicSaaS.BuildingBlocks.Options;

/// <summary>
/// Cấu hình PostgreSQL dùng chung, chỉ chứa placeholder/env binding chứ không chứa secret thật.
/// </summary>
public sealed class PostgreSqlOptions
{
    /// <summary>
    /// Tên section cấu hình PostgreSQL trong appsettings.
    /// </summary>
    public const string SectionName = OptionsSectionNames.PostgreSql;

    /// <summary>
    /// Cho biết service có bật cấu hình PostgreSQL hay chỉ giữ placeholder.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Tên provider database để system endpoint hiển thị.
    /// </summary>
    public string Provider { get; init; } = "PostgreSQL";

    /// <summary>
    /// Connection string fallback từ appsettings, không dùng cho secret production thật.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Tên biến môi trường ưu tiên để lấy connection string thật trong runtime local/staging.
    /// </summary>
    public string? ConnectionStringEnvironmentVariable { get; init; }
}
