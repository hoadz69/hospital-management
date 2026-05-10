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
