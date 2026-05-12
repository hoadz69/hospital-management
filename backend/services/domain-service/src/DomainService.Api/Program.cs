using ClinicSaaS.BuildingBlocks.OpenApi;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Observability.Correlation;
using DomainService.Api.Endpoints;
using DomainService.Api.Middleware;
using DomainService.Application;
using DomainService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDomainServiceApplication();
builder.Services.AddDomainServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddClinicSaaSOpenApi("domain-service", "Domain Service");
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseMiddleware<DomainService.Api.Middleware.TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health")
    .AllowPlatformScope()
    .WithTags("System");
app.UseClinicSaaSOpenApi("Domain Service");
app.MapSystemEndpoints("domain-service");
app.MapDomainEndpoints();

app.Run();

public partial class Program;
