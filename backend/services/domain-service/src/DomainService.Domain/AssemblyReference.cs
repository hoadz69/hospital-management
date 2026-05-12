using System.Reflection;

namespace DomainService.Domain;

/// <summary>
/// Marker dùng để test và resolve assembly Domain Service Domain layer.
/// </summary>
public static class AssemblyReference
{
    /// <summary>
    /// Assembly chứa domain model của Domain Service.
    /// </summary>
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
