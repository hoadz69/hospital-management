using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;

namespace TenantService.Application.Tenants;

/// <summary>
/// Use case đọc chi tiết tenant cho Owner Super Admin.
/// </summary>
public sealed class GetTenantByIdHandler
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>
    /// Khởi tạo handler với repository đọc tenant.
    /// </summary>
    /// <param name="tenantRepository">Repository dùng để truy vấn tenant.</param>
    public GetTenantByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// Đọc chi tiết tenant theo id ở phạm vi platform.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần đọc.</param>
    /// <param name="cancellationToken">Token hủy request khi client ngắt kết nối hoặc host dừng.</param>
    /// <returns>Kết quả chứa tenant response hoặc lỗi not found.</returns>
    public async Task<Result<TenantResponse>> HandleAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        return tenant is null
            ? Result<TenantResponse>.Failure(TenantErrors.NotFound(tenantId))
            : Result<TenantResponse>.Success(TenantResponseMapper.ToResponse(tenant));
    }
}
