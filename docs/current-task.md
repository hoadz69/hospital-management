# Current Task - Shared Backend Primitives

Last updated: 2026-05-09

## Trang Thai

Owner da duyet Backend Phase 1 .NET Skeleton va yeu cau tao shared backend primitives trong `backend/shared`, ap dung toi thieu vao 3 service phase 1.

Da hoan thanh trong scope:

- `backend/shared/building-blocks`
- `backend/shared/contracts`
- `backend/shared/observability`
- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`

Khong sua frontend, khong implement business logic sau, khong connect PostgreSQL that, khong them secret, khong tao Figma file moi, khong commit.

## Runtime Note

- Giu target framework hien tai: `net9.0`.
- SDK build dung duoc nam o `C:\Program Files\dotnet\dotnet.exe`.
- Local x64 SDK hien co: .NET SDK `9.0.304`.

## Da Hoan Thanh

### Shared Projects

- Tao `backend/shared/building-blocks/ClinicSaaS.BuildingBlocks.csproj`.
- Tao `backend/shared/contracts/ClinicSaaS.Contracts.csproj`.
- Tao `backend/shared/observability/ClinicSaaS.Observability.csproj`.
- Them 3 shared projects vao `backend/ClinicSaaS.Backend.sln`.
- Them 3 shared projects vao service solutions:
  - `backend/services/api-gateway/ApiGateway.sln`
  - `backend/services/identity-service/IdentityService.sln`
  - `backend/services/tenant-service/TenantService.sln`

### BuildingBlocks Primitives

- `Tenancy/TenantContext`
- `Tenancy/ITenantContextAccessor`
- `Tenancy/TenantContextAccessor`
- `Tenancy/TenantResolutionResult`
- `Results/Error`
- `Results/Result`
- `Results/Result<T>`
- `Validation/Guard`
- `Options/OptionsSectionNames`
- `Options/PostgreSqlOptions`

### Contracts Primitives

- `Tenancy/TenantReference`
- `Security/UserContext`
- `Authorization/RoleNames`
- `Authorization/PermissionCodes`
- `Authorization/AuthRbacRequirement`
- `Events/IDomainEvent`
- `Events/DomainEventBase`
- `Events/TenantCreatedEvent`

### Observability Primitives

- `Logging/LoggingPropertyNames`
- `Correlation/CorrelationIdMiddleware`
- `Tracing/TraceContext`
- `Health/HealthCheckTags`

## Service References Applied

Ap dung cho 3 service phase 1:

- `api-gateway`
- `identity-service`
- `tenant-service`

Reference pattern:

- Api projects reference:
  - `ClinicSaaS.BuildingBlocks`
  - `ClinicSaaS.Contracts`
  - `ClinicSaaS.Observability`
- Application projects reference:
  - `ClinicSaaS.BuildingBlocks`
  - `ClinicSaaS.Contracts`
- Infrastructure projects reference:
  - `ClinicSaaS.BuildingBlocks`
- Tests projects reference:
  - `ClinicSaaS.BuildingBlocks`

Da thay local duplicate placeholders trong 3 services:

- tenant context/accessor local -> `ClinicSaaS.BuildingBlocks.Tenancy`
- RBAC constants local -> `ClinicSaaS.Contracts.Authorization`
- PostgreSQL options local -> `ClinicSaaS.BuildingBlocks.Options`
- them `CorrelationIdMiddleware` tu `ClinicSaaS.Observability.Correlation`

## Verify Da Chay

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Ket qua:

- Restore pass cho `backend/ClinicSaaS.Backend.sln`.
- Build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build root solution: 0 warnings, 0 errors.

## Con Chua Lam / Ngoai Scope

- Chua implement auth provider/JWT validation thật.
- Chua implement RBAC enforcement thật.
- Chua connect PostgreSQL/Npgsql/EF Core/Dapper.
- Chua implement tenant CRUD hoặc business use cases.
- Chua tao migrations.
- Chua sua frontend routing/layout.

## Buoc Tiep Theo De Xuat

1. Consolidate tenant middleware vao shared package neu muon dung mot middleware chung cho tat ca service.
2. Them OpenAPI package/cau hinh that cho 3 Api projects.
3. Phase tiep theo: implement auth/RBAC contract enforcement placeholder ro hon o API boundary, van chua can connect DB.
