using ClinicSaaS.Contracts.Tenancy;

namespace ApiGateway.Application.Tenants;

/// <summary>
/// Port HTTP client để API Gateway forward tenant management requests sang Tenant Service.
/// </summary>
public interface ITenantServiceClient
{
    /// <summary>
    /// Gửi request tạo tenant sang Tenant Service.
    /// </summary>
    /// <param name="request">Payload tạo tenant từ Owner Super Admin.</param>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service để gateway giữ status/body tương thích.</returns>
    Task<HttpResponseMessage> CreateTenantAsync(
        CreateTenantRequest request,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gửi request liệt kê tenant sang Tenant Service.
    /// </summary>
    /// <param name="status">Filter trạng thái tenant nếu caller truyền.</param>
    /// <param name="search">Từ khóa tìm kiếm nếu caller truyền.</param>
    /// <param name="limit">Số bản ghi tối đa nếu caller truyền.</param>
    /// <param name="offset">Vị trí bắt đầu trang nếu caller truyền.</param>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service để gateway giữ status/body tương thích.</returns>
    Task<HttpResponseMessage> ListTenantsAsync(
        string? status,
        string? search,
        int? limit,
        int? offset,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gửi request đọc chi tiết tenant sang Tenant Service.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần đọc.</param>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service để gateway giữ status/body tương thích.</returns>
    Task<HttpResponseMessage> GetTenantByIdAsync(
        Guid tenantId,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gửi request đổi trạng thái tenant sang Tenant Service.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần cập nhật.</param>
    /// <param name="request">Payload chứa trạng thái mới.</param>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service để gateway giữ status/body tương thích.</returns>
    Task<HttpResponseMessage> UpdateTenantStatusAsync(
        Guid tenantId,
        UpdateTenantStatusRequest request,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Forward request doc DNS/SSL state cua cac domain thuoc tenant sang Tenant Service.
    /// </summary>
    /// <param name="tenantId">Tenant dang duoc Owner Admin mo detail.</param>
    /// <param name="correlationId">Correlation id can propagate sang Tenant Service neu co.</param>
    /// <param name="cancellationToken">Token huy request forwarding.</param>
    /// <returns>HTTP response goc tu Tenant Service.</returns>
    Task<HttpResponseMessage> ListTenantDomainDnsSslStatesAsync(
        Guid tenantId,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Forward request retry DNS cho mot domain sang Tenant Service.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="domainId">Domain can retry DNS.</param>
    /// <param name="correlationId">Correlation id can propagate sang Tenant Service neu co.</param>
    /// <param name="cancellationToken">Token huy request forwarding.</param>
    /// <returns>HTTP response goc tu Tenant Service.</returns>
    Task<HttpResponseMessage> RetryTenantDomainDnsAsync(
        Guid tenantId,
        Guid domainId,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Forward request doc SSL state cua mot domain sang Tenant Service.
    /// </summary>
    /// <param name="tenantId">Tenant so huu domain.</param>
    /// <param name="domainId">Domain can doc SSL state.</param>
    /// <param name="correlationId">Correlation id can propagate sang Tenant Service neu co.</param>
    /// <param name="cancellationToken">Token huy request forwarding.</param>
    /// <returns>HTTP response goc tu Tenant Service.</returns>
    Task<HttpResponseMessage> GetTenantDomainSslStatusAsync(
        Guid tenantId,
        Guid domainId,
        string? correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Forward request đọc owner plan catalog sang Tenant Service.
    /// </summary>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    Task<HttpResponseMessage> ListOwnerPlansAsync(string? correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Forward request đọc owner module matrix sang Tenant Service.
    /// </summary>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    Task<HttpResponseMessage> ListOwnerModulesAsync(string? correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Forward request đọc owner tenant-plan assignments sang Tenant Service.
    /// </summary>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    Task<HttpResponseMessage> ListOwnerTenantPlanAssignmentsAsync(string? correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Forward request bulk-change owner tenant plan sang Tenant Service.
    /// </summary>
    /// <param name="request">Payload bulk-change do Owner Admin gửi lên.</param>
    /// <param name="correlationId">Correlation id cần propagate sang Tenant Service nếu có.</param>
    /// <param name="cancellationToken">Token hủy request forwarding.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    Task<HttpResponseMessage> BulkChangeOwnerTenantPlansAsync(
        BulkChangeTenantPlanRequest request,
        string? correlationId,
        CancellationToken cancellationToken);
}
