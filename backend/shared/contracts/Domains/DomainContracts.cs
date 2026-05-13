namespace ClinicSaaS.Contracts.Domains;

/// <summary>
/// Request đăng ký domain cho một tenant trong Domain Service contract.
/// </summary>
/// <param name="DomainName">Tên domain người dùng nhập, ví dụ `demo.clinicos.local`.</param>
/// <param name="DomainType">Loại domain: `DefaultSubdomain` hoặc `CustomDomain`.</param>
/// <param name="IsPrimary">Cho biết domain này có là domain chính của tenant hay không.</param>
public sealed record RegisterDomainRequest(string DomainName, string? DomainType, bool IsPrimary);

/// <summary>
/// Response mô tả bản ghi domain thuộc một tenant.
/// </summary>
/// <param name="Id">Định danh domain trong Domain Service.</param>
/// <param name="TenantId">Tenant sở hữu domain.</param>
/// <param name="DomainName">Tên domain hiển thị.</param>
/// <param name="NormalizedDomainName">Tên domain đã chuẩn hóa để lookup/cache.</param>
/// <param name="DomainType">Loại domain: `DefaultSubdomain` hoặc `CustomDomain`.</param>
/// <param name="Status">Trạng thái xác minh DNS: `Pending`, `Verified`, `Failed` hoặc `Suspended`.</param>
/// <param name="IsPrimary">Cho biết domain này có là domain chính của tenant hay không.</param>
/// <param name="SslState">Trạng thái SSL: `None`, `Pending`, `Issued` hoặc `Failed`.</param>
/// <param name="DnsTarget">DNS target mà khách hàng cần trỏ về.</param>
/// <param name="VerifiedAtUtc">Thời điểm DNS verify thành công nếu có.</param>
/// <param name="LastCheckedAtUtc">Thời điểm hệ thống kiểm tra DNS gần nhất.</param>
/// <param name="LastErrorMessage">Thông điệp lỗi an toàn cho FE nếu verify thất bại.</param>
public sealed record DomainResponse(
    Guid Id,
    Guid TenantId,
    string DomainName,
    string NormalizedDomainName,
    string DomainType,
    string Status,
    bool IsPrimary,
    string SslState,
    string DnsTarget,
    DateTimeOffset? VerifiedAtUtc,
    DateTimeOffset? LastCheckedAtUtc,
    string? LastErrorMessage);

/// <summary>
/// Response danh sách domain của một tenant.
/// </summary>
/// <param name="Items">Các domain thuộc tenant hiện tại.</param>
public sealed record DomainListResponse(IReadOnlyList<DomainResponse> Items);

/// <summary>
/// DNS record ma FE can hien thi trong hang retry/diagnostic cua domain.
/// </summary>
/// <param name="RecordType">Loai record DNS, vi du `CNAME` hoac `TXT`.</param>
/// <param name="Host">Host/name ma tenant can cau hinh tai DNS provider.</param>
/// <param name="ExpectedValue">Gia tri mong doi de Domain API xac minh.</param>
/// <param name="ActualValue">Gia tri he thong doc duoc gan nhat neu co.</param>
/// <param name="Status">Trang thai record: `pending`, `propagating`, `failed` hoac `verified`.</param>
/// <param name="Message">Thong diep ngan an toan cho FE hien thi.</param>
public sealed record DomainDnsRecordResponse(
    string RecordType,
    string Host,
    string ExpectedValue,
    string? ActualValue,
    string Status,
    string? Message);

/// <summary>
/// Trang thai DNS retry va SSL cua mot domain thuoc tenant.
/// </summary>
/// <param name="DomainId">Dinh danh domain record.</param>
/// <param name="DomainName">Ten domain hien thi.</param>
/// <param name="DnsStatus">Trang thai DNS tong hop: `pending`, `propagating`, `failed` hoac `verified`.</param>
/// <param name="DnsRecords">Danh sach CNAME/TXT can FE hien thi de owner sua DNS.</param>
/// <param name="LastCheckedAt">Thoi diem he thong kiem tra DNS gan nhat.</param>
/// <param name="RetryCount">So lan owner da yeu cau retry DNS.</param>
/// <param name="NextRetryAt">Thoi diem du kien co the kiem tra lai.</param>
/// <param name="SslStatus">Trang thai SSL: `none`, `pending`, `issued` hoac `failed`.</param>
/// <param name="SslIssuer">Issuer cua certificate neu da cap.</param>
/// <param name="ExpiresAt">Thoi diem certificate het han neu co.</param>
/// <param name="Message">Thong diep ngan an toan cho FE hien thi.</param>
public sealed record DomainDnsSslStateResponse(
    Guid DomainId,
    string DomainName,
    string DnsStatus,
    IReadOnlyList<DomainDnsRecordResponse> DnsRecords,
    DateTimeOffset? LastCheckedAt,
    int RetryCount,
    DateTimeOffset? NextRetryAt,
    string SslStatus,
    string? SslIssuer,
    DateTimeOffset? ExpiresAt,
    string? Message);

/// <summary>
/// Response danh sach trang thai DNS/SSL domain cua mot tenant.
/// </summary>
/// <param name="Items">Cac domain thuoc tenant kem trang thai DNS/SSL hien tai.</param>
public sealed record DomainDnsSslListResponse(IReadOnlyList<DomainDnsSslStateResponse> Items);

/// <summary>
/// Response cho thao tác verify DNS dạng dummy trong Wave A.
/// </summary>
/// <param name="DomainId">Định danh domain được verify.</param>
/// <param name="Status">Trạng thái verify hiện tại.</param>
/// <param name="SslState">Trạng thái SSL hiện tại.</param>
/// <param name="NextCheckAfterUtc">Thời điểm FE có thể poll lại.</param>
/// <param name="Message">Thông điệp ngắn để FE hiển thị trạng thái.</param>
public sealed record DomainVerificationResponse(
    Guid DomainId,
    string Status,
    string SslState,
    DateTimeOffset NextCheckAfterUtc,
    string Message);

/// <summary>
/// Response trạng thái publish website theo domain.
/// </summary>
/// <param name="TenantId">Tenant được publish.</param>
/// <param name="PublishStatus">Trạng thái publish dummy của Wave A.</param>
/// <param name="PrimaryDomainName">Domain chính dự kiến phục vụ public website.</param>
/// <param name="PublishedAtUtc">Thời điểm publish dummy.</param>
public sealed record DomainPublishResponse(
    Guid TenantId,
    string PublishStatus,
    string PrimaryDomainName,
    DateTimeOffset PublishedAtUtc);
