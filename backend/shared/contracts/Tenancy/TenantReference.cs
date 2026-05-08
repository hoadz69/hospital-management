namespace ClinicSaaS.Contracts.Tenancy;

public sealed record TenantReference(string TenantId, string? Slug = null, string? DisplayName = null);
