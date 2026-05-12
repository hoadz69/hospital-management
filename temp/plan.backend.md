# Kế Hoạch Backend/DevOps - Phase 2 API Runtime Smoke Gate

Ngày cập nhật: 2026-05-10

Trạng thái: ✅ Phase 2 API Runtime Smoke Gate PASS đủ 5 case trên server `116.118.47.78` sau hai vòng fix (Dapper type handler + reorder positional record `TenantListRow`). Lane sẵn sàng cho Lead Agent đồng bộ dashboard + roadmap để chuyển Phase 2 sang Done.

Chế độ thực hiện: Backend/DevOps lane riêng. Không sửa frontend, không sửa Figma, không commit, không push, không ghi secret/IP/private key vào repo.

## 1. Tóm Tắt Task

Phase 2 Tenant MVP Backend đã có local commit:

```txt
71923f8 feat: implement tenant mvp backend slice
```

Static/build/config đã pass. DB runtime smoke local từng bị blocked vì Docker daemon local chưa chạy. Owner đã cho dùng server riêng để bootstrap Docker/PostgreSQL.

Bootstrap server đã hoàn tất:

```txt
SSH: pass
OS: Ubuntu 22.04.5 LTS
CPU/RAM/disk: 4 cores, 5.8 GiB RAM, root disk 64G
Docker Engine: 29.4.3
Docker Compose plugin: v5.1.3
Docker info: pass
Deploy directory: /opt/clinic-saas
Docker network: clinic-saas-network
PostgreSQL container: clinic-saas-postgres, postgres:16-alpine, Up
PostgreSQL publish port: none
Schema Phase 2: apply pass
Tables: platform.tenants, platform.tenant_profiles, platform.tenant_domains, platform.tenant_modules
```

Phase 2 vẫn chưa Done vì chưa chạy API smoke qua `tenant-service`/`api-gateway`.

## 2. Mục Tiêu Hiện Tại

Chạy API runtime smoke cho Phase 2 bằng PostgreSQL server đã bootstrap:

```txt
tenant-service -> PostgreSQL container clinic-saas-postgres
api-gateway -> tenant-service
smoke endpoints -> pass
```

## 3. Scope Được Làm

Backend/DevOps có thể làm trong phạm vi:

```txt
backend/services/tenant-service
backend/services/api-gateway
backend/shared nếu cần cho runtime config/smoke
infrastructure/docker nếu cần chạy services đúng network
docs/current-task.backend.md
temp/plan.backend.md
docs/deployment/server-bootstrap.md nếu kết quả deploy/smoke thay đổi
docs/roadmap/clinic-saas-roadmap.md nếu status phase thay đổi
```

Không sửa:

```txt
frontend/
Figma
Clinic Admin/Public Website UI
feature ngoài Tenant MVP API smoke
```

## 4. Out Of Scope

- Không commit nếu owner chưa yêu cầu.
- Không push.
- Không ghi private key vào repo.
- Không ghi secret thật vào file tracked.
- Không ghi IP server thật vào repo.
- Không mở PostgreSQL public ra internet.
- Không expose port `5432` ra `0.0.0.0`.
- Không dùng MySQL.
- Không cài package ngoài scope runtime smoke.
- Không sửa frontend.
- Không tạo Figma file.
- Không chuyển Phase 2 sang Done nếu API smoke chưa pass thật.

## 5. Plan Chạy Smoke

### 5.1 Chuẩn Bị Runtime

1. Xác nhận PostgreSQL container còn chạy:

```bash
docker ps --filter name=clinic-saas-postgres
docker exec clinic-saas-postgres pg_isready -U clinic_dev -d clinic_saas_dev
```

2. Xác nhận schema còn đủ:

```bash
docker exec clinic-saas-postgres psql -U clinic_dev -d clinic_saas_dev -Atc "select schemaname || '.' || tablename from pg_tables where schemaname='platform' order by tablename;"
```

3. Chọn cách chạy services:

```txt
Ưu tiên A: chạy tenant-service và api-gateway trong cùng Docker network clinic-saas-network.
Dự phòng B: chạy service từ máy dev qua SSH tunnel nếu runtime container hóa chưa sẵn.
```

### 5.2 Chạy `tenant-service`

- Trỏ connection string tới PostgreSQL host `clinic-saas-postgres` nếu chạy cùng Docker network.
- Không ghi connection string thật vào file tracked.
- Chỉ dùng env var hoặc secret tạm trong shell/runtime.
- Verify:

```txt
GET /health
GET /openapi/v1.json
```

### 5.3 Chạy `api-gateway`

- Trỏ TenantService base URL tới `tenant-service`.
- Verify:

```txt
GET /health
GET /openapi/v1.json
```

### 5.4 API Smoke Bắt Buộc

Chạy qua gateway nếu khả thi, đồng thời có thể kiểm tra trực tiếp tenant-service khi cần debug:

```txt
POST /api/tenants
GET /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
POST duplicate slug/domain => 409
```

## 6. Success Criteria

Phase 2 chỉ được chuyển Done khi:

- `tenant-service` chạy được với PostgreSQL server đã bootstrap.
- `api-gateway` gọi được `tenant-service`.
- `POST /api/tenants` tạo tenant và ghi DB thành công.
- `GET /api/tenants` trả danh sách có tenant vừa tạo.
- `GET /api/tenants/{id}` trả đúng tenant.
- `PATCH /api/tenants/{id}/status` cập nhật status đúng.
- Duplicate slug/domain trả `409`.
- Không expose PostgreSQL public.
- Docs lane backend và roadmap được cập nhật đúng kết quả thật.

## 7. Verify Plan

Local hoặc remote tùy cách chạy:

```powershell
git status --branch --short
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build
docker compose -f infrastructure/docker/docker-compose.dev.yml config
```

Runtime smoke:

```bash
curl -i <gateway-or-tenant-service>/health
curl -i <gateway-or-tenant-service>/openapi/v1.json
curl -i -X POST <gateway>/api/tenants ...
curl -i <gateway>/api/tenants
curl -i <gateway>/api/tenants/{id}
curl -i -X PATCH <gateway>/api/tenants/{id}/status ...
curl -i -X POST <gateway>/api/tenants ...duplicate...
```

Port check:

```bash
docker port clinic-saas-postgres
ss -tulpen
ufw status verbose
```

Kỳ vọng:

```txt
Không có PostgreSQL listen public 0.0.0.0:5432.
```

## 8. Rủi Ro Và Cách Xử Lý

- Nếu SSH fail: dừng và report lỗi xác thực/kết nối.
- Nếu container PostgreSQL không còn chạy: inspect trạng thái, không drop volume ngoài scope.
- Nếu migration đã apply: verify tables, không drop schema.
- Nếu service không đọc được env: sửa config runtime tối thiểu trong backend/devops scope, không ghi secret.
- Nếu gateway forward lỗi: kiểm tra TenantService base URL và network DNS.
- Nếu API trả 500: kiểm tra log, map lỗi rõ, không expose stack trace ngoài Development.
- Nếu duplicate không trả 409: giữ Phase 2 In Progress và ghi blocker.

## 9. Điểm Dừng

Dừng sau khi API smoke pass hoặc khi có blocker rõ. Cập nhật:

```txt
docs/current-task.backend.md
temp/plan.backend.md
docs/roadmap/clinic-saas-roadmap.md
```

Nếu API smoke pass, Lead Agent mới được cập nhật dashboard và roadmap để chuyển Phase 2 Done.

## 10. Cập Nhật 2026-05-10 - Root Cause GET/PATCH 500

### 10.1 Triệu chứng smoke

```txt
POST /api/tenants -> 201 (pass)
duplicate POST /api/tenants -> 409 (pass)
GET /api/tenants -> 500 (fail)
GET /api/tenants/{id} -> 500 (fail)
PATCH /api/tenants/{id}/status -> 500 (fail)
```

### 10.2 Root cause

