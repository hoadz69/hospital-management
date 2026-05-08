using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ClinicSaaS.BuildingBlocks.Tenancy;

public class TenantContextMiddleware(RequestDelegate next)
{
    public const string TenantHeaderName = "X-Tenant-Id";
    public const string TenantClaimName = "tenant_id";

    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContextAccessor)
    {
        var endpointScope = context.GetEndpoint()?.Metadata.GetMetadata<TenantScopeMetadata>()?.Scope
            ?? TenantEndpointScope.Unspecified;
        var resolution = ResolveTenantContext(context);

        if (!string.IsNullOrWhiteSpace(resolution.FailureReason))
        {
            await WriteTenantProblemAsync(context, StatusCodes.Status400BadRequest, resolution.FailureReason);
            return;
        }

        if (endpointScope == TenantEndpointScope.Platform)
        {
            tenantContextAccessor.SetCurrent(TenantContext.Platform);
            await next(context);
            return;
        }

        tenantContextAccessor.SetCurrent(resolution.TenantContext);

        if (endpointScope == TenantEndpointScope.Tenant && !tenantContextAccessor.HasTenant)
        {
            await WriteTenantProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                $"Tenant-scoped endpoint requires tenant context from {TenantHeaderName} or JWT claim {TenantClaimName}.");
            return;
        }

        await next(context);
    }

    private static TenantResolutionResult ResolveTenantContext(HttpContext context)
    {
        var headerTenantId = context.Request.Headers[TenantHeaderName].FirstOrDefault();
        var principalClaimTenantId = context.User.FindFirst(TenantClaimName)?.Value;
        var jwtPlaceholderTenantId = string.IsNullOrWhiteSpace(principalClaimTenantId)
            ? TryReadTenantIdFromBearerTokenPlaceholder(context)
            : null;
        var claimTenantId = principalClaimTenantId ?? jwtPlaceholderTenantId;

        if (!string.IsNullOrWhiteSpace(headerTenantId)
            && !string.IsNullOrWhiteSpace(claimTenantId)
            && !string.Equals(headerTenantId.Trim(), claimTenantId.Trim(), StringComparison.Ordinal))
        {
            return TenantResolutionResult.NotResolved(
                $"{TenantHeaderName} does not match JWT claim {TenantClaimName}.");
        }

        if (!string.IsNullOrWhiteSpace(headerTenantId))
        {
            return TenantResolutionResult.Resolved(headerTenantId.Trim(), TenantHeaderName);
        }

        if (!string.IsNullOrWhiteSpace(principalClaimTenantId))
        {
            return TenantResolutionResult.Resolved(principalClaimTenantId.Trim(), $"jwt:{TenantClaimName}");
        }

        if (!string.IsNullOrWhiteSpace(jwtPlaceholderTenantId))
        {
            return TenantResolutionResult.Resolved(
                jwtPlaceholderTenantId.Trim(),
                $"jwt:{TenantClaimName}:unvalidated-placeholder");
        }

        return TenantResolutionResult.Unresolved();
    }

    private static string? TryReadTenantIdFromBearerTokenPlaceholder(HttpContext context)
    {
        var authorization = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authorization)
            || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authorization["Bearer ".Length..].Trim();
        var parts = token.Split('.');

        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(DecodeBase64Url(parts[1]));
            using var payload = JsonDocument.Parse(payloadJson);

            if (payload.RootElement.TryGetProperty(TenantClaimName, out var tenantClaim)
                && tenantClaim.ValueKind == JsonValueKind.String)
            {
                return tenantClaim.GetString();
            }
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }

        return null;
    }

    private static byte[] DecodeBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');

        base64 = (base64.Length % 4) switch
        {
            0 => base64,
            2 => base64 + "==",
            3 => base64 + "=",
            _ => throw new FormatException("Invalid base64url payload length.")
        };

        return Convert.FromBase64String(base64);
    }

    private static Task WriteTenantProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(new
        {
            type = "https://clinicsaas.local/problems/tenant-context",
            title = "Tenant context required",
            status = statusCode,
            detail
        });
    }
}
