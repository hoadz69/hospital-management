using ClinicSaaS.BuildingBlocks.Results;
using Dapper;
using Npgsql;
using TenantService.Application.Tenants;
using TenantService.Domain.Tenants;

namespace TenantService.Infrastructure.Persistence;

/// <summary>
/// Repository Dapper/Npgsql lưu và truy vấn tenant trong schema `platform`.
/// </summary>
public sealed class DapperTenantRepository : ITenantRepository
{
    private readonly IPostgreSqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Khởi tạo repository với connection factory của Tenant Service.
    /// </summary>
    /// <param name="connectionFactory">Factory tạo kết nối PostgreSQL đã mở.</param>
    public DapperTenantRepository(IPostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    /// <summary>
    /// Lưu tenant mới cùng profile, domain và modules trong một PostgreSQL transaction.
    /// </summary>
    /// <param name="tenant">Aggregate tenant đã được validate và chuẩn hóa ở domain/application.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa tenant đã lưu hoặc lỗi conflict khi unique constraint bị vi phạm.</returns>
    public async Task<Result<Tenant>> CreateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into platform.tenants (
                    id,
                    slug,
                    display_name,
                    status,
                    plan_code,
                    plan_display_name,
                    created_at_utc,
                    updated_at_utc,
                    activated_at_utc,
                    suspended_at_utc,
                    archived_at_utc)
                values (
                    @Id,
                    @Slug,
                    @DisplayName,
                    @Status,
                    @PlanCode,
                    @PlanDisplayName,
                    @CreatedAtUtc,
                    @UpdatedAtUtc,
                    @ActivatedAtUtc,
                    @SuspendedAtUtc,
                    @ArchivedAtUtc);
                """,
                ToTenantParameters(tenant),
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into platform.tenant_profiles (
                    tenant_id,
                    clinic_name,
                    contact_email,
                    phone_number,
                    address_line,
                    specialty)
                values (
                    @TenantId,
                    @ClinicName,
                    @ContactEmail,
                    @PhoneNumber,
                    @AddressLine,
                    @Specialty);
                """,
                tenant.Profile,
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into platform.tenant_domains (
                    id,
                    tenant_id,
                    domain_name,
                    normalized_domain_name,
                    domain_type,
                    status,
                    is_primary,
                    created_at_utc,
                    verified_at_utc)
                values (
                    @Id,
                    @TenantId,
                    @DomainName,
                    @NormalizedDomainName,
                    @DomainType,
                    @Status,
                    @IsPrimary,
                    @CreatedAtUtc,
                    @VerifiedAtUtc);
                """,
                tenant.Domains.Select(domain => new
                {
                    domain.Id,
                    domain.TenantId,
                    domain.DomainName,
                    domain.NormalizedDomainName,
                    DomainType = domain.DomainType.ToString(),
                    Status = domain.Status.ToString(),
                    domain.IsPrimary,
                    domain.CreatedAtUtc,
                    domain.VerifiedAtUtc
                }),
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into platform.tenant_modules (
                    tenant_id,
                    module_code,
                    is_enabled,
                    source_plan_code,
                    created_at_utc,
                    updated_at_utc)
                values (
                    @TenantId,
                    @ModuleCode,
                    @IsEnabled,
                    @SourcePlanCode,
                    @CreatedAtUtc,
                    @UpdatedAtUtc);
                """,
                tenant.Modules,
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            return Result<Tenant>.Success(tenant);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Tenant>.Failure(TenantErrors.Conflict("Tenant slug or domain already exists."));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Đọc tenant đầy đủ theo id từ các bảng `platform.tenants` và bảng con.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần đọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Tenant nếu tồn tại; null nếu không tìm thấy.</returns>
    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await LoadTenantAsync(connection, tenantId, cancellationToken);
    }

