using ClinicSaaS.Observability.Correlation;
using ClinicSaaS.BuildingBlocks.OpenApi;
using ClinicSaaS.BuildingBlocks.Tenancy;
using TenantService.Api.Endpoints;
using TenantService.Api.Middleware;
using TenantService.Application;
using TenantService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTenantServiceApplication();
builder.Services.AddTenantServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddClinicSaaSOpenApi("tenant-service", "Tenant Service");
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseMiddleware<TenantService.Api.Middleware.TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health")
    .AllowPlatformScope()
    .WithTags("System");
app.UseClinicSaaSOpenApi("Tenant Service");
app.MapSystemEndpoints("tenant-service");
app.MapTenantEndpoints();

app.Run();

public partial class Program;
