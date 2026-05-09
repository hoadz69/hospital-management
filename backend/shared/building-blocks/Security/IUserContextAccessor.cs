using ClinicSaaS.Contracts.Security;

namespace ClinicSaaS.BuildingBlocks.Security;

/// <summary>
/// Truy cập user context đã resolve cho request hiện tại.
/// </summary>
public interface IUserContextAccessor
{
    /// <summary>
    /// User context hiện tại của request.
    /// </summary>
    UserContext Current { get; }

    /// <summary>
    /// Gán user context cho request hiện tại.
    /// </summary>
    /// <param name="userContext">User context đã resolve từ middleware hoặc test.</param>
    void SetCurrent(UserContext userContext);
}