    /// <summary>
    /// Liệt kê tenant theo filter status/search và phân trang.
    /// </summary>
    /// <param name="query">Query đã được Application layer validate và normalize.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Trang tenant và tổng số bản ghi khớp filter.</returns>
    public async Task<TenantListPage> ListAsync(TenantListQuery query, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("Limit", query.Limit);
        parameters.Add("Offset", query.Offset);

        var filters = new List<string>();
        if (query.Status is not null)
        {
            filters.Add("t.status = @Status");
            parameters.Add("Status", query.Status.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            filters.Add("(t.slug ilike @Search or t.display_name ilike @Search or p.clinic_name ilike @Search)");
            parameters.Add("Search", $"%{query.Search.Trim()}%");
        }

        var whereClause = filters.Count == 0
            ? string.Empty
            : $"where {string.Join(" and ", filters)}";
        var rows = (await connection.QueryAsync<TenantListRow>(new CommandDefinition(
            $"""
            select
                t.id,
                t.slug,
                t.display_name,
                t.status,
                t.plan_code,
                t.plan_display_name,
                t.created_at_utc,
                t.updated_at_utc,
                t.activated_at_utc,
                t.suspended_at_utc,
                t.archived_at_utc,
                p.clinic_name
            from platform.tenants t
            left join platform.tenant_profiles p on p.tenant_id = t.id
            {whereClause}
            order by t.created_at_utc desc
            limit @Limit offset @Offset;
            """,
            parameters,
            cancellationToken: cancellationToken))).ToArray();
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            $"""
            select count(*)
            from platform.tenants t
            left join platform.tenant_profiles p on p.tenant_id = t.id
            {whereClause};
            """,
            parameters,
            cancellationToken: cancellationToken));
        var tenants = rows.Select(ToTenantSummary).ToArray();

