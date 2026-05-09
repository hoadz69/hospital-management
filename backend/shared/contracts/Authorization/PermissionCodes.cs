namespace ClinicSaaS.Contracts.Authorization;

/// <summary>
/// Permission code chuẩn để gắn vào API boundary và kiểm soát quyền về sau.
/// </summary>
public static class PermissionCodes
{
    /// <summary>
    /// Quyền đọc tenant ở phạm vi platform.
    /// </summary>
    public const string TenantsRead = "tenants.read";

    /// <summary>
    /// Quyền tạo/cập nhật tenant ở phạm vi platform.
    /// </summary>
    public const string TenantsWrite = "tenants.write";

    /// <summary>
    /// Quyền đọc user placeholder.
    /// </summary>
    public const string UsersRead = "users.read";

    /// <summary>
    /// Quyền ghi user placeholder.
    /// </summary>
    public const string UsersWrite = "users.write";

    /// <summary>
    /// Tập permission hợp lệ hiện có trong contract.
    /// </summary>
    public static readonly string[] All =
    [
        TenantsRead,
        TenantsWrite,
        UsersRead,
        UsersWrite
    ];
}
