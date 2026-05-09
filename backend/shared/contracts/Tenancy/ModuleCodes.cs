namespace ClinicSaaS.Contracts.Tenancy;

/// <summary>
/// Mã module dùng để bật/tắt năng lực theo từng tenant.
/// </summary>
public static class ModuleCodes
{
    /// <summary>
    /// Module website public của tenant.
    /// </summary>
    public const string Website = "website";

    /// <summary>
    /// Module đặt lịch hẹn.
    /// </summary>
    public const string Booking = "booking";

    /// <summary>
    /// Module danh mục dịch vụ, bảng giá và nhân sự.
    /// </summary>
    public const string Catalog = "catalog";

    /// <summary>
    /// Module khách hàng/hồ sơ.
    /// </summary>
    public const string Customers = "customers";

    /// <summary>
    /// Module billing/thanh toán.
    /// </summary>
    public const string Billing = "billing";

    /// <summary>
    /// Module báo cáo.
    /// </summary>
    public const string Reports = "reports";

    /// <summary>
    /// Module thông báo.
    /// </summary>
    public const string Notifications = "notifications";

    /// <summary>
    /// Module mặc định khi tạo tenant mới trong Phase 2.
    /// </summary>
    public static readonly string[] DefaultTenantModules =
    [
        Website,
        Booking,
        Catalog
    ];

    /// <summary>
    /// Tập mã module hợp lệ mà các service có thể tham chiếu.
    /// </summary>
    public static readonly string[] All =
    [
        Website,
        Booking,
        Catalog,
        Customers,
        Billing,
        Reports,
        Notifications
    ];
}
