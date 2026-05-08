using ClinicSaaS.Contracts.Security;

namespace ClinicSaaS.BuildingBlocks.Security;

public sealed class UserContextAccessor : IUserContextAccessor
{
    public UserContext Current { get; private set; } = UserContext.Anonymous;

    public void SetCurrent(UserContext userContext)
    {
        Current = userContext;
    }
}
