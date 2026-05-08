using IdentityService.Api.Endpoints;
using IdentityService.Api.Middleware;
using IdentityService.Application;
using IdentityService.Infrastructure;
using ClinicSaaS.Observability.Correlation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServiceApplication();
builder.Services.AddIdentityServiceInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapSystemEndpoints("identity-service");

app.Run();

public partial class Program;
