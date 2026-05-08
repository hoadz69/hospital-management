using ClinicSaaS.Contracts.Tenancy;

namespace ClinicSaaS.Contracts.Security;

public sealed record UserContext(
    string? UserId,
    string? Email,
    TenantReference? Tenant,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    bool IsAuthenticated,
    string Source)
{
    public static UserContext Anonymous { get; } = new(
        null,
        null,
        null,
        [],
        [],
        IsAuthenticated: false,
        Source: "anonymous-placeholder");

    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.Ordinal);
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission, StringComparer.Ordinal);
    }
}
