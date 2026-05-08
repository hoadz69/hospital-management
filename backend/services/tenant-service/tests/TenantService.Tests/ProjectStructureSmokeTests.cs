using ClinicSaaS.BuildingBlocks.Tenancy;
using TenantService.Domain;

namespace TenantService.Tests;

public sealed class ProjectStructureSmokeTests
{
    public bool ApplicationAndDomainReferencesAreAvailable()
    {
        return typeof(ITenantContextAccessor).Assembly.GetName().Name == "ClinicSaaS.BuildingBlocks"
            && AssemblyReference.Assembly.GetName().Name == "TenantService.Domain";
    }
}
