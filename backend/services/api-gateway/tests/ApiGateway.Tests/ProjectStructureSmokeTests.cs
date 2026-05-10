using ApiGateway.Domain;
using ClinicSaaS.BuildingBlocks.Tenancy;
using Xunit;

namespace ApiGateway.Tests;

/// <summary>
/// Smoke test tối thiểu xác nhận project references và assembly identity của API Gateway
/// được resolve đúng sau khi build, dùng làm sanity gate cho test runner pickup.
/// </summary>
public sealed class ProjectStructureSmokeTests
{
    /// <summary>
    /// Đảm bảo Domain assembly của API Gateway load được trong test host và trỏ đúng marker
    /// <see cref="AssemblyReference"/>; bảo vệ cấu hình ProjectReference trong csproj không bị đứt.
    /// </summary>
    [Fact]
    public void AssemblyLoads_DomainAssemblyResolvable()
    {
        var domainAssembly = AssemblyReference.Assembly;

        Assert.NotNull(domainAssembly);
        Assert.NotNull(domainAssembly.FullName);
    }

    /// <summary>
    /// Xác nhận tên assembly của BuildingBlocks và Domain đúng convention Clinic SaaS,
    /// phòng trường hợp ai đó đổi AssemblyName/RootNamespace của shared layer hoặc Domain layer.
    /// </summary>
    [Fact]
    public void AssemblyHasExpectedNamespace()
    {
        var buildingBlocksName = typeof(ITenantContextAccessor).Assembly.GetName().Name;
        var domainName = AssemblyReference.Assembly.GetName().Name;

        Assert.Equal("ClinicSaaS.BuildingBlocks", buildingBlocksName);
        Assert.Equal("ApiGateway.Domain", domainName);
    }
}
