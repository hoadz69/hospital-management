using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Domains;
using DomainService.Application.Domains;
using Xunit;

namespace DomainService.Tests;

/// <summary>
/// Unit test nhỏ cho contract/stub handler của Domain Service.
/// </summary>
public sealed class DomainContractStubHandlerTests
{
    /// <summary>
    /// Xác nhận handler trả list domain khi tenant context khớp route.
    /// </summary>
    [Fact]
    public void ListDomains_TenantContextMatches_ReturnsStubItems()
    {
        var tenantId = Guid.NewGuid();
        var accessor = new TenantContextAccessor();
        accessor.SetCurrent(new TenantContext(tenantId.ToString(), "test"));
        var handler = new DomainContractStubHandler(accessor);

        var result = handler.ListDomains(tenantId);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Items);
    }

    /// <summary>
    /// Xác nhận handler trả conflict có field detail khi domain đã bị chiếm trong stub.
    /// </summary>
    [Fact]
    public void RegisterDomain_DomainTaken_ReturnsConflict()
    {
        var tenantId = Guid.NewGuid();
        var accessor = new TenantContextAccessor();
        accessor.SetCurrent(new TenantContext(tenantId.ToString(), "test"));
        var handler = new DomainContractStubHandler(accessor);

        var result = handler.RegisterDomain(tenantId, new RegisterDomainRequest("taken.clinicos.local", "CustomDomain", true));

        Assert.False(result.IsSuccess);
        Assert.Equal("domains.conflict", result.Error.Code);
        Assert.Contains("domainName", result.Error.Details!.Keys);
    }
}
