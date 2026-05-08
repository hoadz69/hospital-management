using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.Contracts.Security;
using ClinicSaaS.Contracts.Tenancy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicSaaS.BuildingBlocks.Authorization;

public class AuthRbacPlaceholderMiddleware(RequestDelegate next)
{
    public const string ContextItemKey = "ClinicSaaS.AuthRbacPlaceholder";

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextAccessor tenantContextAccessor,
        IUserContextAccessor userContextAccessor)
    {
        var endpoint = context.GetEndpoint();
        var requiredRoles = endpoint?.Metadata
            .GetOrderedMetadata<RequiredRoleMetadata>()
            .SelectMany(metadata => metadata.Roles)
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];
        var requiredPermissions = endpoint?.Metadata
            .GetOrderedMetadata<RequiredPermissionMetadata>()
            .SelectMany(metadata => metadata.Permissions)
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];
        var tenantScope = endpoint?.Metadata.GetMetadata<TenantScopeMetadata>()?.Scope
            ?? TenantEndpointScope.Unspecified;
        var userContext = ResolveUserContext(context, tenantContextAccessor.Current);

        userContextAccessor.SetCurrent(userContext);
        context.Items[ContextItemKey] = new AuthRbacPlaceholderContext(
            endpoint?.DisplayName,
            tenantScope.ToString(),
            requiredRoles,
            requiredPermissions,
            userContext,
            HasRequiredRoles: requiredRoles.Length == 0 || requiredRoles.All(userContext.HasRole),
            HasRequiredPermissions: requiredPermissions.Length == 0 || requiredPermissions.All(userContext.HasPermission),
            HasRequiredTenantContext: tenantScope != TenantEndpointScope.Tenant || tenantContextAccessor.HasTenant,
            "metadata-only-placeholder-not-enforced");

        await next(context);
    }

    private static UserContext ResolveUserContext(HttpContext context, TenantContext tenantContext)
    {
        var user = context.User;
        var roles = ReadClaimValues(user, ClaimTypes.Role, "role", "roles").ToArray();
        var permissions = ReadClaimValues(user, "permission", "permissions", "scope", "scp").ToArray();
        var userId = FirstClaimValue(user, ClaimTypes.NameIdentifier, "sub", "user_id");
        var email = FirstClaimValue(user, ClaimTypes.Email, "email");
        var tenant = tenantContext.HasTenant
            ? new TenantReference(tenantContext.TenantId!, tenantContext.TenantId!)
            : null;

        if (string.IsNullOrWhiteSpace(userId)
            && string.IsNullOrWhiteSpace(email)
            && roles.Length == 0
            && permissions.Length == 0
            && tenant is null)
        {
            return UserContext.Anonymous;
        }

        return new UserContext(
            userId,
            email,
            tenant,
            roles,
            permissions,
            user.Identity?.IsAuthenticated == true,
            Source: "http-context-claims-placeholder");
    }

    private static string? FirstClaimValue(ClaimsPrincipal user, params string[] claimTypes)
    {
        return claimTypes
            .Select(claimType => user.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static IEnumerable<string> ReadClaimValues(ClaimsPrincipal user, params string[] claimTypes)
    {
        return claimTypes
            .SelectMany(claimType => user.FindAll(claimType))
            .SelectMany(claim => claim.Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal);
    }
}