        return new TenantListPage(tenants, total, query.Limit, query.Offset);
    }

    /// <summary>
    /// Cập nhật trạng thái tenant và các mốc lifecycle tương ứng.
    /// </summary>
    /// <param name="tenantId">Định danh tenant cần cập nhật.</param>
    /// <param name="status">Trạng thái mới đã validate.</param>
    /// <param name="now">Thời điểm cập nhật theo UTC.</param>
    /// <param name="cancellationToken">Token hủy thao tác I/O.</param>
    /// <returns>Kết quả chứa tenant sau cập nhật hoặc lỗi not found.</returns>
    public async Task<Result<Tenant>> UpdateStatusAsync(
        Guid tenantId,
        TenantStatus status,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            """
            update platform.tenants
            set
                status = @Status,
                updated_at_utc = @Now,
                activated_at_utc = case when @Status = 'Active' then coalesce(activated_at_utc, @Now) else activated_at_utc end,
                suspended_at_utc = case when @Status = 'Suspended' then @Now else suspended_at_utc end,
                archived_at_utc = case when @Status = 'Archived' then @Now else archived_at_utc end
            where id = @TenantId;
            """,
            new
            {
                TenantId = tenantId,
                Status = status.ToString(),
                Now = now
            },
            cancellationToken: cancellationToken));

        if (affected == 0)
        {
            return Result<Tenant>.Failure(TenantErrors.NotFound(tenantId));
        }

        var tenant = await LoadTenantAsync(connection, tenantId, cancellationToken);
        return tenant is null
            ? Result<Tenant>.Failure(TenantErrors.NotFound(tenantId))
            : Result<Tenant>.Success(tenant);
    }

    private static object ToTenantParameters(Tenant tenant)
    {
        return new
        {
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            Status = tenant.Status.ToString(),
            PlanCode = tenant.Plan.PlanCode,
            PlanDisplayName = tenant.Plan.DisplayName,
            tenant.CreatedAtUtc,
            tenant.UpdatedAtUtc,
            tenant.ActivatedAtUtc,
            tenant.SuspendedAtUtc,
            tenant.ArchivedAtUtc
        };
    }

    private static async Task<Tenant?> LoadTenantAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var root = await connection.QuerySingleOrDefaultAsync<TenantRootRow>(new CommandDefinition(
            """
            select
                id,
                slug,
                display_name,
                status,
                plan_code,
                plan_display_name,
                created_at_utc,
                updated_at_utc,
                activated_at_utc,
                suspended_at_utc,
                archived_at_utc
            from platform.tenants
            where id = @TenantId;
            """,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));

        if (root is null)
        {
            return null;
        }

        var profile = await connection.QuerySingleAsync<ClinicProfileRow>(new CommandDefinition(
            """
            select
                tenant_id,
                clinic_name,
                contact_email,
                phone_number,
                address_line,
                specialty
            from platform.tenant_profiles
            where tenant_id = @TenantId;
            """,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));
        var domains = (await connection.QueryAsync<TenantDomainRow>(new CommandDefinition(
            """
            select
                id,
                tenant_id,
                domain_name,
                normalized_domain_name,
                domain_type,
                status,
                is_primary,
                created_at_utc,
                verified_at_utc
            from platform.tenant_domains
            where tenant_id = @TenantId
            order by is_primary desc, created_at_utc;
            """,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken))).Select(ToTenantDomain).ToArray();
        var modules = (await connection.QueryAsync<TenantModuleRow>(new CommandDefinition(
            """
            select
                tenant_id,
                module_code,
                is_enabled,
                source_plan_code,
                created_at_utc,
                updated_at_utc
            from platform.tenant_modules
            where tenant_id = @TenantId
            order by module_code;
            """,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken))).Select(ToTenantModule).ToArray();

        return ToTenant(root, ToClinicProfile(profile), domains, modules);
    }

    private static Tenant ToTenant(
        TenantRootRow root,
        ClinicProfile profile,
        IReadOnlyList<TenantDomain> domains,
        IReadOnlyList<TenantModule> modules)
    {
        return new Tenant(
            root.Id,
            root.Slug,
            root.DisplayName,
            Enum.Parse<TenantStatus>(root.Status),
            new PlanReference(root.PlanCode, root.PlanDisplayName),
            profile,
            domains,
            modules,
            root.CreatedAtUtc,
            root.UpdatedAtUtc,
            root.ActivatedAtUtc,
            root.SuspendedAtUtc,
            root.ArchivedAtUtc);
    }

    private static Tenant ToTenantSummary(TenantListRow row)
    {
        var profile = new ClinicProfile(
            row.Id,
            row.ClinicName ?? row.DisplayName,
            ContactEmail: null,
            PhoneNumber: null,
            AddressLine: null,
            Specialty: null);

        return new Tenant(
            row.Id,
            row.Slug,
            row.DisplayName,
            Enum.Parse<TenantStatus>(row.Status),
            new PlanReference(row.PlanCode, row.PlanDisplayName),
            profile,
            [],
            [],
            row.CreatedAtUtc,
            row.UpdatedAtUtc,
            row.ActivatedAtUtc,
            row.SuspendedAtUtc,
            row.ArchivedAtUtc);
    }

    private static ClinicProfile ToClinicProfile(ClinicProfileRow row)
    {
        return new ClinicProfile(
            row.TenantId,
            row.ClinicName,
            row.ContactEmail,
            row.PhoneNumber,
            row.AddressLine,
            row.Specialty);
    }

    private static TenantDomain ToTenantDomain(TenantDomainRow row)
    {
        return new TenantDomain(
            row.Id,
            row.TenantId,
            row.DomainName,
            row.NormalizedDomainName,
            Enum.Parse<TenantDomainType>(row.DomainType),
            Enum.Parse<TenantDomainStatus>(row.Status),
            row.IsPrimary,
            row.CreatedAtUtc,
            row.VerifiedAtUtc);
    }

    private static TenantModule ToTenantModule(TenantModuleRow row)
    {
        return new TenantModule(
            row.TenantId,
            row.ModuleCode,
            row.IsEnabled,
            row.SourcePlanCode,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }

    private sealed record TenantRootRow(
        Guid Id,
        string Slug,
        string DisplayName,
        string Status,
        string PlanCode,
        string? PlanDisplayName,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc,
        DateTimeOffset? ActivatedAtUtc,
        DateTimeOffset? SuspendedAtUtc,
        DateTimeOffset? ArchivedAtUtc);

    // Thứ tự positional record phải khớp thứ tự cột trong SELECT của ListAsync vì Dapper
    // 2.1 với positional record materialize theo position (type của tham số ctor phải khớp
    // type của cột reader cùng index). Đặt ClinicName ở cuối vì SQL list query JOIN với
    // tenant_profiles và đặt p.clinic_name sau cùng.
    private sealed record TenantListRow(
        Guid Id,
        string Slug,
        string DisplayName,
        string Status,
        string PlanCode,
        string? PlanDisplayName,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc,
        DateTimeOffset? ActivatedAtUtc,
        DateTimeOffset? SuspendedAtUtc,
        DateTimeOffset? ArchivedAtUtc,
        string? ClinicName);

    private sealed record ClinicProfileRow(
        Guid TenantId,
        string ClinicName,
        string? ContactEmail,
        string? PhoneNumber,
        string? AddressLine,
        string? Specialty);

    private sealed record TenantDomainRow(
        Guid Id,
        Guid TenantId,
        string DomainName,
        string NormalizedDomainName,
        string DomainType,
        string Status,
        bool IsPrimary,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? VerifiedAtUtc);

    private sealed record TenantModuleRow(
        Guid TenantId,
        string ModuleCode,
        bool IsEnabled,
        string? SourcePlanCode,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? UpdatedAtUtc);
}
