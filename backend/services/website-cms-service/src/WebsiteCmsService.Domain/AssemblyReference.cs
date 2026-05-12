using System.Reflection;

namespace WebsiteCmsService.Domain;

/// <summary>
/// Marker dùng để test và resolve assembly Website CMS Service Domain layer.
/// </summary>
public static class AssemblyReference
{
    /// <summary>
    /// Assembly chứa domain model của Website CMS Service.
    /// </summary>
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
