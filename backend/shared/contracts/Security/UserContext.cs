using ClinicSaaS.Contracts.Tenancy;

namespace ClinicSaaS.Contracts.Security;

/// <summary>
/// User context chia sẻ giữa services, hiện dùng cho RBAC placeholder và sẽ map từ auth provider thật sau.
/// </summary>
/// <param name="UserId">Định danh user từ auth provider hoặc claim nếu có.</param>
/// <param name="Email">Email user nếu auth provider cung cấp.</param>
/// <param name="Tenant">Tenant gắn với user context nếu request là tenant-scoped.</param>
/// <param name="Roles">Danh sách role đã resolve từ claims hoặc placeholder.</param>
/// <param name="Permissions">Danh sách permission đã resolve từ claims hoặc placeholder.</param>
/// <param name="IsAuthenticated">Cho biết user đã authenticated theo HttpContext hay chưa.</param>
/// <param name="Source">Nguồn tạo user context để debug/auth hardening về sau.</param>
public sealed record UserContext(
    string? UserId,
    string? Email,
    TenantReference? Tenant,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    bool IsAuthenticated,
    string Source)
{
    /// <summary>
    /// User ẩn danh khi request chưa có thông tin auth hợp lệ.
    /// </summary>
    public static UserContext Anonymous { get; } = new(
        null,
        null,
        null,
        [],
        [],
        IsAuthenticated: false,
        Source: "anonymous-placeholder");

    /// <summary>
    /// Kiểm tra user context hiện tại có role yêu cầu hay không.
    /// </summary>
    /// <param name="role">Role cần kiểm tra.</param>
    /// <returns>`true` nếu role nằm trong danh sách role đã resolve; ngược lại là `false`.</returns>
    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.Ordinal);
    }

    /// <summary>
    /// Kiểm tra user context hiện tại có permission yêu cầu hay không.
    /// </summary>
    /// <param name="permission">Permission cần kiểm tra.</param>
    /// <returns>`true` nếu permission nằm trong danh sách permission đã resolve; ngược lại là `false`.</returns>
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission, StringComparer.Ordinal);
    }
}
