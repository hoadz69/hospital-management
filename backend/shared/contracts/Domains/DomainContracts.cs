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
