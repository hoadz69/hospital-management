# Clinic SaaS Backend

.NET microservice workspace for Clinic SaaS.

Current local skeleton target: `net9.0`, because the available x64 SDK on this machine is .NET SDK `9.0.304`.

Each service follows Clean Architecture:

- `src/<Service>.Api`
- `src/<Service>.Application`
- `src/<Service>.Domain`
- `src/<Service>.Infrastructure`
- `tests`

Business logic is intentionally not implemented in this scaffold pass.

## Phase 1 Services

- `services/api-gateway`
- `services/identity-service`
- `services/tenant-service`

Each phase 1 service includes placeholders for tenant context middleware, auth/RBAC, health checks, OpenAPI contract wiring, and PostgreSQL configuration. PostgreSQL is not connected yet.

## Shared Projects

- `shared/building-blocks`: `ClinicSaaS.BuildingBlocks`, with tenant context, result/error, guard, and options placeholders.
- `shared/contracts`: `ClinicSaaS.Contracts`, with tenant/user DTOs, role/permission constants, domain event placeholders, and auth/RBAC contracts.
- `shared/observability`: `ClinicSaaS.Observability`, with correlation id middleware, logging constants, trace context, and health check tags.

## Verify

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

## Run Local

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' run --project backend/services/api-gateway/src/ApiGateway.Api/ApiGateway.Api.csproj
& 'C:\Program Files\dotnet\dotnet.exe' run --project backend/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj
& 'C:\Program Files\dotnet\dotnet.exe' run --project backend/services/tenant-service/src/TenantService.Api/TenantService.Api.csproj
```

Useful placeholder endpoints:

- `/health`
- `/api/_system/openapi-placeholder`
- `/api/_system/tenant-context`
- `/api/_system/auth-rbac-placeholder`
- `/api/_system/postgres-placeholder`
