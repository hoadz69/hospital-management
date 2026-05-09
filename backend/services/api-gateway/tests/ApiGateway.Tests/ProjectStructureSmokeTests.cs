using ApiGateway.Domain;
using ClinicSaaS.BuildingBlocks.Tenancy;

namespace ApiGateway.Tests;

/// <summary>
/// Smoke test tối thiểu xác nhận project references của API Gateway còn đúng.
/// </summary>
public sealed class ProjectStructureSmokeTests
{
    /// <summary>
    /// Kiểm tra test project nhìn thấy BuildingBlocks và Domain assembly.
    /// </summary>
    /// <returns>`true` nếu các reference quan trọng được resolve đúng tên assembly.</returns>
    public bool ApplicationAndDomainReferencesAreAvailable()
    {
        return typeof(ITenantContextAccessor).Assembly.GetName().Name == "ClinicSaaS.BuildingBlocks"
            && AssemblyReference.Assembly.GetName().Name == "ApiGateway.Domain";
    }
}