Schema PostgreSQL Phase 2 dùng `timestamptz` cho mọi cột `*_at_utc`. Từ Npgsql 6.0 trở lên, provider mặc định trả `timestamptz` về `System.DateTime` với `Kind=Utc`, không còn về `System.DateTimeOffset` như trước. Tất cả row record trong `DapperTenantRepository` (`TenantRootRow`, `TenantListRow`, `TenantDomainRow`, `TenantModuleRow`) đang khai báo property dạng `DateTimeOffset` / `DateTimeOffset?`. Khi Dapper map kết quả SELECT, `InvalidCastException` ném ra → ASP.NET Core trả 500.

POST tạo tenant pass vì `CreateTenantHandler` map response từ aggregate in-memory ngay sau insert, không re-read DB. Đường read/update mới chạm code path lỗi:

```txt
GET list -> DapperTenantRepository.ListAsync -> TenantListRow
GET id -> DapperTenantRepository.GetByIdAsync -> LoadTenantAsync -> TenantRootRow + TenantDomainRow + TenantModuleRow
PATCH status -> DapperTenantRepository.UpdateStatusAsync -> LoadTenantAsync -> cùng các row record trên
```

Tài liệu tham chiếu:

```txt
https://www.npgsql.org/doc/types/datetime.html
https://www.npgsql.org/doc/release-notes/6.0.html
```

### 10.3 Minimal fix đã apply

Đăng ký Dapper `SqlMapper.TypeHandler<DateTimeOffset>` để bridge `DateTime` UTC ↔ `DateTimeOffset` UTC. Không thay schema, không sửa domain entity, không sửa public contracts, không thêm dependency.

File mới:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DateTimeOffsetTypeHandler.cs
```

File modified:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs
```

`DependencyInjection.AddTenantServiceInfrastructure(...)` gọi `RegisterDapperTypeHandlersOnce()` đúng một lần per process bằng `Interlocked.Exchange` guard. Comment/XML doc tiếng Việt theo `rules/backend-coding-rules.md`.

### 10.4 Verify local

```txt
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln: pass
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore: pass, 0 warnings, 0 errors
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build: exit code 0
```

Test runner không pick up test case nào vì project `tests/TenantService.Tests` hiện là placeholder không reference xUnit/NUnit/MSTest. Đây là gap có sẵn trước Phase 2, không thuộc scope smoke gate.

### 10.5 DevOps action cần làm tiếp

```txt
1. Trên server, rebuild Docker image cho tenant-service từ source mới nhất (chứa DateTimeOffsetTypeHandler).
2. Restart container clinic-saas-tenant-service trong Docker network clinic-saas-network.
3. api-gateway không cần đổi (pure pass-through, không sửa code).
4. Smoke lại 5 case Phase 2 qua api-gateway:
   - POST /api/tenants -> 201
   - duplicate slug/domain -> 409
   - GET /api/tenants -> 200 + JSON list
   - GET /api/tenants/{id} -> 200 + JSON detail
   - PATCH /api/tenants/{id}/status body {"status":"Active"} -> 200 + JSON detail
5. Nếu có endpoint vẫn 500, lấy log:
   docker logs clinic-saas-tenant-service --tail=200
   Tìm chuỗi "Unable to cast" hoặc stack trace InvalidCastException.
6. Không expose 5432 ra 0.0.0.0. Không ghi connection string thật vào file tracked.
```

DevOps lane chỉ có thể chạy khi owner cung cấp lại biến shell `DEPLOY_HOST`/`DEPLOY_USER`/`SSH_KEY_PATH` trong phiên hiện tại. Lead Agent không tự dùng SSH/server thật khi owner chưa cung cấp ở phiên này.

### 10.6 Done condition Phase 2

Phase 2 chỉ chuyển Done sau khi cả 5 smoke trên pass thật trên server đã rebuild. Trước khi pass đủ, dashboard và roadmap phải giữ Phase 2 ở trạng thái `🟡 In Progress`.

## 11. Cập Nhật 2026-05-10 - Smoke Pass Đủ

### 11.1 Kết quả vòng 1 deploy

Sau khi deploy fix vòng 1 (Dapper type handler) lên server bằng pattern publish + scp + swap dir + restart:

```txt
POST /api/tenants -> 201 (pass)
duplicate -> 409 (pass)
GET /api/tenants/{id} -> 200 (pass)
PATCH /api/tenants/{id}/status -> 200 (pass)
GET /api/tenants -> 500 (fail tiếp, lý do khác)
```

Log container chỉ còn `InvalidOperationException` cho `TenantListRow` materialization, không còn `InvalidCastException` trên các path khác.

### 11.2 Root cause vòng 2

Dapper 2.1 với positional record materialize theo position type, không lookup type handler khi resolve constructor. `TenantListRow` có `ClinicName` ở position 6 trong ctor nhưng SQL SELECT trong `ListAsync` đặt `clinic_name` cuối cùng (position 11) vì JOIN `tenant_profiles`. Type lệch tại position 6 và 11.

`TenantRootRow`, `ClinicProfileRow`, `TenantDomainRow`, `TenantModuleRow` không gặp vì ctor order đã khớp SELECT.

### 11.3 Fix vòng 2

Reorder ctor `TenantListRow` đúng thứ tự cột SELECT (`ClinicName` cuối). Mapping `ToTenantSummary(TenantListRow row)` truy cập field theo NAME nên reorder không phá logic.

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantRepository.cs (MODIFIED)
```

Thêm comment giải thích invariant order.

### 11.4 Verify Local v2

```txt
dotnet build backend/ClinicSaaS.Backend.sln --nologo: pass, 0 warnings, 0 errors
dotnet test backend/ClinicSaaS.Backend.sln --no-build --nologo: không pickup test case (placeholder)
```

### 11.5 Deploy v2 và Smoke Final

```txt
dotnet publish ... -o temp/publish/tenant-service: pass
scp -r ... root@116.118.47.78:/opt/clinic-saas/runtime-smoke/tenant-service-new/: pass
docker stop clinic-saas-tenant-service-smoke: pass
mv tenant-service tenant-service.bak.20260509T191905Z: pass
mv tenant-service-new tenant-service: pass
docker start clinic-saas-tenant-service-smoke: pass
curl http://127.0.0.1:5006/health: 200 Healthy
curl http://127.0.0.1:5005/health: 200 Healthy
docker logs --since=2m: chỉ còn warning libgssapi_krb5.so.2 cannot open (harmless GSSAPI)
```

Smoke 5 case qua `api-gateway` (`127.0.0.1:5005`) với payload `demo-clinic-smoke-003`:

```txt
POST /api/tenants -> 201
duplicate POST -> 409
GET /api/tenants -> 200 (response items array trả tenant list với clinicName, createdAtUtc, updatedAtUtc đúng)
GET /api/tenants/{id} -> 200 (response detail với profile, domains, modules)
PATCH /api/tenants/{id}/status body {"status":"Active"} -> 200 (status chuyển từ Draft sang Active)
```

Kết luận: Phase 2 đủ điều kiện chuyển Done.

### 11.6 File backend đã sửa trong session này

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DateTimeOffsetTypeHandler.cs (NEW, vòng 1)
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs (MODIFIED, vòng 1)
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantRepository.cs (MODIFIED, vòng 2 - reorder TenantListRow)
```

File helper local (không bắt buộc commit):

```txt
temp/smoke.sh (script smoke 5 case dùng curl + python3 trên server)
temp/publish/tenant-service/ (publish output, ngoài git)
```

Owner chưa yêu cầu commit/push.

## 12. Pre-Phase 4 Hardening - Backend Issues (2026-05-10)

Trạng thái: 🟡 Plan ready, chờ owner duyệt implement.

Phần này thuộc feature team Pre-Phase 4 Hardening do Lead Agent điều phối theo "Feature Team Execution Workflow". Backend lane phụ trách 2/5 issue: P1.1 test infra rỗng, P1.3 Swagger gating Development. Các issue còn lại do DevOps lane (`temp/plan.devops.md`) và Frontend lane (`temp/plan.frontend.md`) phụ trách.

### 12.1 Agents Tham Gia (Lane Backend)

