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
    /// Quyền đọc plan catalog, module entitlement và assignment ở phạm vi Owner Admin.
    /// </summary>
    public const string PlansRead = "plans.read";

    /// <summary>
    /// Quyền đổi plan/module assignment cho tenant ở phạm vi Owner Super Admin.
    /// </summary>
    public const string PlansWrite = "plans.write";

    /// <summary>
    /// Quyền đọc domain của tenant hoặc registry domain ở phạm vi platform.
    /// </summary>
    public const string DomainsRead = "domains.read";

    /// <summary>
    /// Quyền đăng ký, verify hoặc cập nhật domain của tenant.
    /// </summary>
    public const string DomainsWrite = "domains.write";

    /// <summary>
    /// Quyền đọc thư viện template toàn platform.
    /// </summary>
    public const string TemplatesRead = "templates.read";

    /// <summary>
    /// Quyền áp template cho tenant.
    /// </summary>
    public const string TemplatesWrite = "templates.write";

    /// <summary>
    /// Quyền đọc cấu hình Website CMS của tenant.
    /// </summary>
    public const string WebsiteCmsRead = "website-cms.read";

    /// <summary>
    /// Quyền cập nhật cấu hình Website CMS của tenant.
    /// </summary>
    public const string WebsiteCmsWrite = "website-cms.write";

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
        PlansRead,
        PlansWrite,
        DomainsRead,
        DomainsWrite,
        TemplatesRead,
        TemplatesWrite,
        WebsiteCmsRead,
        WebsiteCmsWrite,
        UsersRead,
        UsersWrite
    ];
}
