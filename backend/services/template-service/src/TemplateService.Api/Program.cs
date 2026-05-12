using ClinicSaaS.BuildingBlocks.OpenApi;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Observability.Correlation;
using TemplateService.Api.Endpoints;
using TemplateService.Api.Middleware;
using TemplateService.Application;
using TemplateService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTemplateServiceApplication();
builder.Services.AddTemplateServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddClinicSaaSOpenApi("template-service", "Template Service");
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseMiddleware<TemplateService.Api.Middleware.TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health")
    .AllowPlatformScope()
    .WithTags("System");
app.UseClinicSaaSOpenApi("Template Service");
app.MapSystemEndpoints("template-service");
app.MapTemplateEndpoints();

app.Run();

public partial class Program;
