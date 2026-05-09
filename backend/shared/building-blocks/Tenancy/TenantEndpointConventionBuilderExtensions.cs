using Microsoft.AspNetCore.Builder;

namespace ClinicSaaS.BuildingBlocks.Tenancy;

/// <summary>
/// Extension gắn metadata tenant scope cho Minimal API endpoints.
/// </summary>
public static class TenantEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Đánh dấu endpoint thuộc phạm vi platform, không yêu cầu tenant context.
    /// </summary>
    /// <param name="builder">Endpoint convention builder cần gắn metadata.</param>
    /// <returns>Chính builder đã truyền vào để tiếp tục chain cấu hình endpoint.</returns>
    public static TBuilder AllowPlatformScope<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new TenantScopeMetadata(TenantEndpointScope.Platform));

        return builder;
    }

    /// <summary>
    /// Đánh dấu endpoint phải có tenant context hợp lệ trước khi xử lý.
    /// </summary>
    /// <param name="builder">Endpoint convention builder cần gắn metadata.</param>
    /// <returns>Chính builder đã truyền vào để tiếp tục chain cấu hình endpoint.</returns>
    public static TBuilder RequireTenantContext<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new TenantScopeMetadata(TenantEndpointScope.Tenant));

        return builder;
    }
}
