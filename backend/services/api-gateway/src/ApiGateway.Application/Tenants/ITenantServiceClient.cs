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
}
