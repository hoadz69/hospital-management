namespace ClinicSaaS.Contracts.Authorization;

/// <summary>
/// Role chuẩn dùng trong metadata RBAC giữa gateway và services.
/// </summary>
public static class RoleNames
{
    /// <summary>
    /// Role quản trị toàn platform, được phép thao tác cross-tenant.
    /// </summary>
    public const string OwnerSuperAdmin = "owner.super_admin";

    /// <summary>
    /// Role quản trị trong phạm vi một tenant/phòng khám.
    /// </summary>
    public const string ClinicAdmin = "clinic.admin";

    /// <summary>
    /// Role placeholder cho public tenant website.
    /// </summary>
    public const string PublicTenant = "public.tenant";

    /// <summary>
    /// Tập role hợp lệ hiện có trong contract.
    /// </summary>
    public static readonly string[] All =
    [
        OwnerSuperAdmin,
        ClinicAdmin,
        PublicTenant
    ];
}
