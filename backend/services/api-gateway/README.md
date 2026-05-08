# API Gateway

Gateway/BFF placeholder for routing frontend traffic to backend services.

Responsibilities:

- Tenant context propagation.
- Auth/RBAC integration.
- Service routing.
- OpenAPI aggregation later.

## Local

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' run --project backend/services/api-gateway/src/ApiGateway.Api/ApiGateway.Api.csproj
```

Placeholders:

- `/health`
- `/api/_system/openapi-placeholder`
- `/api/_system/tenant-context`
- `/api/_system/auth-rbac-placeholder`
- `/api/_system/postgres-placeholder`
