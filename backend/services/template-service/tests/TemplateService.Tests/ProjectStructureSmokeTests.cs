using ClinicSaaS.BuildingBlocks.Tenancy;
using TemplateService.Domain;
using Xunit;

namespace TemplateService.Tests;

/// <summary>
/// Smoke test xác nhận project references và assembly identity của Template Service.
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
        Assert.Equal("TemplateService.Domain", domainName);
    }
}
