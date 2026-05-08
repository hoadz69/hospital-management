using ClinicSaaS.Contracts.Security;

namespace ClinicSaaS.BuildingBlocks.Security;

public interface IUserContextAccessor
{
    UserContext Current { get; }

    void SetCurrent(UserContext userContext);
}
