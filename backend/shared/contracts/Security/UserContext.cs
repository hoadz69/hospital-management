using ClinicSaaS.Contracts.Tenancy;

namespace ClinicSaaS.Contracts.Security;

public sealed record UserContext(
    string UserId,
    string Email,
    TenantReference? Tenant,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
