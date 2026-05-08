using ApiGateway.Domain;
using ClinicSaaS.BuildingBlocks.Tenancy;

namespace ApiGateway.Tests;

public sealed class ProjectStructureSmokeTests
{
    public bool ApplicationAndDomainReferencesAreAvailable()
    {
        return typeof(ITenantContextAccessor).Assembly.GetName().Name == "ClinicSaaS.BuildingBlocks"
            && AssemblyReference.Assembly.GetName().Name == "ApiGateway.Domain";
    }
}
