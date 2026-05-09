using System.Net.Http.Json;
using ApiGateway.Application.Tenants;
using ClinicSaaS.Contracts.Tenancy;

namespace ApiGateway.Infrastructure.Tenants;

/// <summary>
/// HTTP client implementation dùng để API Gateway forward tenant APIs sang Tenant Service.
/// </summary>
public sealed class TenantServiceClient : ITenantServiceClient
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Khởi tạo client với HttpClient đã được cấu hình base address trong DI.
    /// </summary>
    /// <param name="httpClient">HttpClient typed client trỏ tới Tenant Service.</param>
    public TenantServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Forward request tạo tenant tới Tenant Service.
    /// </summary>
    /// <param name="request">Payload tạo tenant từ Owner Super Admin.</param>
    /// <param name="correlationId">Correlation id cần propagate nếu có.</param>
    /// <param name="cancellationToken">Token hủy request HTTP.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    public Task<HttpResponseMessage> CreateTenantAsync(
        CreateTenantRequest request,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/tenants")
        {
            Content = JsonContent.Create(request)
        };
        AddCorrelationId(message, correlationId);

        return _httpClient.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Forward request liệt kê tenant tới Tenant Service.
    /// </summary>
    /// <param name="status">Filter trạng thái tenant nếu caller truyền.</param>
    /// <param name="search">Từ khóa tìm kiếm nếu caller truyền.</param>
    /// <param name="limit">Số bản ghi tối đa nếu caller truyền.</param>
    /// <param name="offset">Vị trí bắt đầu trang nếu caller truyền.</param>
    /// <param name="correlationId">Correlation id cần propagate nếu có.</param>
    /// <param name="cancellationToken">Token hủy request HTTP.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    public Task<HttpResponseMessage> ListTenantsAsync(
        string? status,
        string? search,
        int? limit,
        int? offset,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Get,
            BuildListTenantsPath(status, search, limit, offset));
        AddCorrelationId(message, correlationId);

        return _httpClient.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Forward request đọc chi tiết tenant tới Tenant Service.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần đọc.</param>
    /// <param name="correlationId">Correlation id cần propagate nếu có.</param>
    /// <param name="cancellationToken">Token hủy request HTTP.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    public Task<HttpResponseMessage> GetTenantByIdAsync(
        Guid tenantId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, $"/api/tenants/{tenantId}");
        AddCorrelationId(message, correlationId);

        return _httpClient.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Forward request đổi trạng thái tenant tới Tenant Service.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần cập nhật.</param>
    /// <param name="request">Payload chứa trạng thái mới.</param>
    /// <param name="correlationId">Correlation id cần propagate nếu có.</param>
    /// <param name="cancellationToken">Token hủy request HTTP.</param>
    /// <returns>HTTP response gốc từ Tenant Service.</returns>
    public Task<HttpResponseMessage> UpdateTenantStatusAsync(
        Guid tenantId,
        UpdateTenantStatusRequest request,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Patch, $"/api/tenants/{tenantId}/status")
        {
            Content = JsonContent.Create(request)
        };
        AddCorrelationId(message, correlationId);

        return _httpClient.SendAsync(message, cancellationToken);
    }

    private static void AddCorrelationId(HttpRequestMessage message, string? correlationId)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            message.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, correlationId);
        }
    }

    private static string BuildListTenantsPath(string? status, string? search, int? limit, int? offset)
    {
        var query = new List<string>();
        AddQuery(query, nameof(status), status);
        AddQuery(query, nameof(search), search);
        AddQuery(query, nameof(limit), limit?.ToString());
        AddQuery(query, nameof(offset), offset?.ToString());

        return query.Count == 0
            ? "/api/tenants"
            : $"/api/tenants?{string.Join("&", query)}";
    }

    private static void AddQuery(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
        }
    }
}
