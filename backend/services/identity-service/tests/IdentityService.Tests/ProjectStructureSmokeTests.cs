using ClinicSaaS.BuildingBlocks.Tenancy;
using IdentityService.Domain;

namespace IdentityService.Tests;

public sealed class ProjectStructureSmokeTests
{
    public bool ApplicationAndDomainReferencesAreAvailable()
    {
        return typeof(ITenantContextAccessor).Assembly.GetName().Name == "ClinicSaaS.BuildingBlocks"
            && AssemblyReference.Assembly.GetName().Name == "IdentityService.Domain";
    }
}
