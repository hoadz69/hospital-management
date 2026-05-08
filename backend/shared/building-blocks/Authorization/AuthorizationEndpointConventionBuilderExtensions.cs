using Microsoft.AspNetCore.Builder;

namespace ClinicSaaS.BuildingBlocks.Authorization;

public static class AuthorizationEndpointConventionBuilderExtensions
{
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new RequiredRoleMetadata(Normalize(roles, nameof(roles))));

        return builder;
    }

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
