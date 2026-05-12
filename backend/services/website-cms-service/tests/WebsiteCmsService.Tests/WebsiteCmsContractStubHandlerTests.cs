using ClinicSaaS.BuildingBlocks.Tenancy;
using WebsiteCmsService.Application.Website;
using Xunit;

namespace WebsiteCmsService.Tests;

/// <summary>
/// Unit test nhỏ cho contract/stub handler của Website CMS Service.
/// </summary>
public sealed class WebsiteCmsContractStubHandlerTests
{
    /// <summary>
    /// Xác nhận settings dummy trả đúng tenant khi context khớp route.
    /// </summary>
    [Fact]
    public void GetSettings_TenantContextMatches_ReturnsSettings()
    {
        var tenantId = Guid.NewGuid();
        var accessor = new TenantContextAccessor();
        accessor.SetCurrent(new TenantContext(tenantId.ToString(), "test"));
        var handler = new WebsiteCmsContractStubHandler(accessor);

        var result = handler.GetSettings(tenantId);

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value!.TenantId);
    }

    /// <summary>
    /// Xác nhận page key ngoài contract trả not found rõ ràng.
    /// </summary>
    [Fact]
    public void GetPage_UnknownPageKey_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        var accessor = new TenantContextAccessor();
        accessor.SetCurrent(new TenantContext(tenantId.ToString(), "test"));
        var handler = new WebsiteCmsContractStubHandler(accessor);

        var result = handler.GetPage(tenantId, "unknown");

        Assert.False(result.IsSuccess);
        Assert.Equal("website_cms.not_found", result.Error.Code);
    }
}
