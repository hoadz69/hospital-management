using System.Reflection;

namespace TemplateService.Domain;

/// <summary>
/// Marker dùng để test và resolve assembly Template Service Domain layer.
/// </summary>
public static class AssemblyReference
{
    /// <summary>
    /// Assembly chứa domain model của Template Service.
    /// </summary>
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
