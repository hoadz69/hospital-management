namespace TenantService.Domain.Tenants;

/// <summary>
/// Tenant gốc đại diện cho một phòng khám trong nền tảng SaaS.
/// </summary>
public sealed class Tenant
{
    /// <summary>
    /// Dựng lại tenant từ dữ liệu đã persist hoặc từ factory tạo mới.
    /// </summary>
    /// <param name="id">Định danh tenant.</param>
    /// <param name="slug">Slug đã chuẩn hóa của tenant.</param>
    /// <param name="displayName">Tên hiển thị của tenant trên platform.</param>
    /// <param name="status">Trạng thái vòng đời hiện tại.</param>
    /// <param name="plan">Snapshot tham chiếu gói dịch vụ của tenant.</param>
    /// <param name="profile">Hồ sơ phòng khám thuộc tenant.</param>
    /// <param name="domains">Danh sách domain/subdomain gắn với tenant.</param>
    /// <param name="modules">Danh sách module đang cấu hình cho tenant.</param>
    /// <param name="createdAtUtc">Thời điểm tạo tenant theo UTC.</param>
    /// <param name="updatedAtUtc">Thời điểm cập nhật tenant gần nhất theo UTC.</param>
    /// <param name="activatedAtUtc">Thời điểm tenant được active lần đầu theo UTC.</param>
    /// <param name="suspendedAtUtc">Thời điểm tenant bị suspend gần nhất theo UTC.</param>
    /// <param name="archivedAtUtc">Thời điểm tenant được archive theo UTC.</param>
    public Tenant(
        Guid id,
        string slug,
        string displayName,
        TenantStatus status,
        PlanReference plan,
        ClinicProfile profile,
        IReadOnlyList<TenantDomain> domains,
        IReadOnlyList<TenantModule> modules,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        DateTimeOffset? activatedAtUtc,
        DateTimeOffset? suspendedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Slug = slug;
        DisplayName = displayName;
        Status = status;
        Plan = plan;
        Profile = profile;
        Domains = domains;
        Modules = modules;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ActivatedAtUtc = activatedAtUtc;
        SuspendedAtUtc = suspendedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    /// <summary>
    /// Định danh tenant.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Slug đã chuẩn hóa của tenant.
    /// </summary>
    public string Slug { get; }

    /// <summary>
    /// Tên hiển thị tenant trên platform.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Trạng thái vòng đời hiện tại.
    /// </summary>
    public TenantStatus Status { get; }

    /// <summary>
    /// Snapshot tham chiếu gói dịch vụ.
    /// </summary>
    public PlanReference Plan { get; }

    /// <summary>
    /// Hồ sơ phòng khám thuộc tenant.
    /// </summary>
    public ClinicProfile Profile { get; }

    /// <summary>
    /// Danh sách domain/subdomain gắn với tenant.
    /// </summary>
    public IReadOnlyList<TenantDomain> Domains { get; }

    /// <summary>
    /// Danh sách module được cấu hình cho tenant.
    /// </summary>
    public IReadOnlyList<TenantModule> Modules { get; }

    /// <summary>
    /// Thời điểm tạo tenant theo UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// Thời điểm cập nhật tenant gần nhất theo UTC.
    /// </summary>
    public DateTimeOffset? UpdatedAtUtc { get; }

    /// <summary>
    /// Thời điểm tenant được active lần đầu theo UTC.
    /// </summary>
    public DateTimeOffset? ActivatedAtUtc { get; }

    /// <summary>
    /// Thời điểm tenant bị suspend gần nhất theo UTC.
    /// </summary>
    public DateTimeOffset? SuspendedAtUtc { get; }

    /// <summary>
    /// Thời điểm tenant được archive theo UTC.
    /// </summary>
    public DateTimeOffset? ArchivedAtUtc { get; }

    /// <summary>
    /// Tạo tenant mới ở trạng thái Draft cùng profile, domain mặc định và module ban đầu.
    /// </summary>
    /// <param name="slug">Slug owner nhập, sẽ được chuẩn hóa trước khi lưu.</param>
    /// <param name="displayName">Tên hiển thị của tenant trên platform.</param>
    /// <param name="plan">Snapshot tham chiếu gói dịch vụ tại thời điểm tạo tenant.</param>
    /// <param name="profile">Dữ liệu nháp để dựng hồ sơ phòng khám.</param>
    /// <param name="defaultDomainName">Domain/subdomain mặc định dùng để tạo tenant domain đầu tiên.</param>
    /// <param name="moduleCodes">Danh sách mã module cần bật ban đầu.</param>
    /// <param name="now">Thời điểm nghiệp vụ hiện tại theo UTC.</param>
    /// <returns>Tenant mới có profile, domain mặc định và module ban đầu đã chuẩn hóa.</returns>
    public static Tenant Create(
        string slug,
        string displayName,
        PlanReference plan,
        ClinicProfileDraft profile,
        string defaultDomainName,
        IEnumerable<string> moduleCodes,
        DateTimeOffset now)
    {
        var tenantId = Guid.NewGuid();
        var normalizedSlug = TenantNormalization.NormalizeSlug(slug);
        var normalizedDomain = TenantNormalization.NormalizeDomain(defaultDomainName);
        var distinctModules = moduleCodes
            .Select(TenantNormalization.NormalizeModuleCode)
            .Where(moduleCode => moduleCode.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (distinctModules.Length == 0)
        {
            distinctModules = ["website"];
        }

        var profileEntity = new ClinicProfile(
            tenantId,
            TenantNormalization.Required(profile.ClinicName, nameof(profile.ClinicName)),
            TenantNormalization.Optional(profile.ContactEmail),
            TenantNormalization.Optional(profile.PhoneNumber),
            TenantNormalization.Optional(profile.AddressLine),
            TenantNormalization.Optional(profile.Specialty));

        var domains = new[]
        {
            new TenantDomain(
                Guid.NewGuid(),
                tenantId,
                normalizedDomain,
                normalizedDomain,
                TenantDomainType.DefaultSubdomain,
                TenantDomainStatus.Pending,
                IsPrimary: true,
                now,
                VerifiedAtUtc: null)
        };

        var modules = distinctModules
            .Select(moduleCode => new TenantModule(
                tenantId,
                moduleCode,
                IsEnabled: true,
                SourcePlanCode: plan.PlanCode,
                now,
                UpdatedAtUtc: null))
            .ToArray();

        return new Tenant(
            tenantId,
            normalizedSlug,
            TenantNormalization.Required(displayName, nameof(displayName)),
            TenantStatus.Draft,
            plan,
            profileEntity,
            domains,
            modules,
            now,
            updatedAtUtc: null,
            activatedAtUtc: null,
            suspendedAtUtc: null,
            archivedAtUtc: null);
    }
}

/// <summary>
/// Dữ liệu nháp để dựng hồ sơ phòng khám khi tạo tenant.
/// </summary>
/// <param name="ClinicName">Tên phòng khám bắt buộc khi tạo tenant.</param>
/// <param name="ContactEmail">Email liên hệ của phòng khám nếu có.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ của phòng khám nếu có.</param>
/// <param name="AddressLine">Địa chỉ phòng khám nếu có.</param>
/// <param name="Specialty">Chuyên khoa chính của phòng khám nếu có.</param>
public sealed record ClinicProfileDraft(
    string ClinicName,
    string? ContactEmail,
    string? PhoneNumber,
    string? AddressLine,
    string? Specialty);
