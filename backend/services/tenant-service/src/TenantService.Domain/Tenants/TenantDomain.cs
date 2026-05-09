namespace TenantService.Domain.Tenants;

/// <summary>
/// Domain hoặc subdomain được gắn với tenant.
/// </summary>
/// <param name="Id">Định danh domain record.</param>
/// <param name="TenantId">Định danh tenant sở hữu domain.</param>
/// <param name="DomainName">Domain hiển thị đã được lưu cho tenant.</param>
/// <param name="NormalizedDomainName">Domain đã chuẩn hóa để lookup và enforce unique.</param>
/// <param name="DomainType">Loại domain: subdomain mặc định hoặc custom domain.</param>
/// <param name="Status">Trạng thái domain trong vòng đời Tenant MVP.</param>
/// <param name="IsPrimary">Cho biết domain này là domain chính của tenant.</param>
/// <param name="CreatedAtUtc">Thời điểm domain được tạo theo UTC.</param>
/// <param name="VerifiedAtUtc">Thời điểm domain được xác minh theo UTC, nếu có.</param>
public sealed record TenantDomain(
    Guid Id,
    Guid TenantId,
    string DomainName,
    string NormalizedDomainName,
    TenantDomainType DomainType,
    TenantDomainStatus Status,
    bool IsPrimary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? VerifiedAtUtc);
