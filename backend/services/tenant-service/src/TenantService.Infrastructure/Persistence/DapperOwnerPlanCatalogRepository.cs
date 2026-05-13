using ClinicSaaS.BuildingBlocks.Results;
using ClinicSaaS.Contracts.Tenancy;
using Dapper;
using TenantService.Application.Plans;

namespace TenantService.Infrastructure.Persistence;

/// <summary>
/// Repository Dapper/Npgsql đọc plan catalog và ghi tenant-plan assignment trong schema `platform`.
/// </summary>
public sealed class DapperOwnerPlanCatalogRepository : IOwnerPlanCatalogRepository
{
    private const string EffectiveAtNextRenewal = "next_renewal";

    private readonly IPostgreSqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Khởi tạo repository với connection factory của Tenant Service.
    /// </summary>
    /// <param name="connectionFactory">Factory tạo kết nối PostgreSQL đã mở.</param>
    public DapperOwnerPlanCatalogRepository(IPostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    /// <inheritdoc />
    public async Task<OwnerPlanCatalogResponse> ListPlansAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PlanCatalogRow>(new CommandDefinition(
            """
            select
                p.plan_code as code,
                p.name,
                p.price_monthly as price,
                coalesce(p.description, '') as description,
                count(a.tenant_id)::int as tenant_count,
                coalesce(p.tone, 'neutral') as tone,
                p.is_popular as popular
            from platform.plans p
            left join platform.tenant_plan_assignments a on a.current_plan_code = p.plan_code
            where p.is_active = true
            group by p.plan_code, p.name, p.price_monthly, p.description, p.tone, p.is_popular, p.display_order
            order by p.display_order, p.plan_code;
            """,
            cancellationToken: cancellationToken));

        return new OwnerPlanCatalogResponse(rows.Select(row => new OwnerPlanCatalogItemResponse(
            row.Code,
            row.Name,
            row.Price,
            row.Description,
            row.TenantCount,
            row.Tone,
            row.Popular)).ToArray());
    }

    /// <inheritdoc />
    public async Task<OwnerModuleCatalogResponse> ListModulesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = (await connection.QueryAsync<ModuleEntitlementRow>(new CommandDefinition(
            """
            select
                m.module_code,
                m.name,
                m.category,
                e.plan_code,
                e.is_enabled,
                e.display_value,
                e.limit_value,
                m.display_order
            from platform.modules m
            join platform.plan_module_entitlements e on e.module_code = m.module_code
            join platform.plans p on p.plan_code = e.plan_code
            where m.is_active = true
              and p.is_active = true
            order by m.display_order, m.module_code, p.display_order;
            """,
            cancellationToken: cancellationToken))).ToArray();
        var items = rows
            .GroupBy(row => new { row.ModuleCode, row.Name, row.Category, row.DisplayOrder })
            .OrderBy(group => group.Key.DisplayOrder)
            .ThenBy(group => group.Key.ModuleCode, StringComparer.Ordinal)
            .Select(group => new OwnerModuleCatalogItemResponse(
                group.Key.ModuleCode,
                group.Key.Name,
                group.Key.Category,
                EntitlementValue(group, PlanCodes.Starter),
                EntitlementValue(group, PlanCodes.Growth),
                EntitlementValue(group, PlanCodes.Premium)))
            .ToArray();

