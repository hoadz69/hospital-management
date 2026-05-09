using ClinicSaaS.Contracts.Security;

namespace ClinicSaaS.BuildingBlocks.Security;

/// <summary>
/// Lưu user context placeholder cho các layer sau RBAC middleware.
/// </summary>
public sealed class UserContextAccessor : IUserContextAccessor
{
    /// <summary>
    /// User context hiện tại của request.
    /// </summary>
    public UserContext Current { get; private set; } = UserContext.Anonymous;

    /// <summary>
    /// Gán user context đã resolve cho request hiện tại.
    /// </summary>
    /// <param name="userContext">User context được RBAC middleware hoặc test thiết lập.</param>
    public void SetCurrent(UserContext userContext)
    {
        Current = userContext;
    }
}
