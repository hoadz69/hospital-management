using System.Reflection;

namespace TenantService.Domain;

/// <summary>
/// Tham chiếu assembly Domain để các project khác/test có thể kiểm tra reference.
/// </summary>
public static class AssemblyReference
{
    /// <summary>
    /// Assembly chứa domain model của Tenant Service.
    /// </summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
