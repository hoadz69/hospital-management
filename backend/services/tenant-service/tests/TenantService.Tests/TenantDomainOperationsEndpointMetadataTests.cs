using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Security;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using ClinicSaaS.Contracts.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TenantService.Api.Endpoints;
using TenantService.Application.Domains;
using Xunit;

namespace TenantService.Tests;

/// <summary>
/// Contract test cho metadata route Domain DNS/SSL trong Tenant Service.
/// </summary>
public sealed class TenantDomainOperationsEndpointMetadataTests
{
    /// <summary>
    /// Xac nhan ba route FE can co tenant scope, OwnerSuperAdmin role va permission dung.
    /// </summary>
    [Fact]
    public void MapTenantDomainOperationsEndpoints_RegistersExpectedSecurityMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();
        builder.Services.AddScoped<ITenantDomainOperationsRepository, ThrowingTenantDomainOperationsRepository>();
        builder.Services.AddScoped<TenantDomainOperationsHandler>();
        var app = builder.Build();

        app.MapTenantDomainOperationsEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
        var expectedRoutes = new Dictionary<string, string>
        {
            ["/api/tenants/{tenantId:guid}/domains"] = PermissionCodes.DomainsRead,
            ["/api/tenants/{tenantId:guid}/domains/{domainId:guid}/dns-retry"] = PermissionCodes.DomainsWrite,
            ["/api/tenants/{tenantId:guid}/domains/{domainId:guid}/ssl-status"] = PermissionCodes.DomainsRead
        };

        foreach (var (route, permission) in expectedRoutes)
        {
            var endpoint = Assert.Single(endpoints, candidate => candidate.RoutePattern.RawText == route);
            var roleMetadata = endpoint.Metadata.GetMetadata<RequiredRoleMetadata>();
            var permissionMetadata = endpoint.Metadata.GetMetadata<RequiredPermissionMetadata>();
            var scopeMetadata = endpoint.Metadata.GetMetadata<TenantScopeMetadata>();

            Assert.NotNull(roleMetadata);
            Assert.Contains(RoleNames.OwnerSuperAdmin, roleMetadata.Roles);
            Assert.NotNull(permissionMetadata);
            Assert.Contains(permission, permissionMetadata.Permissions);
            Assert.NotNull(scopeMetadata);
            Assert.Equal(TenantEndpointScope.Tenant, scopeMetadata.Scope);
        }
    }

    private sealed class ThrowingTenantDomainOperationsRepository : ITenantDomainOperationsRepository
    {
        public Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken)
            => throw new NotSupportedException("Metadata test does not invoke repository.");

        public Task<IReadOnlyList<DomainDnsSslStateResponse>> ListDomainsAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException("Metadata test does not invoke repository.");

        public Task<DomainDnsSslStateResponse?> GetDomainAsync(
            Guid tenantId,
            Guid domainId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException("Metadata test does not invoke repository.");

        public Task<DomainDnsSslStateResponse?> RetryDnsAsync(
            Guid tenantId,
            Guid domainId,
            DateTimeOffset now,
            DateTimeOffset nextRetryAt,
            CancellationToken cancellationToken)
            => throw new NotSupportedException("Metadata test does not invoke repository.");
    }
}
