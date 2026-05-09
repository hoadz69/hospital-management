using ClinicSaaS.Contracts.Security;

namespace ClinicSaaS.BuildingBlocks.Authorization;

/// <summary>
/// Metadata role bắt buộc được gắn trên endpoint.
/// </summary>
/// <param name="Roles">Danh sách role được endpoint yêu cầu.</param>
public sealed record RequiredRoleMetadata(IReadOnlyCollection<string> Roles);

/// <summary>
/// Metadata permission bắt buộc được gắn trên endpoint.
/// </summary>
/// <param name="Permissions">Danh sách permission được endpoint yêu cầu.</param>
public sealed record RequiredPermissionMetadata(IReadOnlyCollection<string> Permissions);

/// <summary>
/// Context RBAC placeholder để inspect metadata trong giai đoạn chưa có provider auth thật.
/// </summary>
/// <param name="EndpointDisplayName">Tên endpoint hiện tại nếu ASP.NET Core metadata có cung cấp.</param>
/// <param name="TenantScope">Phạm vi tenant của endpoint sau khi đọc metadata.</param>
/// <param name="RequiredRoles">Role endpoint yêu cầu.</param>
/// <param name="RequiredPermissions">Permission endpoint yêu cầu.</param>
/// <param name="UserContext">User context placeholder được resolve từ HttpContext.</param>
/// <param name="HasRequiredRoles">Cho biết user context hiện có đủ role theo metadata hay không.</param>
/// <param name="HasRequiredPermissions">Cho biết user context hiện có đủ permission theo metadata hay không.</param>
/// <param name="HasRequiredTenantContext">Cho biết request tenant-scoped đã có tenant context hay chưa.</param>
/// <param name="EnforcementMode">Mô tả chế độ enforce hiện tại, Phase 1/2 vẫn là metadata-only.</param>
public sealed record AuthRbacPlaceholderContext(
    string? EndpointDisplayName,
    string TenantScope,
    IReadOnlyCollection<string> RequiredRoles,
    IReadOnlyCollection<string> RequiredPermissions,
    UserContext UserContext,
    bool HasRequiredRoles,
    bool HasRequiredPermissions,
    bool HasRequiredTenantContext,
    string EnforcementMode);
