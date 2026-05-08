using ApiGateway.Api.Endpoints;
using ApiGateway.Api.Middleware;
using ApiGateway.Application;
using ApiGateway.Infrastructure;
using ClinicSaaS.Observability.Correlation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiGatewayApplication();
builder.Services.AddApiGatewayInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();
app.UseMiddleware<AuthRbacPlaceholderMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapSystemEndpoints("api-gateway");

app.Run();

public partial class Program;
