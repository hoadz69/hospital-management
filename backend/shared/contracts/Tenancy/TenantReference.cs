namespace ClinicSaaS.Contracts.Tenancy;

/// <summary>
/// Tham chiếu tenant gọn nhẹ dùng trong contract, event và user context.
/// </summary>
/// <param name="TenantId">Định danh tenant được truyền giữa services hoặc trong user context.</param>
/// <param name="Slug">Slug tenant nếu caller cần hiển thị hoặc log ngữ cảnh.</param>
/// <param name="DisplayName">Tên hiển thị của tenant nếu caller cần hiển thị cho người dùng.</param>
public sealed record TenantReference(string TenantId, string? Slug = null, string? DisplayName = null);
