using ClinicSaaS.Observability.Correlation;
using TenantService.Api.Endpoints;
using TenantService.Api.Middleware;
using TenantService.Application;
using TenantService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTenantServiceApplication();
builder.Services.AddTenantServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapSystemEndpoints("tenant-service");

app.Run();

public partial class Program;
