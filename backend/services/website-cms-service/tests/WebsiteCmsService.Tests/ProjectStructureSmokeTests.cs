using ClinicSaaS.BuildingBlocks.Tenancy;
using WebsiteCmsService.Domain;
using Xunit;

namespace WebsiteCmsService.Tests;

/// <summary>
/// Smoke test xác nhận project references và assembly identity của Website CMS Service.
/// </summary>
public sealed class ProjectStructureSmokeTests
{
    /// <summary>
    /// Đảm bảo Domain assembly load được và shared BuildingBlocks đúng tên assembly.
    /// </summary>
    [Fact]
    public void AssemblyHasExpectedNamespace()
    {
        var buildingBlocksName = typeof(ITenantContextAccessor).Assembly.GetName().Name;
        var domainName = AssemblyReference.Assembly.GetName().Name;

        Assert.Equal("ClinicSaaS.BuildingBlocks", buildingBlocksName);
        Assert.Equal("WebsiteCmsService.Domain", domainName);
    }
}
