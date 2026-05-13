using ClinicSaaS.Contracts.Domains;
using TenantService.Application.Domains;
using Xunit;

namespace TenantService.Tests;

/// <summary>
/// Unit test cho handler Domain DNS/SSL state dung repository persistence port.
/// </summary>
public sealed class TenantDomainOperationsHandlerTests
{
    /// <summary>
    /// Xac nhan list domains can tenant ton tai va tra dung response FE can de bo mock.
    /// </summary>
    [Fact]
    public async Task ListDomainsAsync_ExistingTenant_ReturnsDnsSslFields()
    {
        var tenantId = Guid.NewGuid();
        var repository = new FakeTenantDomainOperationsRepository(tenantId);
        var handler = new TenantDomainOperationsHandler(repository);

        var result = await handler.ListDomainsAsync(tenantId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(repository.DomainId, item.DomainId);
        Assert.Equal("pending", item.DnsStatus);
        Assert.Equal("pending", item.SslStatus);
        Assert.Single(item.DnsRecords);
    }

    /// <summary>
    /// Xac nhan tenant id rong tra validation 400 thay vi query DB mo ho.
    /// </summary>
    [Fact]
    public async Task ListDomainsAsync_EmptyTenantId_ReturnsValidation()
    {
        var handler = new TenantDomainOperationsHandler(new FakeTenantDomainOperationsRepository(Guid.NewGuid()));

        var result = await handler.ListDomainsAsync(Guid.Empty, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("domains.validation", result.Error.Code);
        Assert.Contains("tenantId", result.Error.Details!.Keys);
    }

    /// <summary>
    /// Xac nhan retry DNS tang retry count va tra state moi tu repository.
    /// </summary>
    [Fact]
    public async Task RetryDnsAsync_ExistingDomain_ReturnsUpdatedState()
    {
        var tenantId = Guid.NewGuid();
        var repository = new FakeTenantDomainOperationsRepository(tenantId);
        var handler = new TenantDomainOperationsHandler(repository);

        var result = await handler.RetryDnsAsync(tenantId, repository.DomainId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.RetryCount);
        Assert.Equal("propagating", result.Value.DnsStatus);
        Assert.NotNull(result.Value.LastCheckedAt);
        Assert.NotNull(result.Value.NextRetryAt);
    }

    /// <summary>
    /// Xac nhan domain khong thuoc tenant tra 404 ro rang.
    /// </summary>
    [Fact]
    public async Task GetSslStatusAsync_MissingDomain_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        var handler = new TenantDomainOperationsHandler(new FakeTenantDomainOperationsRepository(tenantId));

        var result = await handler.GetSslStatusAsync(tenantId, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("domains.not_found", result.Error.Code);
        Assert.Contains("Domain", result.Error.Message);
    }

    private sealed class FakeTenantDomainOperationsRepository : ITenantDomainOperationsRepository
    {
        private readonly Guid _tenantId;

        public FakeTenantDomainOperationsRepository(Guid tenantId)
        {
            _tenantId = tenantId;
            DomainId = Guid.NewGuid();
        }

        public Guid DomainId { get; }

        public Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(tenantId == _tenantId);

        public Task<IReadOnlyList<DomainDnsSslStateResponse>> ListDomainsAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DomainDnsSslStateResponse>>(
                tenantId == _tenantId ? [BuildResponse(0, "pending")] : []);

        public Task<DomainDnsSslStateResponse?> GetDomainAsync(
            Guid tenantId,
            Guid domainId,
            CancellationToken cancellationToken)
            => Task.FromResult<DomainDnsSslStateResponse?>(
                tenantId == _tenantId && domainId == DomainId ? BuildResponse(0, "pending") : null);

        public Task<DomainDnsSslStateResponse?> RetryDnsAsync(
            Guid tenantId,
            Guid domainId,
            DateTimeOffset now,
            DateTimeOffset nextRetryAt,
            CancellationToken cancellationToken)
            => Task.FromResult<DomainDnsSslStateResponse?>(
                tenantId == _tenantId && domainId == DomainId
                    ? BuildResponse(1, "propagating", now, nextRetryAt)
                    : null);

        private DomainDnsSslStateResponse BuildResponse(
            int retryCount,
            string dnsStatus,
            DateTimeOffset? lastCheckedAt = null,
            DateTimeOffset? nextRetryAt = null)
            => new(
                DomainId,
                "demo.clinicos.local",
                dnsStatus,
                [new DomainDnsRecordResponse(
                    "CNAME",
                    "demo.clinicos.local",
                    "cname.clinicos.local",
                    null,
                    dnsStatus,
                    "CNAME should point to the Clinic SaaS gateway.")],
                lastCheckedAt,
                retryCount,
                nextRetryAt,
                "pending",
                null,
                null,
                "Domain is waiting for DNS propagation and SSL provisioning.");
    }
}