        return new OwnerModuleCatalogResponse(items);
    }

    /// <inheritdoc />
    public async Task<TenantPlanAssignmentListResponse> ListTenantPlanAssignmentsAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<TenantPlanAssignmentRow>(new CommandDefinition(
            """
            select
                a.tenant_id::text as id,
                t.slug,
                a.current_plan_code as current_plan,
                p.name as current_plan_name,
                a.current_mrr,
                coalesce(to_char(a.next_renewal_date, 'DD/MM/YYYY'), 'next renewal') as next_renewal,
                false as selected,
                coalesce(a.target_plan_code, a.current_plan_code) as target_plan
            from platform.tenant_plan_assignments a
            join platform.tenants t on t.id = a.tenant_id
            join platform.plans p on p.plan_code = a.current_plan_code
            order by t.created_at_utc desc, t.slug;
            """,
            cancellationToken: cancellationToken));

        return new TenantPlanAssignmentListResponse(rows.Select(row => new TenantPlanAssignmentResponse(
            row.Id,
            row.Slug,
            row.CurrentPlan,
            row.CurrentPlanName,
            row.CurrentMrr,
            row.NextRenewal,
            row.Selected,
            row.TargetPlan)).ToArray());
    }

    /// <inheritdoc />
    public async Task<Result<BulkChangeTenantPlanResponse>> BulkChangeTenantPlansAsync(
        BulkChangeTenantPlanRequest request,
        string? actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var selectedTenantIds = request.SelectedTenantIds
            .Select(Guid.Parse)
            .Distinct()
            .ToArray();
        var auditReason = request.AuditReason.Trim();
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var targetPlan = await connection.QuerySingleOrDefaultAsync<PlanPriceRow>(new CommandDefinition(
                """
                select plan_code, name, price_monthly
                from platform.plans
                where plan_code = @TargetPlan
                  and is_active = true;
                """,
                new { request.TargetPlan },
                transaction,
                cancellationToken: cancellationToken));
            if (targetPlan is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<BulkChangeTenantPlanResponse>.Failure(
                    OwnerPlanCatalogErrors.NotFound($"Target plan '{request.TargetPlan}' was not found."));
            }

            var assignments = (await connection.QueryAsync<AssignmentForUpdateRow>(new CommandDefinition(
                """
                select
                    tenant_id,
                    current_plan_code,
                    current_mrr
                from platform.tenant_plan_assignments
                where tenant_id = any(@TenantIds)
                for update;
                """,
                new { TenantIds = selectedTenantIds },
                transaction,
                cancellationToken: cancellationToken))).ToArray();
            var foundIds = assignments.Select(assignment => assignment.TenantId).ToHashSet();
            var missingIds = selectedTenantIds.Where(id => !foundIds.Contains(id)).ToArray();
            if (missingIds.Length > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<BulkChangeTenantPlanResponse>.Failure(
                    OwnerPlanCatalogErrors.NotFound("One or more tenant plan assignments were not found."));
            }

            var operationId = Guid.NewGuid();
            var mrrDiff = assignments.Sum(assignment => targetPlan.PriceMonthly - assignment.CurrentMrr);
            await connection.ExecuteAsync(new CommandDefinition(
                """
                update platform.tenant_plan_assignments
                set
                    current_plan_code = @TargetPlan,
                    target_plan_code = @TargetPlan,
                    effective_at = @EffectiveAt,
                    current_mrr = @TargetPrice,
                    audit_reason = @AuditReason,
                    assigned_by_user_id = @ActorUserId,
                    assigned_at_utc = @Now,
                    updated_at_utc = @Now,
                    version = version + 1
                where tenant_id = any(@TenantIds);
                """,
                new
                {
                    TargetPlan = targetPlan.PlanCode,
                    TargetPrice = targetPlan.PriceMonthly,
                    EffectiveAt = EffectiveAtNextRenewal,
                    AuditReason = auditReason,
                    ActorUserId = actorUserId,
                    Now = now,
                    TenantIds = selectedTenantIds
                },
                transaction,
                cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(
                """
                update platform.tenants
                set
                    plan_code = @TargetPlan,
                    plan_display_name = @TargetPlanName,
                    updated_at_utc = @Now
                where id = any(@TenantIds);
                """,
                new
                {
                    TargetPlan = targetPlan.PlanCode,
                    TargetPlanName = targetPlan.Name,
                    Now = now,
                    TenantIds = selectedTenantIds
                },
                transaction,
                cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(
                """
                delete from platform.tenant_modules
                where tenant_id = any(@TenantIds);
                """,
                new { TenantIds = selectedTenantIds },
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
                select
                    tenant_id,
                    entitlement.module_code,
                    true,
                    @TargetPlan,
                    @Now,
                    @Now
                from unnest(@TenantIds) as selected(tenant_id)
                cross join (
                    select module_code
                    from platform.plan_module_entitlements
                    where plan_code = @TargetPlan
                      and is_enabled = true
                ) entitlement;
                """,
                new
                {
                    TenantIds = selectedTenantIds,
                    TargetPlan = targetPlan.PlanCode,
                    Now = now
                },
                transaction,
                cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into platform.tenant_plan_assignment_changes (
                    id,
                    bulk_operation_id,
                    tenant_id,
                    from_plan_code,
                    to_plan_code,
                    effective_at,
                    audit_reason,
                    actor_user_id,
                    created_at_utc)
                values (
                    @Id,
                    @BulkOperationId,
                    @TenantId,
                    @FromPlanCode,
                    @ToPlanCode,
                    @EffectiveAt,
                    @AuditReason,
                    @ActorUserId,
                    @CreatedAtUtc);
                """,
                assignments.Select(assignment => new
                {
                    Id = Guid.NewGuid(),
                    BulkOperationId = operationId,
                    assignment.TenantId,
                    FromPlanCode = assignment.CurrentPlanCode,
                    ToPlanCode = targetPlan.PlanCode,
                    EffectiveAt = EffectiveAtNextRenewal,
                    AuditReason = auditReason,
                    ActorUserId = actorUserId,
                    CreatedAtUtc = now
                }),
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            return Result<BulkChangeTenantPlanResponse>.Success(new BulkChangeTenantPlanResponse(
                assignments.Length,
                mrrDiff,
                assignments.Length == 0 ? "noop" : "accepted-db",
                assignments.Length == 0
                    ? "No matching tenants were found."
                    : $"{assignments.Length} tenant plan changes persisted for next renewal.",
                EffectiveAtNextRenewal,
                auditReason));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static object EntitlementValue(
        IEnumerable<ModuleEntitlementRow> rows,
        string planCode)
    {
        var row = rows.FirstOrDefault(candidate => string.Equals(candidate.PlanCode, planCode, StringComparison.Ordinal));
        if (row is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(row.DisplayValue))
        {
            return row.DisplayValue;
        }

        if (!string.IsNullOrWhiteSpace(row.LimitValue))
        {
            return row.LimitValue;
        }

        return row.IsEnabled;
    }

    private sealed record PlanCatalogRow(
        string Code,
        string Name,
        decimal Price,
        string Description,
        int TenantCount,
        string Tone,
        bool Popular);

    private sealed record ModuleEntitlementRow(
        string ModuleCode,
        string Name,
        string Category,
        string PlanCode,
        bool IsEnabled,
        string? DisplayValue,
        string? LimitValue,
        int DisplayOrder);

    private sealed record TenantPlanAssignmentRow(
        string Id,
        string Slug,
        string CurrentPlan,
        string CurrentPlanName,
        decimal CurrentMrr,
        string NextRenewal,
        bool Selected,
        string TargetPlan);

    private sealed record PlanPriceRow(string PlanCode, string Name, decimal PriceMonthly);

    private sealed record AssignmentForUpdateRow(Guid TenantId, string CurrentPlanCode, decimal CurrentMrr);
}
