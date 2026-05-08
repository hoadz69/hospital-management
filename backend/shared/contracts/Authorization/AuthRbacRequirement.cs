namespace ClinicSaaS.Contracts.Authorization;

public sealed record AuthRbacRequirement(string Role, string Permission, bool RequiresTenant);
