using System.Text.Json;
using ClinicSaaS.Contracts.Domains;
using Dapper;
using TenantService.Application.Domains;

namespace TenantService.Infrastructure.Persistence;

/// <summary>
/// Repository Dapper/Npgsql doc va cap nhat DNS/SSL state tren `platform.tenant_domains`.
/// </summary>
public sealed class DapperTenantDomainOperationsRepository : ITenantDomainOperationsRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IPostgreSqlConnectionFactory _connectionFactory;

    /// <summary>
    /// Khoi tao repository voi connection factory cua Tenant Service.
    /// </summary>
    /// <param name="connectionFactory">Factory tao ket noi PostgreSQL da mo.</param>
    public DapperTenantDomainOperationsRepository(IPostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    /// <inheritdoc />
    public async Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            """
            select exists (
                select 1
                from platform.tenants
                where id = @TenantId
            );
            """,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DomainDnsSslStateResponse>> ListDomainsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<DomainOperationRow>(new CommandDefinition(
            $"""
            {SelectDomainOperationColumns()}
            where d.tenant_id = @TenantId
            order by d.is_primary desc, d.created_at_utc;
            """,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));

        return rows.Select(ToResponse).ToArray();
    }

    /// <inheritdoc />
    public async Task<DomainDnsSslStateResponse?> GetDomainAsync(
        Guid tenantId,
        Guid domainId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<DomainOperationRow>(new CommandDefinition(
            $"""
            {SelectDomainOperationColumns()}
            where d.tenant_id = @TenantId
              and d.id = @DomainId;
            """,
            new { TenantId = tenantId, DomainId = domainId },
            cancellationToken: cancellationToken));

        return row is null ? null : ToResponse(row);
    }

    /// <inheritdoc />
    public async Task<DomainDnsSslStateResponse?> RetryDnsAsync(
        Guid tenantId,
        Guid domainId,
        DateTimeOffset now,
        DateTimeOffset nextRetryAt,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<DomainOperationRow>(new CommandDefinition(
            """
            update platform.tenant_domains d
            set
                retry_count = d.retry_count + 1,
                last_checked_at_utc = @Now,
                next_retry_at_utc = @NextRetryAt,
                dns_status = case
                    when d.dns_status = 'verified' then 'verified'
                    else 'propagating'
                end,
                status_message = case
                    when d.dns_status = 'verified' then 'DNS already verified. Retry timestamp updated.'
                    else 'DNS retry accepted. Next automated check scheduled.'
                end,
                dns_records = coalesce(d.dns_records, jsonb_build_array(jsonb_build_object(
                    'recordType', 'CNAME',
                    'host', d.domain_name,
                    'expectedValue', 'cname.clinicos.local',
                    'actualValue', null,
                    'status', case when d.dns_status = 'verified' then 'verified' else 'propagating' end,
                    'message', 'CNAME should point to the Clinic SaaS gateway.'
                )))
            where d.tenant_id = @TenantId
              and d.id = @DomainId
            returning
                d.id as domain_id,
                d.domain_name,
                d.dns_status,
                coalesce(d.dns_records, '[]'::jsonb)::text as dns_records_json,
                d.last_checked_at_utc as last_checked_at,
                d.retry_count,
                d.next_retry_at_utc as next_retry_at,
                d.ssl_status,
                d.ssl_issuer,
                d.expires_at_utc as expires_at,
                d.status_message as message;
            """,
            new
            {
                TenantId = tenantId,
                DomainId = domainId,
                Now = now,
                NextRetryAt = nextRetryAt
            },
            cancellationToken: cancellationToken));

        return row is null ? null : ToResponse(row);
    }

    private static string SelectDomainOperationColumns()
    {
        return """
            select
                d.id as domain_id,
                d.domain_name,
                coalesce(d.dns_status, case when d.status = 'Active' then 'verified' else 'pending' end) as dns_status,
                coalesce(d.dns_records, jsonb_build_array(jsonb_build_object(
                    'recordType', 'CNAME',
                    'host', d.domain_name,
                    'expectedValue', 'cname.clinicos.local',
                    'actualValue', null,
                    'status', case when d.status = 'Active' then 'verified' else 'pending' end,
                    'message', 'CNAME should point to the Clinic SaaS gateway.'
                )))::text as dns_records_json,
                coalesce(d.last_checked_at_utc, d.verified_at_utc, d.created_at_utc) as last_checked_at,
                coalesce(d.retry_count, 0) as retry_count,
                d.next_retry_at_utc as next_retry_at,
                coalesce(d.ssl_status, case when d.status = 'Active' then 'issued' else 'pending' end) as ssl_status,
                d.ssl_issuer,
                d.expires_at_utc as expires_at,
                coalesce(d.status_message, case
                    when d.status = 'Active' then 'Domain DNS verified and SSL state is available.'
                    else 'Domain is waiting for DNS propagation and SSL provisioning.'
                end) as message
            from platform.tenant_domains d
            """;
    }

    private static DomainDnsSslStateResponse ToResponse(DomainOperationRow row)
    {
        return new DomainDnsSslStateResponse(
            row.DomainId,
            row.DomainName,
            row.DnsStatus,
            DeserializeDnsRecords(row.DnsRecordsJson),
            row.LastCheckedAt,
            row.RetryCount,
            row.NextRetryAt,
            row.SslStatus,
            row.SslIssuer,
            row.ExpiresAt,
            row.Message);
    }

    private static IReadOnlyList<DomainDnsRecordResponse> DeserializeDnsRecords(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<DomainDnsRecordResponse>();
        }

        return JsonSerializer.Deserialize<DomainDnsRecordResponse[]>(json, JsonOptions)
            ?? Array.Empty<DomainDnsRecordResponse>();
    }

    private sealed class DomainOperationRow
    {
        public Guid DomainId { get; set; }

        public string DomainName { get; set; } = string.Empty;

        public string DnsStatus { get; set; } = string.Empty;

        public string? DnsRecordsJson { get; set; }

        public DateTimeOffset? LastCheckedAt { get; set; }

        public int RetryCount { get; set; }

        public DateTimeOffset? NextRetryAt { get; set; }

        public string SslStatus { get; set; } = string.Empty;

        public string? SslIssuer { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }

        public string? Message { get; set; }
    }
}
