using ClinicSaaS.BuildingBlocks.OpenApi;
using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Observability.Correlation;
using WebsiteCmsService.Api.Endpoints;
using WebsiteCmsService.Api.Middleware;
using WebsiteCmsService.Application;
using WebsiteCmsService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebsiteCmsServiceApplication();
builder.Services.AddWebsiteCmsServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddClinicSaaSOpenApi("website-cms-service", "Website CMS Service");
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseMiddleware<WebsiteCmsService.Api.Middleware.TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health")
    .AllowPlatformScope()
    .WithTags("System");
app.UseClinicSaaSOpenApi("Website CMS Service");
app.MapSystemEndpoints("website-cms-service");
app.MapWebsiteCmsEndpoints();

app.Run();

public partial class Program;
