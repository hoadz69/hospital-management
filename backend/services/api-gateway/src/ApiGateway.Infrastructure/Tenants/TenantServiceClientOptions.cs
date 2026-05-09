namespace ApiGateway.Infrastructure.Tenants;

/// <summary>
/// Options cấu hình base URL của Tenant Service cho typed HttpClient.
/// </summary>
public sealed class TenantServiceClientOptions
{
    /// <summary>
    /// Tên section cấu hình trong appsettings.
    /// </summary>
    public const string SectionName = "Services:TenantService";

    /// <summary>
    /// Base URL fallback đọc từ appsettings khi env var không có.
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// Tên biến môi trường ưu tiên để override Tenant Service base URL.
    /// </summary>
    public string? BaseUrlEnvironmentVariable { get; init; } = "CLINICSAAS_TENANT_SERVICE_BASE_URL";
}
