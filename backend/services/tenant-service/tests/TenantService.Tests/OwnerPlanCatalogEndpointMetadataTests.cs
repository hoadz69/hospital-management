using ClinicSaaS.BuildingBlocks.Authorization;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TenantService.Api.Endpoints;
using TenantService.Application.Plans;
using Xunit;

namespace TenantService.Tests;

/// <summary>
/// Contract test cho metadata route `/api/owner/*` trong Tenant Service.
/// </summary>
public sealed class OwnerPlanCatalogEndpointMetadataTests
{
    /// <summary>
    /// Xác nhận bốn route Owner Plan được giữ đúng path, platform scope, role và permission cho FE A9.
    /// </summary>
    [Fact]
    public void MapOwnerPlanCatalogEndpoints_RegistersOwnerRoutesWithExpectedSecurityMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<OwnerPlanCatalogStubHandler>();
        var app = builder.Build();

        app.MapOwnerPlanCatalogEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
        var expectedRoutes = new Dictionary<string, string>
        {
            ["/api/owner/plans"] = PermissionCodes.PlansRead,
            ["/api/owner/modules"] = PermissionCodes.PlansRead,
            ["/api/owner/tenant-plan-assignments"] = PermissionCodes.PlansRead,
            ["/api/owner/tenant-plan-assignments/bulk-change"] = PermissionCodes.PlansWrite
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
            Assert.Equal(TenantEndpointScope.Platform, scopeMetadata.Scope);
        }
    }
}
