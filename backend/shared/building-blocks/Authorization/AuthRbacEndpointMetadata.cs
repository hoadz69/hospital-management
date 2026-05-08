using ClinicSaaS.Contracts.Security;

namespace ClinicSaaS.BuildingBlocks.Authorization;

public sealed record RequiredRoleMetadata(IReadOnlyCollection<string> Roles);

public sealed record RequiredPermissionMetadata(IReadOnlyCollection<string> Permissions);

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
