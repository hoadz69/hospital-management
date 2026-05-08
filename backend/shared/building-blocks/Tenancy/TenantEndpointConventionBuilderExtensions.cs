using Microsoft.AspNetCore.Builder;

namespace ClinicSaaS.BuildingBlocks.Tenancy;

public static class TenantEndpointConventionBuilderExtensions
{
    public static TBuilder AllowPlatformScope<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new TenantScopeMetadata(TenantEndpointScope.Platform));

        return builder;
    }

    public static TBuilder RequireTenantContext<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new TenantScopeMetadata(TenantEndpointScope.Tenant));

        return builder;
    }
}
