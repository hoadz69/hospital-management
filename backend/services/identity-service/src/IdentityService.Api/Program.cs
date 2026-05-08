using IdentityService.Api.Endpoints;
using IdentityService.Api.Middleware;
using IdentityService.Application;
using IdentityService.Infrastructure;
using ClinicSaaS.BuildingBlocks.OpenApi;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Observability.Correlation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServiceApplication();
builder.Services.AddIdentityServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddClinicSaaSOpenApi("identity-service", "Identity Service");
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseMiddleware<IdentityService.Api.Middleware.TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health")
    .AllowPlatformScope()
    .WithTags("System");
app.UseClinicSaaSOpenApi("Identity Service");
app.MapSystemEndpoints("identity-service");

app.Run();

public partial class Program;
