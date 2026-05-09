using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;

namespace TenantService.Application.Tenants;

/// <summary>
/// Use case liệt kê tenant theo filter platform-scoped.
/// </summary>
public sealed class ListTenantsHandler
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>
    /// Khởi tạo handler với repository liệt kê tenant.
    /// </summary>
    /// <param name="tenantRepository">Repository dùng để truy vấn danh sách tenant.</param>
    public ListTenantsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// Liệt kê tenant theo filter trạng thái/search và phân trang bảo thủ.
    /// </summary>
    /// <param name="status">Trạng thái tenant cần lọc; null nghĩa là không lọc theo trạng thái.</param>
    /// <param name="search">Từ khóa tìm trong slug, display name hoặc clinic name.</param>
    /// <param name="limit">Số bản ghi tối đa caller yêu cầu, được clamp trong khoảng 1-100.</param>
    /// <param name="offset">Vị trí bắt đầu trang, giá trị âm được đưa về 0.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối hoặc host dừng.</param>
    /// <returns>Kết quả chứa trang tenant hoặc lỗi validation nếu status không hợp lệ.</returns>
    public async Task<Result<TenantListResponse>> HandleAsync(
        string? status,
        string? search,
        int? limit,
        int? offset,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(status) && !TenantStatusParser.TryParse(status, out var parsedStatus))
        {
            return Result<TenantListResponse>.Failure(TenantErrors.Validation(new Dictionary<string, string[]>
            {
                [nameof(status)] = ["Status must be one of: Draft, Active, Suspended, Archived."]
            }));
        }

        TenantStatusParser.TryParse(status, out var tenantStatus);
        var effectiveLimit = Math.Clamp(limit ?? 50, 1, 100);
        var effectiveOffset = Math.Max(offset ?? 0, 0);
        var page = await _tenantRepository.ListAsync(
            new TenantListQuery(
                string.IsNullOrWhiteSpace(status) ? null : tenantStatus,
                string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                effectiveLimit,
                effectiveOffset),
            cancellationToken);

        return Result<TenantListResponse>.Success(new TenantListResponse(
            page.Items.Select(TenantResponseMapper.ToSummaryResponse).ToArray(),
            page.Total,
            page.Limit,
            page.Offset));
    }
}