- Lead Agent: điều phối, gom report, không tự code.
- Architect Agent: review boundary/risk trước khi Backend Agent đụng OpenAPI extension và test project.
- Backend Agent: thực hiện thay đổi `ClinicSaaSOpenApiExtensions`, 3 `*.Tests.csproj` và 3 `Program.cs`.
- QA Agent: tạo verify checklist, chạy `dotnet restore/build/test`, kiểm tra Swagger endpoint trong Development vs non-Development.
- Documentation Agent: cập nhật dashboard, lane plan này, roadmap.

### 12.2 Issues Trong Lane Này

#### P1.1 test infra rỗng

File hiện tại: 3 test project chỉ có `ProjectReference`, không có test framework:

```txt
backend/services/api-gateway/tests/ApiGateway.Tests/ApiGateway.Tests.csproj
backend/services/identity-service/tests/IdentityService.Tests/IdentityService.Tests.csproj
backend/services/tenant-service/tests/TenantService.Tests/TenantService.Tests.csproj
```

Mỗi csproj đang thiếu: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio` (hoặc tương đương). Vì vậy `dotnet test backend/ClinicSaaS.Backend.sln --no-build` exit 0 nhưng không pickup test case nào (đã ghi nhận trong section 10.4 và 11.4 của lane này).

Mục tiêu:

- Thêm 3 PackageReference (Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio) vào 3 csproj.
- Thêm `IsPackable=false` cho project test theo convention.
- Tạo 1 sample test minh họa cho mỗi service (ví dụ `HealthEndpointTests` hoặc `SmokeTests`) để verify test runner pickup. Sample test rất nhỏ, chỉ dạng `Assert.True(true)` hoặc 1 unit test thật trên Application/Domain code đã có.
- Tránh kéo dependency lớn (ASP.NET TestHost, WebApplicationFactory) trong scope hardening; nếu cần integration test thật, tách Phase 4.

Quyết định version package: chọn xUnit 2.9.x + Microsoft.NET.Test.Sdk 17.11.x (LTS hiện tại cho .NET 9). Nếu owner có policy package version riêng, để dành thêm slot cho confirm; default xài LTS mới nhất tại 2026-05-10.

#### P1.3 Swagger/OpenAPI chỉ bật ở Development

File hiện tại: `backend/shared/building-blocks/OpenApi/ClinicSaaSOpenApiExtensions.cs` exposes Swagger UI (`/swagger`) và OpenAPI JSON (`/openapi/v1.json`) cho mọi environment. 3 `Program.cs` đều gọi `app.UseClinicSaaSOpenApi("...")` không gating.

Mục tiêu:

- Sửa `UseClinicSaaSOpenApi(this WebApplication app, string serviceDisplayName)` thành extension nhận `IWebHostEnvironment` và chỉ map Swagger UI trong Development. OpenAPI JSON cũng chỉ expose Development để tránh phát tán contract endpoint trên prod.
- Hoặc thay vì sửa signature, bọc trong `if (app.Environment.IsDevelopment()) { ... }` ngay trong extension. Cách thứ 2 ít invasive hơn, không phá callsite hiện tại.
- Verify Swagger vẫn hoạt động trên server smoke nếu env là Development. Tài liệu lại trong `docs/deployment/server-bootstrap.md` (nếu cần) rằng để mở Swagger trên server, phải set `ASPNETCORE_ENVIRONMENT=Development`.
- Health endpoint `/health` và `/openapi/v1.json` phía backend smoke đã PASS Phase 2 trên server với env Development; không break smoke nếu giữ env này. Nếu server đang chạy env khác, chấp nhận `/swagger` 404 trên prod là đúng yêu cầu.

### 12.3 File Dự Kiến Sửa

```txt
backend/shared/building-blocks/OpenApi/ClinicSaaSOpenApiExtensions.cs
backend/services/api-gateway/tests/ApiGateway.Tests/ApiGateway.Tests.csproj
backend/services/identity-service/tests/IdentityService.Tests/IdentityService.Tests.csproj
backend/services/tenant-service/tests/TenantService.Tests/TenantService.Tests.csproj
backend/services/api-gateway/tests/ApiGateway.Tests/SmokeTests.cs (NEW, sample)
backend/services/identity-service/tests/IdentityService.Tests/SmokeTests.cs (NEW, sample)
backend/services/tenant-service/tests/TenantService.Tests/SmokeTests.cs (NEW, sample)
docs/current-task.backend.md (lane status)
temp/plan.backend.md (file này, ghi nhận trạng thái sau implement)
```

Không sửa:

```txt
3 Program.cs (giữ nguyên signature cũ, gating bên trong extension)
backend/services/*/src/* (chỉ Application/Domain/Infrastructure được test, không sửa)
frontend/**
infrastructure/**
.env*
```

### 12.4 Risk

- P1.1: thêm package cần NuGet feed. Nếu môi trường offline, restore fail. Owner phải confirm có internet/NuGet access trong session implement.
- P1.1: nếu sample test reference Domain với constructor có dependency, có thể fail compile. Lean về `Assert.True(1+1==2)` cho 3 service đầu tiên, để khẳng định runner pickup, rồi mới mở rộng test thật.
- P1.3: nếu server smoke không set `ASPNETCORE_ENVIRONMENT=Development`, Swagger UI sẽ trả 404 sau hardening. Document rõ trước. Phase 2 smoke 5 case không phụ thuộc Swagger UI nên không break.
- Backward compat: callsite `app.UseClinicSaaSOpenApi(...)` không đổi signature ⇒ không phải sửa 3 Program.cs.

### 12.5 Verify Command

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build
```

Kỳ vọng:

- restore/build PASS, 0 error.
- test runner pickup ≥ 3 test case (1 mỗi service) và PASS.
- Output có dòng `Passed!  - Failed: 0, Passed: 3+`.

Smoke Swagger gating local (nếu Backend Agent có dotnet runtime):

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project backend/services/tenant-service/src/TenantService.Api
# Trong session khác:
curl -i http://localhost:<port>/swagger
curl -i http://localhost:<port>/openapi/v1.json
# Đổi env:
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run --project backend/services/tenant-service/src/TenantService.Api
curl -i http://localhost:<port>/swagger    # expect 404
curl -i http://localhost:<port>/openapi/v1.json # expect 404
```

Nếu môi trường không có dotnet runtime cho Backend Agent, báo blocker và để QA Agent verify khi có server.

### 12.6 Commit Split Đề Xuất

```txt
chore(backend): wire xunit + sample tests for 3 services (P1.1)
chore(backend): gate Swagger/OpenAPI to Development (P1.3)
```

Không gom với commit DevOps/Frontend của Pre-Phase 4 Hardening.

### 12.7 Out Of Scope

- Không refactor service code ngoài extension OpenAPI.
- Không thêm integration test (WebApplicationFactory, TestServer) ở scope hardening.
- Không thêm test cho domain logic sâu; chỉ smoke confirm runner pickup.
- Không đụng frontend/devops/infrastructure files.
- Không tạo Figma file.
- Không commit/push.
- Không đổi Phase 2 status đã Done; chỉ thêm dòng Pre-Phase 4 Hardening vào roadmap khi feature team finish.

### 12.8 Điểm Dừng

Plan ready. Backend Agent chỉ implement khi owner đã duyệt rõ. Sau implement, QA Agent verify, Documentation Agent cập nhật dashboard/lane/roadmap. Lead Agent gom report.

### 12.9 Implementation Result (2026-05-10)

Trạng thái: ✅ **IMPLEMENTATION DONE** — P1.1 + P1.3 đã sửa, `dotnet restore/build/test` PASS đầy đủ.

File đã sửa:

```txt
backend/shared/building-blocks/OpenApi/ClinicSaaSOpenApiExtensions.cs (P1.3: thêm using Microsoft.Extensions.Hosting + bọc UseSwagger/UseSwaggerUI trong if (!app.Environment.IsDevelopment()) return app)
backend/services/api-gateway/tests/ApiGateway.Tests/ApiGateway.Tests.csproj (P1.1: PropertyGroup IsPackable=false + IsTestProject=true; thêm 3 PackageReference Microsoft.NET.Test.Sdk 17.11.1, xunit 2.9.2, xunit.runner.visualstudio 2.8.2)
backend/services/identity-service/tests/IdentityService.Tests/IdentityService.Tests.csproj (P1.1: cùng pattern như ApiGateway.Tests)
backend/services/tenant-service/tests/TenantService.Tests/TenantService.Tests.csproj (P1.1: cùng pattern như ApiGateway.Tests)
backend/services/api-gateway/tests/ApiGateway.Tests/SmokeTests.cs (NEW, P1.1: 1 [Fact] TestRunner_PicksUp_ApiGatewayTests)
backend/services/identity-service/tests/IdentityService.Tests/SmokeTests.cs (NEW, P1.1: 1 [Fact] TestRunner_PicksUp_IdentityServiceTests)
backend/services/tenant-service/tests/TenantService.Tests/SmokeTests.cs (NEW, P1.1: 1 [Fact] TestRunner_PicksUp_TenantServiceTests)
```

Verify đã chạy:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore --nologo
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build --nologo
```

Kết quả:

```txt
restore: PASS, 3 test project đã restore xunit + Microsoft.NET.Test.Sdk + xunit.runner.visualstudio
build: PASS, 0 Warning(s), 0 Error(s), 9.03s
test: PASS đủ 3/3 test case (1 mỗi service):
  - IdentityService.Tests.dll: Passed: 1, Failed: 0, Total: 1, 157 ms
  - ApiGateway.Tests.dll: Passed: 1, Failed: 0, Total: 1, 180 ms
  - TenantService.Tests.dll: Passed: 1, Failed: 0, Total: 1, 189 ms
```

P1.3 Swagger gating: callsite trong 3 Program.cs không đổi (`app.UseClinicSaaSOpenApi("...")` vẫn signature cũ); gating xảy ra bên trong extension. Khi `ASPNETCORE_ENVIRONMENT != Development`, extension return ngay sau khi check, không map `/swagger` và `/openapi/v1.json`.

Smoke real Swagger gating (chưa chạy do session không có dotnet runtime đang mở; đề xuất QA chạy 2 vòng env):

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project backend/services/tenant-service/src/TenantService.Api
# expect /swagger 200, /openapi/v1.json 200
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run --project backend/services/tenant-service/src/TenantService.Api
# expect /swagger 404, /openapi/v1.json 404
```

Smoke server `116.118.47.78`: container hiện đang chạy ASPNETCORE_ENVIRONMENT theo cấu hình deploy cũ. Nếu env không phải Development, sau khi rebuild image, `/swagger` sẽ trả 404 trên server. Phase 2 smoke 5 case không phụ thuộc Swagger UI nên không break. Documentation Agent cần ghi note này trong `docs/deployment/server-bootstrap.md` khi cập nhật.

Commit Split (Step 9): cách A (2 commit nhỏ) hoặc cách B (1 commit lane backend) — owner chọn:

```txt
# Cách A
chore(backend): wire xunit + sample tests for 3 services (P1.1)
chore(backend): gate Swagger/OpenAPI to Development (P1.3)

# Cách B
chore(backend): pre-phase 4 hardening (xunit infra, swagger gating)
```

## 13. Phase 4 Backend Plan (theo V3v2 dependency)

Trạng thái: 🟡 **Planning — chờ owner duyệt**.

Phase 4 backend được lập theo dependency của 5 Wave Phase 4+ (UI Redesign V3 source of truth, page Figma `65:2`). Tham chiếu chi tiết V3 + 5 Wave + 5 owner decision risk: `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4`.

### 13.1 Phase 4.1 - Domain Service

```txt
Trạng thái   : 🔜 Wave A unblock contract → Wave B-D implement thật.
Ưu tiên      : Wave A FE cần OpenAPI contract (mock OK); Wave B/D cần thật.
Effort dự kiến: ~10-15 dev-day cho contract + DNS verify + SSL ACME.

API endpoint dự kiến:
  POST /api/tenants/{tenantId}/domains
  GET  /api/tenants/{tenantId}/domains
  GET  /api/tenants/{tenantId}/domains/{domainId}
  POST /api/tenants/{tenantId}/domains/{domainId}/verify
  GET  /api/tenants/{tenantId}/domains/{domainId}/verify-status (polling)
  POST /api/tenants/{tenantId}/domains/{domainId}/retry (DNS retry)
  POST /api/tenants/{tenantId}/publish

Dependency:
  PostgreSQL (table tenant_domains schema platform).
  Redis (cache domain → tenantId resolution; TTL ngắn, invalidate khi publish).
  Background worker (DNS verify async + SSL ACME provisioning, có thể dùng
  IHostedService hoặc tách worker service riêng nếu Phase 6 cần scale).

Tenant isolation note:
  Owner Super Admin được thao tác cross-tenant.
  Clinic Admin chỉ thao tác domain của tenant mình (tenant context bắt buộc).

Owner decision blocker:
  - DNS retry tolerance + retry max attempts (block Wave A useDomainVerifyPoll
    + Wave D DNS verify async).

Contract Open API priority cho Wave A:
  - Endpoint list + detail + verify trả ProblemDetails RFC 9457 chuẩn.
  - Schema bắt buộc: domainName, normalizedDomainName, domainType,
    status (Pending|Verified|Failed|Suspended), isPrimary, sslState
    (None|Pending|Issued|Failed), verifiedAtUtc, lastErrorMessage.
```

### 13.2 Phase 4.2 - Template Service

```txt
Trạng thái   : 🔜 Wave A unblock contract → Wave D implement apply.
Effort dự kiến: ~8-12 dev-day cho registry + apply 3-mode + diff.

API endpoint dự kiến:
  GET  /api/templates
  GET  /api/templates/{templateKey}
  POST /api/tenants/{tenantId}/template/apply
  GET  /api/tenants/{tenantId}/template/active
  POST /api/tenants/{tenantId}/template/preview-diff

Apply 3-mode (frame Figma 116:207 Template Apply Dialog):
  - apply full template (overwrite settings + content + style).
  - apply style only (chỉ overwrite design tokens + brand).
  - apply content only (chỉ overwrite homepage modules + sample content).

Dependency:
  MongoDB (template registry collection + applied template snapshot).
  PostgreSQL (link tenant ↔ active template).

Tenant isolation note:
  Apply chỉ thao tác trong tenant. Audit log bắt buộc cho mỗi apply
  (block Wave E Audit Log).

Contract Open API priority cho Wave A:
  - Schema templateKey, name, specialty, previewImage, supportedModes,
    appliedAtUtc, appliedBy.
```

### 13.3 Phase 4.3 - Website CMS Service

```txt
Trạng thái   : 🔜 Wave A unblock contract → Wave B/D implement thật.
Effort dự kiến: ~15-20 dev-day cho settings + sliders + page content +
                 draft/publish.

API endpoint dự kiến:
  GET  /api/tenants/{tenantId}/website/settings
  PUT  /api/tenants/{tenantId}/website/settings
  GET  /api/tenants/{tenantId}/website/sliders
  POST /api/tenants/{tenantId}/website/sliders
  PUT  /api/tenants/{tenantId}/website/sliders/{slideId}
  DELETE /api/tenants/{tenantId}/website/sliders/{slideId}
  GET  /api/tenants/{tenantId}/website/pages
  GET  /api/tenants/{tenantId}/website/pages/{pageKey}
  PUT  /api/tenants/{tenantId}/website/pages/{pageKey}
  POST /api/tenants/{tenantId}/website/publish
  GET  /api/tenants/{tenantId}/website/publish-history

Public read endpoint (cho Wave B Public Website):
  GET  /api/public/tenants/{slug}/settings
  GET  /api/public/tenants/{slug}/pages/{pageKey}
  (resolve qua subdomain/domain edge resolver hoặc Public BFF)

Dependency:
  MongoDB (website_settings, sliders, page_contents, template_configs,
  publish_snapshots).
  Redis (cache settings public với TTL + invalidate khi publish).
  Optimistic concurrency cho autosave (Wave D Builder).

Tenant isolation note:
  Mọi document phải có tenantId field; query phải filter tenantId.
  Public read endpoint resolve tenant qua slug/domain, không cho Clinic Admin
  thao tác cross-tenant.

Owner decision blocker:
  - Builder autosave conflict policy (optimistic merge / pessimistic lock /
    manual) → block Wave D autosave implement.
  - Tenant suspended fallback content & branding → block Wave B Public Website
    suspended state.

Contract Open API priority cho Wave A:
  - Schema settings (brandColors, logoUrl, faviconUrl, contactInfo,
    socialLinks, seoMeta, customDomain, themeKey).
  - Schema slider (id, title, subtitle, imageUrl, ctaText, ctaUrl, order, isEnabled).
  - Schema page (pageKey: home|about|services|doctors|pricing|contact|faq|blog,
    sections, lastModifiedAtUtc, publishStatus: draft|published|conflict).
```

### 13.4 Phase 5 - Catalog Service Public Read

```txt
Trạng thái   : 🔜 Wave B dependency.
Effort dự kiến: ~8-12 dev-day cho services + doctors public read.

API endpoint dự kiến (public, cho Wave B Public Website):
  GET  /api/public/tenants/{slug}/services
  GET  /api/public/tenants/{slug}/services/{serviceSlug}
  GET  /api/public/tenants/{slug}/doctors
  GET  /api/public/tenants/{slug}/doctors/{doctorSlug}
  GET  /api/public/tenants/{slug}/specialties

Write endpoint (Wave D Clinic Admin):
  CRUD services, doctors, schedule (theo Phase 6 model trong roadmap).

Dependency:
  PostgreSQL (services, service_categories, doctors, working_schedule,
  prices — schema mới chưa có, tạo migration mới khi Wave D start).
  Redis (cache public read TTL ngắn).

Tenant isolation note:
  Public read filter theo tenant (resolve slug → tenantId), không cho cross-tenant.
  Write endpoint Clinic Admin chỉ thao tác trong tenant của họ.
```

### 13.5 Phase 6 - Booking Service

```txt
Trạng thái   : 🔜 Wave C dependency.
Effort dự kiến: ~15-20 dev-day cho slot lock + reschedule + cancel + concurrency.

API endpoint dự kiến:
  GET  /api/public/tenants/{slug}/doctors/{doctorSlug}/available-slots
  POST /api/bookings
  POST /api/bookings/{bookingId}/lock        (slot lock, ETag/version)
  PATCH /api/bookings/{bookingId}/reschedule
  PATCH /api/bookings/{bookingId}/cancel
  GET  /api/tenants/{tenantId}/appointments  (Clinic Admin Wave D)
  GET  /api/tenants/{tenantId}/appointments/{appointmentId}
  PATCH /api/tenants/{tenantId}/appointments/{appointmentId}/status

Concurrency:
  Slot lock dùng ETag/version + optimistic concurrency.
  SSE (Server-Sent Events) push slot availability khi 2 user race condition.
  Phase 6 Wave C dùng polling, Wave E nâng SSE.

Insurance verify integration (Wave C):
  POST /api/bookings/{bookingId}/insurance-verify (stub).

Event:
  AppointmentCreated event publish ra Kafka (placeholder).

Dependency:
  PostgreSQL (appointments, booking_slots, patient_info, booking_source).
  Redis (slot lock với TTL ngắn 15 phút).

Tenant isolation note:
  Public booking endpoint resolve tenant qua slug/domain.
  Clinic Admin chỉ thấy appointment của tenant mình.
```

### 13.6 Phase 7 - Billing / Reports / Audit / Monitoring / Notification

```txt
Trạng thái   : 🔜 Wave E dependency.
Effort dự kiến: ~25-35 dev-day tổng cho 5 service.

Report Service:
  Aggregate KPI cross-tenant (cho Owner Admin Dashboard frame 124:2)
  + per-tenant KPI (cho Clinic Admin Dashboard frame 121:2).
  CSV/Excel export (composable useReportExport phía FE).

Audit Log Service:
  90-day retention (Linear-style) hoặc 6 năm (HIPAA) — chờ owner decision.
  API: GET /api/audit-logs với filter tenantId, actorId, action, dateRange,
  pagination cursor-based.

Monitoring Service:
  Health check 12-service (api-gateway, identity, tenant, website-cms,
  template, domain, booking, catalog, customer, billing, report,
  notification, realtime-gateway).
  API: GET /api/monitoring/services-health.

Billing Service:
  Stripe sandbox + VNPay sandbox.
  API: subscription, invoice, renewal, payment status.

Notification Service:
  Queue email/SMS/Zalo placeholder.
  Event: notification.requested.

Dependency:
  PostgreSQL + MongoDB cho audit log retention scope.
  Kafka (cho notification event consume).
  Background worker.

Owner decision blocker:
  - Audit log retention scope → block Audit Log Service schema.
  - PII patient handling rule → block security review Wave E.
```

### 13.7 Backend Verify Plan Phase 4+

```txt
Mỗi service Phase 4+:
  - dotnet restore/build/test PASS với 0 warning 0 error.
  - Migration SQL applied trên server PostgreSQL dev.
  - Health endpoint /health 200.
  - OpenAPI /openapi/v1.json 200 (Development env).
  - Smoke test 5+ case end-to-end qua api-gateway.
  - Tenant isolation test: query thiếu tenantId → fail rõ.
  - Integration test với Testcontainers PostgreSQL khi feature data
    nghiệp vụ thật (bắt đầu từ Phase 4.1 Domain).

Cross-service:
  - Event publish/consume Kafka khi domain event thật xuất hiện
    (TenantCreated, AppointmentCreated, DomainVerified, TemplateApplied).
  - Distributed tracing CorrelationIdMiddleware end-to-end.
```

### 13.8 Hard Rules Phase 4 Backend

```txt
- Không bỏ tenant isolation trong bất kỳ endpoint nào (kể cả public).
- Không hard-code secret/connection string thật.
- Không expose PostgreSQL public.
- Không dùng EF Core (giữ Dapper + Npgsql như Phase 2).
- Không skip ProblemDetails RFC 9457 cho 4xx response.
- Không gom multi-service migration vào 1 file SQL; mỗi service migrate riêng.
- Không tạo Figma file.
- Không commit/push nếu owner chưa yêu cầu.
- Mọi background work phải dùng IHostedService hoặc worker service riêng,
  không fire-and-forget từ request handler.
- Transaction boundary rõ với mỗi command có nhiều DB operation.
```

### 13.9 Điểm Dừng

```txt
- Plan ready. Backend Agent chỉ implement OpenAPI contract Wave A khi owner
  duyệt rõ. Wave B/D backend implement thật chỉ khi Wave tương ứng FE bắt đầu
  hoặc owner yêu cầu sớm.
- Owner decision blocker (audit retention, PII rule, autosave conflict policy,
  DNS retry tolerance) phải chốt trước khi service tương ứng implement schema/logic.
```

## 14. Phase 4 Wave A Backend Contract/Stub - 2026-05-12

Trạng thái: ✅ **QA verify PASS / ready for backend commit khi owner yêu cầu**.

### 14.1 Backend Status Trước Khi Làm

```txt
Phase 2 Tenant MVP Done, 5 smoke case PASS qua API Gateway.
Pre-Phase 4 hardening Done: xUnit test infra + Swagger/OpenAPI chỉ Development.
Endpoint thật đã có: /api/tenants CRUD tối thiểu qua tenant-service và api-gateway.
Service còn placeholder trước wave này: domain-service, template-service, website-cms-service.
FE Wave A cần contract Domain/Template/CMS để chuyển từ mock-first sang ghép endpoint shape.
```

### 14.2 Scope Được Chọn

```txt
FULL next backend scope trong lane BE/DevOps: Phase 4 Wave A contract/stub cho cả 3 service.
Không làm persistence thật, DNS async thật, SSL ACME, MongoDB write thật hoặc Redis cache thật.
Không sửa frontend, không sửa docs frontend, không commit/push/stage.
```

### 14.3 Agents Và Boundary

```txt
Lead Agent: điều phối scope BE, cập nhật plan/handoff, không stage/commit.
Architect Agent: chốt Wave A contract/stub, service boundary và tenant isolation.
Backend Agent: tạo shared contracts, service stubs Clean Architecture, gateway contract routes.
Database Agent: review không cần migration vì chưa persistence thật.
QA Agent: cài/bật dotnet user-local, restore/build/test/smoke PASS.
Documentation Agent: cập nhật docs/current-task.backend.md và plan backend section này.
```

### 14.4 Allowed File Areas Đã Dùng

```txt
backend/shared/contracts/**
backend/services/domain-service/**
backend/services/template-service/**
backend/services/website-cms-service/**
backend/services/api-gateway/src/ApiGateway.Api/**
backend/ClinicSaaS.Backend.sln
docs/current-task.backend.md
temp/plan.backend.md
```

### 14.5 Endpoints/Contracts Hoàn Thành Ở Mức Stub

```txt
Domain Service + API Gateway contract:
  GET  /api/tenants/{tenantId}/domains
  GET  /api/tenants/{tenantId}/domains/{domainId}
  POST /api/tenants/{tenantId}/domains
  POST /api/tenants/{tenantId}/domains/{domainId}/verify
  GET  /api/tenants/{tenantId}/domains/{domainId}/verify-status
  GET  /api/tenants/{tenantId}/domains/{domainId}/ssl-status
  POST /api/tenants/{tenantId}/publish

Template Service + API Gateway contract:
  GET  /api/templates
  GET  /api/templates/{templateKey}
  POST /api/tenants/{tenantId}/template/apply
  GET  /api/tenants/{tenantId}/template/active
  POST /api/tenants/{tenantId}/template/preview-diff

Website CMS Service + API Gateway contract:
  GET/PUT /api/tenants/{tenantId}/website/settings
  GET/POST /api/tenants/{tenantId}/website/sliders
  PUT/DELETE /api/tenants/{tenantId}/website/sliders/{slideId}
  GET     /api/tenants/{tenantId}/website/pages
  GET/PUT /api/tenants/{tenantId}/website/pages/{pageKey}
  POST    /api/tenants/{tenantId}/website/publish
  GET     /api/tenants/{tenantId}/website/publish-history
```

### 14.6 Data/DB Decision

```txt
DB/schema: không có migration trong wave này.
Lý do: contract/stub trả dữ liệu Application layer, chưa persistence thật.
Phase sau:
  Domain: PostgreSQL platform domain registry + Redis domain mapping cache.
  Template: MongoDB template registry/snapshot + PostgreSQL active template link.
  Website CMS: MongoDB settings/sliders/pages/publish snapshots + Redis public cache.
```

### 14.7 Verify

```txt
where dotnet: không có trong PATH.
winget install Microsoft.DotNet.SDK.9: fail do msstore certificate mismatch và installer exit 1603.
dotnet-install.ps1 user-local: PASS, SDK 9.0.313 cài tại C:\Users\nvhoa2\.dotnet.
dotnet --info: SDK 9.0.313, host/runtime 9.0.15, Windows win-x64.
git diff --check: PASS.
dotnet restore backend/ClinicSaaS.Backend.sln: PASS.
dotnet build backend/ClinicSaaS.Backend.sln --no-restore: FAIL lần 1 do CS8506 trong TemplateContractStubHandler; đã fix bằng khai báo `string[] changedAreas`.
dotnet build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 Warning(s), 0 Error(s).
dotnet test backend/ClinicSaaS.Backend.sln --no-build: PASS, 21/21 tests.
Local Development smoke: PASS.
```

### 14.8 Smoke PASS

```txt
Domain Service:
  GET /health -> 200
  GET /openapi/v1.json -> 200
  GET /api/tenants/{tenantId}/domains với X-Tenant-Id khớp route -> 200

Template Service:
  GET /health -> 200
  GET /openapi/v1.json -> 200
  GET /api/templates -> 200

Website CMS Service:
  GET /health -> 200
  GET /openapi/v1.json -> 200
  GET /api/tenants/{tenantId}/website/settings với X-Tenant-Id khớp route -> 200

API Gateway:
  GET /health -> 200
  GET /openapi/v1.json -> 200
  GET /api/tenants/{tenantId}/domains với X-Tenant-Id khớp route -> 200
  GET /api/templates -> 200
  GET /api/tenants/{tenantId}/website/settings với X-Tenant-Id khớp route -> 200
```

### 14.9 Commit Split Đề Xuất

```txt
feat(backend): add phase 4 domain template cms contract stubs
test(backend): add phase 4 service stub smoke tests
docs(backend): record phase 4 wave a backend contract handoff
```

## 15. Backend Phase 4 Wave A.2 - Owner Plan Module Contract/Stub - 2026-05-12

Trạng thái: ✅ **Implementation + QA PASS**, chờ owner yêu cầu commit. Không stage/commit/push.

### 15.1 Scope

```txt
Implement FULL backend contract/stub cho Owner Plan & Module Catalog để FE A8 `/plans`
có API shape thật dần.

Endpoint:
  GET  /api/owner/plans
  GET  /api/owner/modules
  GET  /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change

Không làm persistence thật.
Không tạo migration/schema.
Không dùng DB/server/secret thật.
Không sửa frontend hoặc docs frontend.
```

### 15.2 Agents Và Boundary

```txt
Lead Agent: điều phối lane Backend/DevOps, giữ guardrail không stage/commit/push.
Architect Agent: chốt service owner là Tenant Service, Billing Service để phase sau.
Backend Agent: thêm shared contracts, Tenant Service application stub, API endpoints, API Gateway contract routes.
Database Agent: xác nhận chưa cần migration vì chưa persistence thật.
QA Agent: restore/build/test/smoke Development + forbidden ClinicAdmin check.
Documentation Agent: cập nhật docs/current-task.backend.md, temp/plan.backend.md, dashboard ngắn.
```

### 15.3 Architect Decision

```txt
Service owner: Tenant Service.
Lý do: plan catalog/module entitlement/tenant-plan assignment hiện gắn tenant lifecycle
và module enablement. Billing Service sẽ sở hữu subscription/pricing/invoice/payment khi
owner duyệt persistence thật.

API Gateway:
  Expose `/api/owner/*` contract route để FE gọi shape ổn định.
  Không truy database.

Security:
  Endpoint platform-scoped/cross-tenant chỉ dành cho Owner Super Admin.
  Metadata: RequireRole OwnerSuperAdmin + PermissionCodes.PlansRead/PlansWrite.
  Guard placeholder route-specific chặn `X-Owner-Role: ClinicAdmin` hoặc role header khác.
```

### 15.4 Allowed File Areas Đã Dùng

```txt
backend/shared/contracts/**
backend/services/tenant-service/src/**
backend/services/tenant-service/tests/**
backend/services/api-gateway/src/ApiGateway.Api/**
docs/current-task.backend.md
temp/plan.backend.md
docs/current-task.md
```

### 15.5 Contract Shape

```txt
GET /api/owner/plans
  items[]: code, name, price, description, tenantCount, tone, popular
  Data: Starter/Growth/Premium.

GET /api/owner/modules
  items[]: id, name, category, starter, growth, premium
  Entitlement cell giữ bool hoặc string limit như FE mock.

GET /api/owner/tenant-plan-assignments
  items[]: id, slug, currentPlan, currentPlanName, currentMrr,
           nextRenewal, selected, targetPlan

POST /api/owner/tenant-plan-assignments/bulk-change
  request: selectedTenantIds, targetPlan, effectiveAt, auditReason
  validation: selectedTenantIds không rỗng, targetPlan hợp lệ,
              effectiveAt = next_renewal, auditReason bắt buộc
  response: changedCount, mrrDiff, status, message, effectiveAt, auditReason
```

### 15.6 Data/DB Decision

```txt
DB/schema: không có migration.
Lý do: Wave A.2 chỉ contract/stub in-memory để unblock FE A8.
Phase sau mới thiết kế PostgreSQL plan/subscription/assignment tables,
module entitlement source of truth và Billing integration khi owner duyệt.
```

### 15.7 Verify

```txt
git diff --check: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 Warning(s), 0 Error(s).
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 25/25 tests.
Local Development smoke: PASS.
```

Smoke detail:

```txt
Tenant Service :5006
  GET /health -> 200
  GET /openapi/v1.json -> 200
  GET /api/owner/plans -> 200
  GET /api/owner/modules -> 200
  GET /api/owner/tenant-plan-assignments -> 200
  POST /api/owner/tenant-plan-assignments/bulk-change -> 200
  POST bulk-change với X-Owner-Role=ClinicAdmin -> 403

API Gateway :5018
  GET /health -> 200
  GET /openapi/v1.json -> 200
  GET /api/owner/plans -> 200
  GET /api/owner/modules -> 200
  GET /api/owner/tenant-plan-assignments -> 200
  POST /api/owner/tenant-plan-assignments/bulk-change -> 200
  POST bulk-change với X-Owner-Role=ClinicAdmin -> 403
```

### 15.8 Commit Split Đề Xuất

```txt
feat(backend): add owner plan module contract stubs
test(backend): cover owner plan module stub handler
docs(backend): record owner plan module contract handoff
```

## 16. Backend Phase 4 Wave B - Owner Plan/Module Persistence Preparation - 2026-05-12

Trạng thái: 🟡 **Plan ready / approval gate**. Chưa implement code, chưa tạo migration/schema, chưa dùng DB/server/secret thật.

### 16.1 Lane Và Agents

```txt
Lane: Backend/DevOps.
Không sửa frontend.
Không stage/commit/push.

Agents tự chọn:
  Lead Agent: điều phối, cập nhật plan/handoff/dashboard.
  Architect Agent: chốt service boundary Tenant Service vs Billing Service.
  Database Agent: thiết kế schema dự kiến, query path, index và migration guardrail.
  Backend Agent: chuẩn bị hướng implement repository/use case/API wiring sau khi owner duyệt.
  QA Agent: định nghĩa test/smoke cho endpoint owner plan/module.
  Documentation Agent: cập nhật docs lane và dashboard ngắn.
  DevOps Agent: chỉ tham gia sau khi cần runtime DB smoke, không dùng server thật ở planning này.
```

### 16.2 Architect + Database Decision

```txt
Service owner hiện tại: Tenant Service.

Lý do:
  - Plan catalog, module entitlement và tenant-plan assignment đang gắn trực tiếp với tenant lifecycle,
    create clinic flow và module enablement.
  - Existing Tenant Service đã sở hữu platform.tenants và platform.tenant_modules.
  - FE A8/A9 cần cùng endpoint contract `/api/owner/*`, không cần Billing payment runtime.

Billing Service phase sau:
  - Sở hữu subscription, invoice, payment, renewal collection và payment provider integration.
  - Không query trực tiếp bảng private của Tenant Service.
  - Tích hợp qua API/event khi Billing thật được duyệt, ví dụ TenantPlanChanged hoặc SubscriptionRenewed.

API Gateway:
  - Không truy database.
  - Giai đoạn persistence thay static contract stub bằng forwarding/typed HttpClient tới Tenant Service cho `/api/owner/*`.

Security:
  - `/api/owner/*` là Owner Super Admin platform-scoped/cross-tenant explicit use case.
  - ClinicAdmin phải nhận 403, tối thiểu với list assignment và bulk-change; đề xuất áp dụng cho cả 4 endpoint owner plan/module.
```

### 16.3 Scope Nếu Owner Duyệt Implement

```txt
Implement persistence thật cho endpoint A.2 hiện có:
  GET  /api/owner/plans
  GET  /api/owner/modules
  GET  /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change

Mục tiêu:
  - Thay data in-memory bằng repository Dapper/Npgsql trong Tenant Service.
  - Giữ response contract hiện tại để không phá FE A8/A9.
  - Bulk-change chạy transaction rõ ràng khi update nhiều tenant assignment.
  - Map validation/conflict/not-found thành ProblemDetails/ValidationProblem rõ.
  - API Gateway tiếp tục không truy DB.
```

### 16.4 Out Of Scope

```txt
Không tạo migration/schema trước khi owner duyệt.
Không implement Billing Service.
Không tích hợp Stripe/VNPay.
Không đổi frontend/API shape trừ khi owner duyệt riêng.
Không dùng DB/server/connection string/secret thật.
Không sửa `frontend/`.
Không tạo Figma file.
Không commit/push/stage.
```

### 16.5 Allowed File Areas Khi Được Duyệt

```txt
backend/shared/contracts/Tenancy/**
backend/shared/contracts/Authorization/**
backend/services/tenant-service/src/TenantService.Domain/**
backend/services/tenant-service/src/TenantService.Application/Plans/**
backend/services/tenant-service/src/TenantService.Application/Tenants/**
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/**
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/**
backend/services/tenant-service/src/TenantService.Api/Endpoints/**
backend/services/tenant-service/tests/TenantService.Tests/**
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/**
docs/current-task.backend.md
temp/plan.backend.md
docs/current-task.md
```

### 16.6 Schema Dự Kiến

Chỉ là thiết kế để owner duyệt, chưa tạo migration.

```txt
platform.plans
  plan_code text primary key
  name text not null
  description text null
  price_monthly numeric(12,2) not null
  currency_code text not null default 'USD'
  tone text null
  is_popular boolean not null default false
  display_order integer not null
  is_active boolean not null default true
  created_at_utc timestamptz not null
  updated_at_utc timestamptz not null

platform.modules
  module_code text primary key
  name text not null
  category text not null
  description text null
  display_order integer not null
  is_active boolean not null default true
  created_at_utc timestamptz not null
  updated_at_utc timestamptz not null

platform.plan_module_entitlements
  plan_code text not null references platform.plans(plan_code)
  module_code text not null references platform.modules(module_code)
  is_enabled boolean not null
  limit_value text null
  display_value text null
  created_at_utc timestamptz not null
  updated_at_utc timestamptz not null
  primary key (plan_code, module_code)

platform.tenant_plan_assignments
  id uuid primary key
  tenant_id uuid not null references platform.tenants(id)
  current_plan_code text not null references platform.plans(plan_code)
  target_plan_code text null references platform.plans(plan_code)
  effective_at text not null default 'next_renewal'
  next_renewal_date date null
  current_mrr numeric(12,2) not null
  currency_code text not null default 'USD'
  status text not null default 'active'
  audit_reason text null
  assigned_by_user_id text null
  assigned_at_utc timestamptz not null
  updated_at_utc timestamptz not null
  version integer not null default 1

platform.tenant_plan_assignment_changes (đề xuất kèm để giữ audit tối thiểu)
  id uuid primary key
  bulk_operation_id uuid null
  tenant_id uuid not null references platform.tenants(id)
  from_plan_code text not null
  to_plan_code text not null
  effective_at text not null
  audit_reason text not null
  actor_user_id text null
  created_at_utc timestamptz not null
```

### 16.7 Constraint Và Index Dự Kiến

```txt
plans:
  ck_plans_price_monthly_non_negative
  ck_plans_currency_code_not_empty
  idx_plans_is_active_display_order

modules:
  idx_modules_category_display_order
  idx_modules_is_active_display_order

plan_module_entitlements:
  idx_plan_module_entitlements_plan_code
  idx_plan_module_entitlements_module_code

tenant_plan_assignments:
  ux_tenant_plan_assignments_tenant_id
  idx_tenant_plan_assignments_current_plan_code
  idx_tenant_plan_assignments_target_plan_code
  idx_tenant_plan_assignments_status_next_renewal_date
  ck_tenant_plan_assignments_effective_at
  ck_tenant_plan_assignments_status
  ck_tenant_plan_assignments_current_mrr_non_negative

tenant_plan_assignment_changes:
  idx_tenant_plan_assignment_changes_tenant_id_created_at
  idx_tenant_plan_assignment_changes_bulk_operation_id
```

### 16.8 Migration/Backfill Strategy Dự Kiến

```txt
Migration đề xuất sau duyệt: 0002_add_owner_plan_module_persistence.sql.

1. Tạo bảng catalog platform.plans, platform.modules, platform.plan_module_entitlements.
2. Seed idempotent Starter/Growth/Premium và module matrix từ contract A.2.
3. Tạo platform.tenant_plan_assignments.
4. Backfill assignment từ platform.tenants.plan_code, plan_display_name và current tenant list.
5. Giữ platform.tenants.plan_code/plan_display_name làm snapshot tương thích Phase 2 ở wave đầu;
   không drop/rename trong migration này.
6. Giữ platform.tenant_modules làm tenant-owned module enablement thực tế; plan_module_entitlements là source
   template theo plan, còn tenant_modules là snapshot/override theo tenant.
7. Ghi SQL comment tiếng Việt + COMMENT ON cho table/column/index quan trọng.
8. Không destructive/drop/truncate.
```

### 16.9 Backend Implementation Plan Sau Duyệt

```txt
1. Thêm domain/application model tối thiểu cho plan catalog, module entitlement và tenant assignment.
2. Tách interface repository khỏi OwnerPlanCatalogStubHandler:
   - IPlanCatalogRepository
   - ITenantPlanAssignmentRepository
3. Implement Dapper repository trong TenantService.Infrastructure.
4. List plans/modules đọc từ PostgreSQL, sort theo display_order/category.
5. List assignments join platform.tenants + tenant_plan_assignments, trả contract hiện tại.
6. Bulk-change:
   - validate selectedTenantIds, targetPlan, effectiveAt, auditReason.
   - transaction update tenant_plan_assignments + insert tenant_plan_assignment_changes.
   - tính changedCount và mrrDiff từ current_mrr/target plan price.
7. Cập nhật DI để dùng repository thật; giữ stub chỉ nếu cần test hoặc fallback dev đã được duyệt.
8. Thay API Gateway static stub bằng forwarding/typed HttpClient tới Tenant Service, không thêm DB dependency vào gateway.
9. Không đổi endpoint path/response field cho FE.
```

### 16.10 Test/Smoke Plan

```txt
Static/build:
  git diff --check
  C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln
  C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore
  C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build

Database verify sau khi có migration:
  Apply migration trên DB dev/local được owner duyệt.
  Verify tables/constraints/indexes tồn tại.
  Verify seed Starter/Growth/Premium + module matrix.
  Verify backfill tenant_plan_assignments từ tenant hiện có.

API smoke Tenant Service:
  GET /health -> 200
  GET /openapi/v1.json -> 200 trong Development
  GET /api/owner/plans -> 200, trả plan từ DB, không rỗng
  GET /api/owner/modules -> 200, trả module matrix từ DB
  GET /api/owner/tenant-plan-assignments -> 200, join đúng tenant hiện có
  POST /api/owner/tenant-plan-assignments/bulk-change -> 200, transaction update đúng
  POST bulk-change thiếu auditReason -> 400 validation
  POST bulk-change targetPlan không hợp lệ -> 400 validation
  ClinicAdmin 403 cho `/api/owner/*`, tối thiểu assignment list và bulk-change

API smoke API Gateway:
  Lặp lại 4 endpoint `/api/owner/*` qua gateway.
  Verify ClinicAdmin 403 qua gateway.
  Verify gateway không cần DB connection string.
```

### 16.11 Acceptance Criteria

```txt
- Service ownership ghi rõ: Tenant Service hiện tại, Billing Service phase sau.
- Không có frontend diff.
- Không có migration/schema khi chưa được owner duyệt.
- Schema dự kiến đủ plans/modules/tenant_plan_assignments và audit tối thiểu.
- Query path + index dự kiến rõ.
- Bulk-change có transaction boundary rõ.
- Test/smoke cover list plans, list modules, list assignments, bulk change, ClinicAdmin 403.
- API contract hiện tại giữ ổn định cho FE A8/A9.
```

### 16.12 Có Thể Chạy Song Song Với FE A9

```txt
Có thể chạy song song:
  - FE A9 tiếp tục dùng contract hiện có `/api/owner/*` hoặc mock/auto mode.
  - Backend persistence implement phía sau cùng response shape, không cần FE đổi trước.
  - Backend unit/integration test có thể chạy độc lập.
  - API Gateway route shape giữ nguyên nên không block FE.

Không nên chạy song song nếu:
  - FE A9 muốn đổi field contract của `/plans` hoặc assignment payload.
  - Owner quyết định chuyển service owner sang Billing Service ngay trong wave này.
  - Owner muốn implement Billing payment/subscription thật cùng lúc.

Coordination note:
  - Nếu Backend đổi validation/status message, phải báo FE để cập nhật error state copy.
  - Nếu thêm field response, chỉ add-only để không phá FE.
```

### 16.13 Điểm Dừng Và Owner Approval Cần Có

```txt
Dừng tại plan. Chờ owner duyệt rõ trước khi code/migration:
  "Tôi duyệt plan Backend Phase 4 Wave B persistence, bắt đầu implement"
  hoặc "Duyệt, tạo migration Tenant Service 0002".

Nếu owner chỉ muốn chuẩn bị tiếp mà chưa tạo schema:
  - Có thể tách task tiếp theo thành design-only review/OpenAPI contract review.
```

## 17. BE A.3 Contract Hardening + FE A9 Support Plan - 2026-05-12

Trạng thái: **Plan ready / có thể chạy song song FE A9; chưa implement code**.

### 17.1 Mục Tiêu

```txt
Tăng độ chắc của contract BE A.2 để FE A9 gọi `/api/owner/*` ổn định hơn,
không đụng persistence/schema/migration và không đổi response shape hiện tại.
```

### 17.2 Agents

```txt
Lead Agent: điều phối lane và report song song với FE.
Architect Agent: giữ service boundary Tenant Service + API Gateway, không kéo Billing/DB vào wave này.
Backend Agent: bổ sung test/guard nếu owner duyệt implement.
QA Agent: restore/build/test + local Development smoke service/gateway.
Documentation Agent: cập nhật backend lane.
Database Agent: chỉ xác nhận no migration.
```

### 17.3 Scope Có Thể Làm Song Song

```txt
- Không đổi endpoint/field contract:
  GET /api/owner/plans
  GET /api/owner/modules
  GET /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change
- Tăng test cho ClinicAdmin 403, owner role invalid, validation bulk-change.
- Rà OpenAPI/gateway consistency để FE A9 real smoke không gặp lệch route.
- Không persistence, không Billing/subscription, không migration 0002.
```

### 17.4 Allowed Files Nếu Owner Duyệt Implement

```txt
backend/shared/contracts/**
backend/services/tenant-service/src/TenantService.Api/**
backend/services/tenant-service/src/TenantService.Application/Plans/**
backend/services/tenant-service/tests/**
backend/services/api-gateway/src/ApiGateway.Api/**
backend/services/api-gateway/tests/**
docs/current-task.backend.md
temp/plan.backend.md
```

### 17.5 Verify Commands

```powershell
git diff --check
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build
```

Local smoke nếu runtime bật:

```txt
Tenant Service + API Gateway:
  GET /health
  GET /openapi/v1.json
  GET /api/owner/plans
  GET /api/owner/modules
  GET /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change
  Negative: ClinicAdmin 403, invalid targetPlan 400, missing auditReason 400.
```

### 17.6 Out Of Scope / Approval Gate

```txt
- Không tạo schema/migration/repository persistence trong A.3.
- Không triển khai Billing Service/subscription/invoice/payment.
- Không đổi FE A9 contract hoặc UI.
- Persistence thật vẫn theo §16, chờ owner duyệt riêng.
```
