namespace ClinicSaaS.Contracts.Authorization;

public static class PermissionCodes
{
    public const string TenantsRead = "tenants.read";
    public const string TenantsWrite = "tenants.write";
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";

    public static readonly string[] All =
    [
        TenantsRead,
        TenantsWrite,
        UsersRead,
        UsersWrite
    ];
}
