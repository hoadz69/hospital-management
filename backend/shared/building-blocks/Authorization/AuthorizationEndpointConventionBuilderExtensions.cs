using Microsoft.AspNetCore.Builder;

namespace ClinicSaaS.BuildingBlocks.Authorization;

/// <summary>
/// Extension gắn metadata role/permission cho API boundary.
/// </summary>
public static class AuthorizationEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Gắn danh sách role yêu cầu cho endpoint.
    /// </summary>
    /// <param name="builder">Endpoint convention builder cần gắn metadata.</param>
    /// <param name="roles">Danh sách role được phép truy cập endpoint.</param>
    /// <returns>Chính builder đã truyền vào để tiếp tục chain cấu hình endpoint.</returns>
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new RequiredRoleMetadata(Normalize(roles, nameof(roles))));

        return builder;
    }

    /// <summary>
    /// Gắn danh sách permission yêu cầu cho endpoint.
    /// </summary>
    /// <param name="builder">Endpoint convention builder cần gắn metadata.</param>
    /// <param name="permissions">Danh sách permission bắt buộc cho hành động của endpoint.</param>
    /// <returns>Chính builder đã truyền vào để tiếp tục chain cấu hình endpoint.</returns>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new RequiredPermissionMetadata(Normalize(permissions, nameof(permissions))));

        return builder;
    }

    private static string[] Normalize(IEnumerable<string> values, string parameterName)
    {
        var normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", parameterName);
        }

        return normalized;
    }
}
