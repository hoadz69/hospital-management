namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Metadata gắn vào endpoint để middleware biết endpoint là platform-scoped hay tenant-scoped.
/// </summary>
/// <param name="Scope">Phạm vi tenant mà endpoint yêu cầu khi xử lý request.</param>
public sealed record TenantScopeMetadata(TenantEndpointScope Scope);
