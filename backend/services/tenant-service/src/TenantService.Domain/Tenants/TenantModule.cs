namespace TenantService.Domain.Tenants;

/// <summary>
/// Module chức năng được bật/tắt cho tenant theo gói hoặc cấu hình platform.
/// </summary>
/// <param name="TenantId">Định danh tenant sở hữu cấu hình module.</param>
/// <param name="ModuleCode">Mã module được bật/tắt.</param>
/// <param name="IsEnabled">Cho biết module đang bật hay tắt.</param>
/// <param name="SourcePlanCode">Mã gói là nguồn bật module nếu có.</param>
/// <param name="CreatedAtUtc">Thời điểm cấu hình module được tạo theo UTC.</param>
/// <param name="UpdatedAtUtc">Thời điểm cấu hình module được cập nhật theo UTC.</param>
public sealed record TenantModule(
    Guid TenantId,
    string ModuleCode,
    bool IsEnabled,
    string? SourcePlanCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
