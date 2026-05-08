namespace ClinicSaaS.Contracts.Authorization;

public static class RoleNames
{
    public const string OwnerSuperAdmin = "owner.super_admin";
    public const string ClinicAdmin = "clinic.admin";
    public const string PublicTenant = "public.tenant";

    public static readonly string[] All =
    [
        OwnerSuperAdmin,
        ClinicAdmin,
        PublicTenant
    ];
}
