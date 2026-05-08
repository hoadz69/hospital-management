# Identity Service

Authentication, authorization, RBAC, and tenant claims placeholder.

## Local

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' run --project backend/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj
```

Placeholders:

- `/health`
- `/openapi/v1.json`
- `/swagger`
- `/api/_system/openapi-placeholder`
- `/api/_system/tenant-context` requires tenant context from `X-Tenant-Id` or JWT claim `tenant_id`
- `/api/_system/auth-rbac-placeholder` exposes metadata-only `RequireRole`, `RequirePermission`, and `UserContext` placeholders
- `/api/_system/postgres-placeholder`
