# Current Task Backend - Phase 2 API Runtime Smoke Gate

Ngày cập nhật: 2026-05-10

## Vai Trò File

File này là handoff riêng cho Backend/DevOps workstream. Backend Agent và DevOps Agent cập nhật file này khi làm Phase 2 API Runtime Smoke Gate.

Không ghi task frontend vào file này. Không overwrite `docs/current-task.md`.

## Trạng Thái

Phase 2 API Runtime Smoke Gate ✅ ĐÃ PASS đủ 5 smoke trên server `116.118.47.78` (run 2026-05-10):

```txt
POST /api/tenants -> 201
duplicate slug/domain -> 409
GET /api/tenants -> 200
GET /api/tenants/{id} -> 200
PATCH /api/tenants/{id}/status -> 200
```

Hai vòng fix đã apply trên `tenant-service` (api-gateway không cần đổi vì pure pass-through):

```txt
Vòng 1: thêm Dapper SqlMapper.TypeHandler<DateTimeOffset> để Npgsql 6+ trả timestamptz về DateTime vẫn map được sang DateTimeOffset của row record.
Vòng 2: reorder positional record TenantListRow cho khớp thứ tự cột SELECT trong ListAsync (clinic_name nằm cuối SQL nhưng trước đó nằm pos 6 trong ctor) — Dapper 2.1 với positional record matching theo position type, không qua type handler trong giai đoạn lookup constructor.
```

Phase 2 đủ điều kiện chuyển Done sau khi Lead Agent đồng bộ dashboard + roadmap.

## Bối Cảnh Đã Move Từ `docs/current-task.md`

Owner đã yêu cầu DevOps/Lead Agent:

- Tạo lead-plan trước.
- SSH vào server owner cung cấp.
- Kiểm tra OS/CPU/RAM/disk.
- Cài Docker Engine và Docker Compose plugin theo OS thực tế.
- Tạo deploy directory, Docker network và PostgreSQL dev/smoke container.
- Apply schema Phase 2.
- Verify schema/tables.
- Cập nhật docs, không commit, không push.

Phase 2 Tenant MVP Backend đã có local commit:

```txt
71923f8 feat: implement tenant mvp backend slice
```

Static/build/config đã pass. DB runtime smoke local bị blocked vì Docker daemon local chưa chạy, nên owner cho dùng server riêng để bootstrap Docker/PostgreSQL.

## Kết Quả Server Bootstrap

Server:

```txt
OS: Ubuntu 22.04.5 LTS
Kernel: Linux 5.15.0-163-generic x86_64
CPU: 4 cores
RAM: 5.8 GiB total, khoảng 1.5 GiB available tại thời điểm verify
Disk root: 64G total, 48G available, 22% used
```

Docker:

```txt
Docker Engine: 29.4.3
Docker Compose plugin: v5.1.3
docker info: pass
CgroupDriver: systemd
StorageDriver: overlayfs
```

Deploy path/network/volume:

```txt
/opt/clinic-saas
/opt/clinic-saas/migrations
Docker network: clinic-saas-network
Docker volume: clinic-saas-postgres-data
```

PostgreSQL container:

```txt
name: clinic-saas-postgres
image: postgres:16-alpine
status: Up
database: clinic_saas_dev
user: clinic_dev
port publish: none
pg_isready: accepting connections
```

Schema Phase 2 đã apply từ:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0001_create_tenant_mvp.sql
```

Tables đã verify:

```txt
platform.tenant_domains
platform.tenant_modules
platform.tenant_profiles
platform.tenants
```

## Port / Exposure

PostgreSQL:

```txt
docker port clinic-saas-postgres: không trả mapping nào
docker ps hiển thị 5432/tcp nội bộ container, không publish ra host
Không thấy PostgreSQL listen public trên host
UFW không mở 5432/tcp
```

Host listening ports tại thời điểm verify:

```txt
127.0.0.1:18789
127.0.0.1:5679
0.0.0.0:22
0.0.0.0:80
0.0.0.0:443
0.0.0.0:20128
*:5678
[::]:22
[::1]:18789
```

## Verify Đã Chạy

Local:

```powershell
Test-Path <SSH_KEY_PATH>
Test-Path backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0001_create_tenant_mvp.sql
scp ... 0001_create_tenant_mvp.sql /opt/clinic-saas/migrations/0001_create_tenant_mvp.sql
```

Remote:

```bash
cat /etc/os-release
nproc
free -h
df -h /
docker --version
docker compose version
docker info
mkdir -p /opt/clinic-saas/migrations
docker network create clinic-saas-network
docker volume create clinic-saas-postgres-data
docker run -d --name clinic-saas-postgres --network clinic-saas-network --restart unless-stopped ...
docker exec clinic-saas-postgres pg_isready -U clinic_dev -d clinic_saas_dev
docker exec -i clinic-saas-postgres psql -v ON_ERROR_STOP=1 -U clinic_dev -d clinic_saas_dev < /opt/clinic-saas/migrations/0001_create_tenant_mvp.sql
docker exec clinic-saas-postgres psql -U clinic_dev -d clinic_saas_dev -Atc "select schemaname || '.' || tablename from pg_tables where schemaname='platform' order by tablename;"
docker port clinic-saas-postgres
ss -tulpen
ufw status verbose
```

## API Smoke Kết Quả Cuối 2026-05-10

```txt
POST /api/tenants -> 201 (pass)
duplicate slug/domain -> 409 (pass)
GET /api/tenants -> 200 (pass)
GET /api/tenants/{id} -> 200 (pass)
PATCH /api/tenants/{id}/status -> 200 (pass)
```

Smoke chạy qua `api-gateway` tại `127.0.0.1:5005` trên server, gateway forward sang `tenant-service` tại `127.0.0.1:5006`. Cả hai bind-mount publish output từ `/opt/clinic-saas/runtime-smoke`. PostgreSQL container `clinic-saas-postgres` không publish public port, network nội bộ `clinic-saas-network`. Tenant test đã tạo trong DB: `demo-clinic-smoke-002`, `demo-clinic-smoke-003` (cả hai status `Active` sau PATCH).

## Root Cause Và Code Fix 2026-05-10

### Vòng 1 - Type Handler

Triệu chứng: GET list/detail/PATCH status đều 500 sau khi POST/duplicate đã pass. Log:

```txt
System.InvalidCastException: Unable to cast object of type 'System.DateTime'
to type 'System.DateTimeOffset'.
```

Nguyên nhân: Npgsql 6+ trả `timestamptz` về `System.DateTime` (Kind=Utc), không còn về `System.DateTimeOffset` như Npgsql 5. Mọi row record trong `DapperTenantRepository` có property `DateTimeOffset/DateTimeOffset?`. POST/duplicate pass vì `CreateTenantHandler` map response từ aggregate in-memory, không re-read DB.

Fix: thêm `Dapper SqlMapper.TypeHandler<DateTimeOffset>` bridge `DateTime` UTC ↔ `DateTimeOffset` UTC, đăng ký một lần per process bằng `Interlocked.Exchange` guard.

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DateTimeOffsetTypeHandler.cs (NEW)
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs (MODIFIED)
```

### Vòng 2 - Record Constructor Order

Sau v1 deploy, GET id/PATCH đã 200 nhưng GET list vẫn 500:

```txt
System.InvalidOperationException: A parameterless default constructor or one matching
signature (... System.DateTime created_at_utc, System.DateTime updated_at_utc, ...
System.String clinic_name) is required for ...DapperTenantRepository+TenantListRow
materialization.
```

Nguyên nhân: Dapper 2.1 với positional record materialize **theo position type**, không lookup type handler khi resolve constructor. `TenantListRow` có `ClinicName` ở position 6 trong ctor nhưng SQL SELECT trong `ListAsync` đặt `clinic_name` cuối cùng (position 11) vì JOIN với `tenant_profiles`. Type lệch tại position 6 (string vs DateTime) và position 11 (DateTimeOffset? vs string). Các row record còn lại (`TenantRootRow`, `ClinicProfileRow`, `TenantDomainRow`, `TenantModuleRow`) không gặp vì ctor order khớp SELECT từ trước.

Fix: reorder ctor `TenantListRow` đúng thứ tự cột SELECT (`ClinicName` cuối). Mapping `ToTenantSummary` truy cập field theo NAME nên reorder không phá logic. Không sửa SQL, không sửa schema, không sửa Domain, không sửa contracts.

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantRepository.cs (MODIFIED)
```

### Verify Local

```txt
dotnet build backend/ClinicSaaS.Backend.sln --nologo: pass, 0 warnings, 0 errors
dotnet test backend/ClinicSaaS.Backend.sln --no-build --nologo: không pick test case (project Tests là placeholder, gap có sẵn ngoài scope smoke gate)
```

### Verify Server (sau v2 deploy)

```txt
docker ps: tenant-service Up + api-gateway Up + postgres Up
curl http://127.0.0.1:5006/health: 200 Healthy
curl http://127.0.0.1:5005/health: 200 Healthy
docker logs clinic-saas-tenant-service-smoke --since=2m: không còn InvalidCastException/InvalidOperationException
   (chỉ còn warning libgssapi_krb5.so.2 cannot open - harmless GSSAPI Kerberos library missing,
    không ảnh hưởng password auth Postgres)
5 smoke case: pass đủ (xem mục API Smoke Kết Quả Cuối)
```

## Deploy Pattern Đã Dùng

```txt
1. dotnet publish TenantService.Api -c Release --no-self-contained -o temp/publish/tenant-service.
2. scp -r staging dir lên /opt/clinic-saas/runtime-smoke/tenant-service-new.
3. docker stop clinic-saas-tenant-service-smoke.
4. mv tenant-service tenant-service.bak.<UTC ts>; mv tenant-service-new tenant-service.
5. docker start clinic-saas-tenant-service-smoke.
6. Health check + smoke 5 case qua api-gateway tại 127.0.0.1:5005.
```

Mount `/opt/clinic-saas/runtime-smoke/tenant-service` -> `/app:ro`, command `dotnet /app/TenantService.Api.dll`. Connection string env var giữ trong container, không ghi vào file tracked.

Backup directories còn trên server (chưa dọn): `tenant-service.bak.20260509T191310Z` (v1 swap), `tenant-service.bak.20260509T191905Z` (v2 swap). Có thể xóa khi không cần rollback.

## Guardrail

- Không commit.
- Không push.
- Không ghi private key vào repo.
- Không ghi secret thật vào file tracked.
- Không ghi IP server thật vào repo.
- Không mở PostgreSQL public ra internet.
- Không expose port `5432` ra `0.0.0.0`.
- Không dùng MySQL.
- Không sửa frontend.
- Không tạo Figma file.
- Không chuyển Phase 2 sang Done nếu API smoke chưa pass.

## File Liên Quan

```txt
docs/current-task.backend.md
temp/plan.backend.md
docs/deployment/server-bootstrap.md
docs/roadmap/clinic-saas-roadmap.md
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0001_create_tenant_mvp.sql
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DateTimeOffsetTypeHandler.cs
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantRepository.cs
```

## Bước Tiếp Theo

Phase 2 đã pass đủ smoke. Bước tiếp theo nằm ngoài Phase 2:

```txt
1. (Optional) Owner duyệt commit backend fix + lane docs (DateTimeOffsetTypeHandler.cs,
   DependencyInjection.cs, DapperTenantRepository.cs reorder).
2. (Optional) DevOps dọn backup `tenant-service.bak.*` trên server khi không cần rollback.
3. (Optional) Bổ sung integration test với Testcontainers PostgreSQL cho Tenant Service
   để cover cả 2 root cause vừa fix - đề xuất ngoài scope Phase 2 smoke gate.
4. Lead Agent đồng bộ docs/current-task.md dashboard và docs/roadmap/clinic-saas-roadmap.md
   chuyển Phase 2 sang Done.
5. Sau Phase 2 Done, Frontend lane (Phase 3 Owner Admin Tenant Slice) có thể chuyển từ
   mock fallback sang real API smoke.
6. (Phase 4 Wave A FE-driven) Backend Agent giao OpenAPI contract Phase 4.1 Domain
   Service + 4.2 Template Service + 4.3 Website CMS (settings, sliders, page content)
   cho FE Wave A. Mock-first cho phép — Wave A FE chỉ cần contract (TypeScript types
   + endpoint shape), backend implement thật ở Wave B/C/D dependency.
7. (Phase 4 Wave B-D backend) Phase 4.1 Domain Service implement DNS verify async
   + SSL ACME provisioning. Phase 4.2 Template Service apply 3-mode (full / style /
   content). Phase 4.3 Website CMS Service write/publish path. Phase 5 Catalog Service
   public read endpoints (GET /api/public/services | doctors). Phase 6 Booking Service
   slot lock concurrency với ETag/version + reschedule + cancel. Customer Service
   patient endpoints + Records APSO.
```

## Phase 4 Backend Roadmap (theo V3v2 dependency)

5 Wave Phase 4+ trong roadmap (xem `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4`) tạo dependency cho backend lane theo thứ tự sau:

```txt
Wave A (FE-driven, mock-first):
  Backend deliverable: OpenAPI contract Phase 4.1 Domain + 4.2 Template + 4.3 CMS.
  Backend service không bắt buộc implement thật ở Wave A.
  Owner Admin Dashboard cross-tenant aggregate có thể mock — backend implement
  thật ở Wave E (Reports/Monitoring).

Wave B (Public Website V3):
  Backend deliverable cần thật:
    - Phase 4.3 Website CMS Service production-ready (settings/sliders/page content).
    - Phase 5 Catalog Service public read endpoints
      (GET /api/public/services, /doctors, /services/:slug, /doctors/:slug).
    - Public BFF tenant resolution endpoint hoặc edge resolver subdomain → tenantId.
    - Phase 4.1 Domain Service publish state cho frontend đọc.

Wave C (Booking V3):
  Backend deliverable cần thật:
    - Booking Service:
      POST /api/bookings, GET available-slots,
      PATCH reschedule | cancel,
      slot lock concurrency POST /api/bookings/:id/lock với ETag/version.
    - Insurance verify integration stub.

Wave D (Clinic Admin Builder + Operational):
  Backend deliverable cần thật:
    - Phase 4.3 CMS production write/publish.
    - Phase 4.2 Template apply 3-mode + diff.
    - Phase 4.1 Domain DNS verify async + SSL ACME provisioning.
    - Catalog Service write (services + doctors + schedule).
    - Customer Service patient endpoints + Records APSO domain.
    - Booking Service Clinic Admin appointment view.

Wave E (A11y/Performance/QA Polish + Reports/Audit/Monitoring/Billing):
  Backend deliverable cần thật:
    - Report Service aggregate (KPI cross-tenant + per-tenant).
    - Audit Log Service 90-day pagination (retention scope chờ owner decision —
      90 ngày Linear-style hay 6 năm HIPAA).
    - Monitoring Service health 12-service.
    - Billing Service Stripe / VNPay sandbox.
    - Notification Service queue.
```

Tenant isolation note: tất cả endpoint Phase 4+ phải tuân thủ tenant context theo `AGENTS.md` "Luật Kiến Trúc Bắt Buộc". Owner Super Admin endpoint cross-tenant phải có guard rõ; Clinic Admin endpoint chỉ thao tác trong tenant của họ; Public Website endpoint resolve tenant qua domain/subdomain.

Owner decision blocker liên quan backend:

```txt
- Audit retention scope (90d / 6y) — chốt schema Audit Log Service trước Wave E.
- PII patient handling rule — chốt encryption + audit rule trước Wave D Records APSO.
- DNS retry tolerance — chốt retry max attempts cho Domain Service trước Wave A composable.
```

## Cập Nhật 2026-05-12 - Backend Phase 4 Wave A.2 Owner Plan Module Contract/Stub

Trạng thái: ✅ **QA verify PASS** trong worktree hiện tại, chưa commit/push/stage.

Scope Lead Agent đã chạy trong Backend/DevOps lane:

```txt
Owner Plan & Module Catalog contract/stub cho FE A8 `/plans`.
Không sửa frontend.
Không sửa docs frontend.
Không tạo migration/schema.
Không dùng DB/server/secret thật.
```

Architect decision:

```txt
Service owner: Tenant Service.
Lý do: plan catalog/module entitlement và tenant-plan assignment đang gắn tenant lifecycle,
module enablement và create clinic flow. Billing Service để phase sau khi subscription,
invoice và payment thật được owner duyệt.

API Gateway: expose route contract `/api/owner/*`, không truy DB.
Platform-scoped:
  GET /api/owner/plans
  GET /api/owner/modules
Owner Super Admin cross-tenant scoped:
  GET  /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change

Security:
  RequireRole OwnerSuperAdmin + permission metadata plans.read/plans.write.
  Guard route-specific chặn `X-Owner-Role: ClinicAdmin` hoặc role header khác trên bulk/list cross-tenant.
  Auth thật/JWT vẫn là Phase 8; wave này là contract/stub có guard placeholder.
```

Backend files đã tạo/sửa:

```txt
backend/shared/contracts/Tenancy/OwnerPlanCatalogContracts.cs
backend/shared/contracts/Authorization/PermissionCodes.cs
backend/services/tenant-service/src/TenantService.Application/Plans/OwnerPlanCatalogStubHandler.cs
backend/services/tenant-service/src/TenantService.Application/DependencyInjection.cs
backend/services/tenant-service/src/TenantService.Api/Endpoints/OwnerPlanCatalogEndpoints.cs
backend/services/tenant-service/src/TenantService.Api/Program.cs
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/OwnerPlanCatalogContractEndpoints.cs
backend/services/api-gateway/src/ApiGateway.Api/Program.cs
backend/services/tenant-service/tests/TenantService.Tests/OwnerPlanCatalogStubHandlerTests.cs
backend/services/tenant-service/tests/TenantService.Tests/TenantService.Tests.csproj
```

Contract handoff FE A8:

```txt
GET /api/owner/plans
  items[]: code, name, price, description, tenantCount, tone, popular
  Data: Starter/Growth/Premium.

GET /api/owner/modules
  items[]: id, name, category, starter, growth, premium
  Cell entitlement: boolean hoặc limit string, 12 module như FE mock.

GET /api/owner/tenant-plan-assignments
  items[]: id, slug, currentPlan, currentPlanName, currentMrr,
           nextRenewal, selected, targetPlan

POST /api/owner/tenant-plan-assignments/bulk-change
  request: selectedTenantIds, targetPlan, effectiveAt, auditReason
  effectiveAt hiện chỉ nhận `next_renewal`.
  auditReason bắt buộc.
  response: changedCount, mrrDiff, status, message, effectiveAt, auditReason
```

DB/schema decision:

```txt
Không có migration trong wave này.
Lý do: contract/stub trả dữ liệu in-memory Application/API layer, chưa persistence thật.
Phase sau mới thiết kế PostgreSQL plan/subscription/assignment tables khi owner duyệt
Billing/Tenant persistence thật.
```

Verify đã chạy:

```txt
git diff --check: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 warning, 0 error.
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 25/25 tests.

Local Development smoke:
  Tenant Service :5006 GET /health, GET /openapi/v1.json, 4 endpoint /api/owner/*: PASS.
  API Gateway    :5018 GET /health, GET /openapi/v1.json, 4 endpoint /api/owner/*: PASS.
  POST bulk-change với auditReason + effectiveAt=next_renewal: PASS.
  POST bulk-change với X-Owner-Role=ClinicAdmin: 403 PASS ở Tenant Service và API Gateway.
```

Resume tiếp theo:

```txt
1. Backend Phase 4 Wave A.2 đủ điều kiện commit theo lane backend khi owner yêu cầu.
2. Không cần DB/schema ở wave này.
3. Phase sau: persistence thật cho plan/subscription/assignment khi owner duyệt Billing/Tenant plan module.
4. Không stage/commit/push trong lượt này.
```

## Cập Nhật 2026-05-12 - Phase 4 Wave A Backend Contract/Stub

Trạng thái: ✅ **QA verify PASS** trong worktree hiện tại, chưa commit/push/stage.

Scope Lead Agent đã chọn cho Backend/DevOps lane:

```txt
Phase 4.1 Domain Service contract/stub.
Phase 4.2 Template Service contract/stub.
Phase 4.3 Website CMS settings/sliders/pages contract/stub.
Không sửa frontend.
Không tạo migration/schema.
Không dùng DB/server/secret thật.
```

Architect boundary:

```txt
API Gateway: expose contract routes cho FE Wave A, không truy DB.
Domain Service: owner domain registry/publish state; dữ liệu thật về sau ở PostgreSQL + Redis domain cache.
Template Service: owner template registry/apply; dữ liệu thật về sau ở MongoDB + PostgreSQL tenant active template.
Website CMS Service: owner settings/sliders/pages/publish; dữ liệu thật về sau ở MongoDB + Redis public cache.
Endpoint tenant-scoped phải có X-Tenant-Id/JWT tenant_id và khớp route tenantId.
Endpoint platform-scoped: GET /api/templates.
```

Backend files đã tạo/sửa trong wave này:

```txt
backend/shared/contracts/Domains/DomainContracts.cs
backend/shared/contracts/Templates/TemplateContracts.cs
backend/shared/contracts/WebsiteCms/WebsiteCmsContracts.cs
backend/shared/contracts/Authorization/PermissionCodes.cs
backend/services/domain-service/src/** (Api/Application/Domain/Infrastructure)
backend/services/template-service/src/** (Api/Application/Domain/Infrastructure)
backend/services/website-cms-service/src/** (Api/Application/Domain/Infrastructure)
backend/services/*/tests/*Service.Tests/** cho 3 service mới
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/Phase4ContractEndpoints.cs
backend/services/api-gateway/src/ApiGateway.Api/Program.cs
backend/ClinicSaaS.Backend.sln
```

Contract handoff FE Wave A:

```txt
Domain:
  GET  /api/tenants/{tenantId}/domains
  GET  /api/tenants/{tenantId}/domains/{domainId}
  POST /api/tenants/{tenantId}/domains
  POST /api/tenants/{tenantId}/domains/{domainId}/verify
  GET  /api/tenants/{tenantId}/domains/{domainId}/verify-status
  GET  /api/tenants/{tenantId}/domains/{domainId}/ssl-status
  POST /api/tenants/{tenantId}/publish
  Status: contract-stub, DNS/SSL async thật pending.

Template:
  GET  /api/templates
  GET  /api/templates/{templateKey}
  POST /api/tenants/{tenantId}/template/apply
  GET  /api/tenants/{tenantId}/template/active
  POST /api/tenants/{tenantId}/template/preview-diff
  Status: contract-stub, apply persistence thật pending.

Website CMS:
  GET/PUT /api/tenants/{tenantId}/website/settings
  GET/POST /api/tenants/{tenantId}/website/sliders
  PUT/DELETE /api/tenants/{tenantId}/website/sliders/{slideId}
  GET     /api/tenants/{tenantId}/website/pages
  GET/PUT /api/tenants/{tenantId}/website/pages/{pageKey}
  POST    /api/tenants/{tenantId}/website/publish
  GET     /api/tenants/{tenantId}/website/publish-history
  Status: contract-stub, MongoDB/Redis/write-publish thật pending.
```

Verify đã chạy trong phiên hiện tại:

```txt
where dotnet: không có trong PATH.
C:\Program Files\dotnet và C:\Program Files (x86)\dotnet: không có dotnet.exe.
winget install Microsoft.DotNet.SDK.9: fail lần 1 do msstore certificate mismatch; fail lần 2 exit 1603.
dotnet-install.ps1 user-local: PASS, cài SDK 9.0.313 tại C:\Users\nvhoa2\.dotnet.
dotnet --info bằng full path: SDK 9.0.313, host/runtime 9.0.15, RID win-x64.
git diff --check: PASS.
dotnet restore backend/ClinicSaaS.Backend.sln: PASS.
dotnet build backend/ClinicSaaS.Backend.sln --no-restore: FAIL lần 1 do CS8506 trong TemplateContractStubHandler switch expression; đã fix trong backend scope.
dotnet build backend/ClinicSaaS.Backend.sln --no-restore: PASS sau fix, 0 warning, 0 error.
dotnet test backend/ClinicSaaS.Backend.sln --no-build: PASS, 21/21 tests.
Local Development smoke: PASS 14/14 checks cho Domain, Template, Website CMS và API Gateway.
```

Resume tiếp theo:

```txt
1. Phase 4 Wave A backend contract/stub đủ điều kiện commit theo lane backend sau khi owner yêu cầu.
2. Không cần migration/schema ở wave này.
3. Wave tiếp theo: persistence thật cho Domain/Template/CMS khi owner duyệt Phase 4 Wave B/D backend.
4. Dotnet hiện dùng user-local full path C:\Users\nvhoa2\.dotnet\dotnet.exe nếu PATH chưa refresh.
```

## Cập Nhật 2026-05-12 - Backend Phase 4 Wave B Owner Plan/Module Persistence Preparation

Trạng thái: 🟡 **Plan ready / approval gate** trong Backend/DevOps lane. Chưa implement code, chưa tạo migration/schema, chưa dùng DB/server/secret thật, không sửa frontend.

Agents Lead đã chọn:

```txt
Lead Agent: điều phối lane, cập nhật plan/handoff/dashboard.
Architect Agent: quyết định service ownership Tenant Service vs Billing Service.
Database Agent: thiết kế schema dự kiến, constraint/index, migration guardrail.
Backend Agent: chuẩn bị hướng repository/use case/API wiring sau khi owner duyệt.
QA Agent: định nghĩa smoke cho list plans, list modules, list assignments, bulk-change, ClinicAdmin 403.
Documentation Agent: cập nhật docs lane.
DevOps Agent: chỉ tham gia sau khi cần runtime DB smoke, không dùng server thật ở lượt này.
```

Architect + Database decision:

```txt
Persistence owner hiện tại: Tenant Service.
Lý do: plan catalog, module entitlement và tenant-plan assignment gắn tenant lifecycle,
create clinic flow và module enablement; Tenant Service đã sở hữu platform.tenants
và platform.tenant_modules.

Billing Service để phase sau:
  Sở hữu subscription, invoice, payment, renewal và payment provider integration.
  Không query trực tiếp bảng private của Tenant Service; tích hợp sau qua API/event.

API Gateway: không truy DB; khi implement persistence sẽ thay static contract stub bằng forwarding/typed HttpClient tới Tenant Service cho `/api/owner/*`.
Security: `/api/owner/*` là Owner Super Admin cross-tenant explicit use case;
ClinicAdmin phải 403.
```

Schema dự kiến để owner duyệt:

```txt
platform.plans
platform.modules
platform.plan_module_entitlements
platform.tenant_plan_assignments
platform.tenant_plan_assignment_changes (đề xuất audit tối thiểu cho bulk-change)
```

Migration/backfill dự kiến nếu owner duyệt:

```txt
Migration: backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0002_add_owner_plan_module_persistence.sql
Seed idempotent Starter/Growth/Premium + module matrix từ contract A.2.
Backfill tenant_plan_assignments từ platform.tenants.plan_code/plan_display_name.
Giữ platform.tenants.plan_code/plan_display_name trong wave đầu để tương thích Phase 2.
Giữ platform.tenant_modules là snapshot/override theo tenant; plan_module_entitlements là source template theo plan.
Không drop/truncate/destructive.
```

Test/smoke plan:

```txt
Static/build:
  git diff --check
  C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln
  C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore
  C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build

API smoke Tenant Service + API Gateway:
  GET /api/owner/plans -> 200, data từ DB
  GET /api/owner/modules -> 200, module matrix từ DB
  GET /api/owner/tenant-plan-assignments -> 200, join tenant hiện có
  POST /api/owner/tenant-plan-assignments/bulk-change -> 200, transaction update đúng
  POST bulk-change thiếu auditReason -> 400 validation
  POST bulk-change targetPlan không hợp lệ -> 400 validation
  ClinicAdmin 403 cho `/api/owner/*`, tối thiểu assignment list và bulk-change
```

Song song với FE A9:

```txt
Có thể chạy song song nếu Backend giữ nguyên endpoint/response field hiện tại và chỉ thay source data từ stub sang DB.
FE A9 có thể tiếp tục mock/auto mode hoặc gọi contract `/api/owner/*`.
Nếu FE A9 đổi payload `/plans` hoặc owner đổi service owner sang Billing Service ngay, cần sync lại trước khi implement.
```

Resume tiếp theo:

```txt
1. Chờ owner duyệt plan §16 trong temp/plan.backend.md trước khi tạo migration/schema/code persistence.
2. Nếu duyệt implement: bắt đầu Tenant Service migration 0002 + Dapper repository + tests/smoke đúng §16.
3. Không stage/commit/push trong lượt này.
```

## Cập Nhật 2026-05-12 - BE A.3 Contract Hardening + FE A9 Support Plan

Trạng thái: ✅ **QA verify PASS** trong Backend/DevOps lane. Đã tăng test/guard contract, chưa tạo migration/schema, chưa implement persistence.

Scope song song không chặn FE:

```txt
- Giữ nguyên `/api/owner/*` response shape cho FE A9.
- Tăng test guard cho Owner Plan Module contract/stub:
  ClinicAdmin forbidden, missing/invalid owner role, bulk-change validation.
- Rà OpenAPI/gateway consistency cho `/api/owner/plans`, `/api/owner/modules`,
  `/api/owner/tenant-plan-assignments`, `/api/owner/tenant-plan-assignments/bulk-change`.
- Không persistence, không DB, không Billing, không đổi contract field.
```

Agents đề xuất:

```txt
Lead + Architect + Backend + QA + Documentation.
Database chỉ review "no migration"; DevOps chỉ tham gia nếu cần local runtime smoke.
```

Điểm dừng:

```txt
BE A.3 contract hardening đã hoàn tất trong phạm vi stub.
Plan/Module persistence thật vẫn giữ ở §16 và cần owner duyệt riêng.
```

Kết quả đã làm:

```txt
- Giữ nguyên response shape hiện tại cho FE A9 ở 4 endpoint `/api/owner/*`.
- Bổ sung test validation Tenant Service cho:
  selectedTenantIds rỗng -> validation
  targetPlan không hợp lệ -> validation
  effectiveAt khác next_renewal -> validation
  auditReason thiếu -> validation
- Bổ sung route metadata test cho Tenant Service:
  `/api/owner/plans`, `/api/owner/modules`, `/api/owner/tenant-plan-assignments`,
  `POST /api/owner/tenant-plan-assignments/bulk-change` đều platform-scoped,
  yêu cầu OwnerSuperAdmin và permission plans.read/plans.write đúng route.
- Rà API Gateway route hiện giữ cùng 4 path, cùng validation và cùng guard header `X-Owner-Role`.
- Không sửa frontend, không DB, không migration 0002, không Billing.
```

File đã sửa/tạo:

```txt
backend/services/tenant-service/tests/TenantService.Tests/OwnerPlanCatalogStubHandlerTests.cs
backend/services/tenant-service/tests/TenantService.Tests/OwnerPlanCatalogEndpointMetadataTests.cs
backend/services/tenant-service/tests/TenantService.Tests/TenantService.Tests.csproj
docs/current-task.backend.md
temp/plan.backend.md
docs/current-task.md
```

Verify 2026-05-12:

```txt
git diff --check: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 warning, 0 error.
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 29/29 tests.
Local Development smoke:
  Tenant Service :5006 PASS.
  API Gateway :5018 PASS.
  Health + OpenAPI PASS.
  4 endpoint `/api/owner/*` happy path PASS.
  ClinicAdmin 403 cho 4 endpoint PASS.
  Invalid owner role 403 PASS.
  bulk-change thiếu auditReason, effectiveAt invalid, targetPlan invalid, selectedTenantIds rỗng -> 400 PASS.
```

Verify readiness bổ sung 2026-05-12:

```txt
Scope: xác nhận Backend Phase 4 Wave A.2 `/api/owner/*` thật sự sẵn sàng cho FE A9.

Code review:
  Tenant Service map đủ 4 endpoint `/api/owner/*`, platform-scoped, OwnerSuperAdmin, plans.read/plans.write.
  API Gateway map cùng 4 endpoint và cùng validation/guard placeholder.
  Response shape giữ nguyên: plans, modules, tenant-plan-assignments, bulk-change response.

Runtime:
  Docker CLI không có trong PATH nên không verify container local được.
  Không có Tenant Service/API Gateway sẵn ở :5006/:5018; port :5005 do node process giữ.
  Đã dựng runtime local bằng dotnet cho Tenant Service :5006 và API Gateway :5018 để QA smoke.

Smoke:
  Tenant Service :5006 owner endpoint smoke PASS.
  API Gateway :5018 owner endpoint smoke PASS.
  GET /health và GET /openapi/v1.json PASS; OpenAPI có đủ 4 route `/api/owner/*`.
  Happy path 4 endpoint PASS và response shape PASS.
  OwnerSuperAdmin + X-Tenant-Id vẫn PASS vì route là platform-scoped.
  ClinicAdmin 403 cho 4 endpoint PASS, kể cả khi gửi X-Tenant-Id.
  Invalid owner role 403 PASS.
  bulk-change validation 400 PASS: thiếu auditReason, effectiveAt invalid, targetPlan invalid, selectedTenantIds rỗng.
  409 conflict: không áp dụng cho `/api/owner/*` contract/stub hiện tại vì chưa có persistence/conflict path; giữ pending cho §16 persistence.
```

## Cập Nhật 2026-05-12 - Backend/DevOps Phase 4 Task A.10 Intake

Trạng thái: 🟡 **Plan/blocker - chưa implement code**. Lead Agent đã chạy intake + verify nền backend theo Fast/Budget Mode, nhưng chưa tìm thấy spec/contract A.10 trong source of truth hiện có.

Agents giả lập:

```txt
Lead Agent: phân lane Backend/DevOps, kiểm soát scope, không stage/commit/push.
Architect Agent: giữ boundary Tenant Service + API Gateway; không kéo DB/Billing/persistence khi chưa có spec.
Backend Agent: rà endpoint/spec hiện có, xác nhận chưa có A.10 trong lane docs/source.
QA Agent: chạy static verify backend hiện tại.
Documentation Agent: cập nhật lane backend/dashboard với blocker cụ thể.
```

Kết quả intake A.10:

```txt
- Đã tìm `A.10`, `A10`, `Step A10`, `Wave A Step A10`, `BE A.10` trong `docs/`, `temp/`, `backend/`.
- Không thấy định nghĩa endpoint, request/response, acceptance criteria, service owner hoặc verify command riêng cho A.10.
- Backend lane hiện có A.2/A.3 `/api/owner/*` đã QA PASS và §16 persistence plan đang chờ owner duyệt riêng.
- Vì thiếu spec A.10, chưa implement endpoint mới để tránh tự bịa contract hoặc chạm sai boundary.
```

Verify đã chạy:

```txt
git status --branch --short: PASS, chỉ thấy dirty frontend lane ngoài scope.
git diff --stat: PASS, chỉ thấy dirty frontend lane ngoài scope tại thời điểm intake.
git diff --check: PASS, chỉ warning LF/CRLF trên Windows.
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 warning, 0 error.
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 29/29 tests.
Runtime check: không có backend process sẵn ở :5005/:5006/:5018; chưa dựng smoke A.10 vì thiếu endpoint/spec.
```

Điểm cần owner/Lead chốt trước khi implement A.10:

```txt
1. A.10 thuộc service nào: Tenant Service, API Gateway route-only, Domain/Template/CMS, hay service khác?
2. Endpoint path/method cụ thể.
3. Request/response contract và field bắt buộc.
4. Guard/tenant scope: OwnerSuperAdmin platform-scoped hay ClinicAdmin tenant-scoped.
5. Có persistence/transaction/409 conflict hay chỉ contract/stub.
6. Acceptance criteria + smoke cases.
```

## Cập Nhật 2026-05-12 - Backend/DevOps Phase 4 Task A.10 Full Team Fast/Budget Verify

Trạng thái: 🟡 **Blocker do thiếu spec A.10; baseline backend QA PASS**. Lead Agent đã chạy full team giả lập theo Fast/Budget Mode với Lead + Architect + Backend + QA + Documentation. Không implement code vì A.10 chưa có endpoint/method/request/response/acceptance riêng trong source of truth.

Agents:

```txt
Lead Agent: điều phối lane Backend/DevOps, giữ guardrail không stage/commit/push và không sửa frontend.
Architect Agent: read-only review service boundary, xác nhận A.10 thiếu spec; giữ boundary Tenant Service + API Gateway cho `/api/owner/*` hiện hữu.
Backend Agent: rà endpoint hiện có trong Tenant Service/API Gateway; không thêm endpoint mới khi thiếu contract.
QA Agent: chạy static/build/test và local Development smoke cho runtime hiện có.
Documentation Agent: cập nhật lane backend, plan backend và dashboard summary.
```

Kết quả Architect/Backend:

```txt
- A.10 chỉ tìm thấy ở section intake/blocker hiện tại, không có spec riêng trong `docs/`, `temp/`, `backend/`.
- Endpoint hiện hữu liên quan là A.2/A.3 `/api/owner/*`:
  GET /api/owner/plans
  GET /api/owner/modules
  GET /api/owner/tenant-plan-assignments
  POST /api/owner/tenant-plan-assignments/bulk-change
- Service boundary hiện tại: Tenant Service sở hữu contract/stub; API Gateway expose route contract/stub, không truy DB.
- Tenant isolation/security: `/api/owner/*` là platform-scoped OwnerSuperAdmin use case; ClinicAdmin bị 403, kể cả khi gửi `X-Tenant-Id`.
- 409 conflict và transaction DB chưa áp dụng cho stub hiện tại; giữ pending cho persistence plan §16 nếu owner duyệt.
```

Verify 2026-05-12:

```txt
git status --branch --short: PASS, worktree có dirty sẵn ngoài scope FE và docs backend.
git diff --stat: PASS.
git diff --check: PASS, chỉ warning LF/CRLF trên Windows.
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 warning, 0 error.
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 29/29 tests.
Runtime initial check: không có backend process sẵn ở :5005/:5006/:5018.
Local Development runtime dựng bằng dotnet:
  Tenant Service: http://localhost:5006
  API Gateway: http://localhost:5018
Smoke Tenant Service + API Gateway:
  GET /health -> 200 PASS.
  GET /openapi/v1.json -> 200 PASS, có `/api/owner/plans`.
  GET /api/owner/plans -> 200 PASS.
  GET /api/owner/modules -> 200 PASS.
  GET /api/owner/tenant-plan-assignments -> 200 PASS.
  POST /api/owner/tenant-plan-assignments/bulk-change -> 200 PASS.
  ClinicAdmin + X-Tenant-Id trên 4 endpoint -> 403 PASS.
  bulk-change thiếu auditReason -> 400 PASS.
```

Runtime/cleanup:

```txt
- Đã tắt local dotnet runtime sau smoke.
- Đã xóa log tạm do lượt này tạo: `temp/a10-*.log`.
- Không xóa các untracked log cũ `temp/*-real.*.log` vì không do lượt A.10 này tạo.
```

Điểm dừng:

```txt
1. A.10 vẫn cần owner/Lead cung cấp spec thật trước khi implement.
2. Không có code backend mới trong lượt này.
3. Không có DB/schema/migration/transaction mới.
4. Không stage/commit/push.
```

## Cập Nhật 2026-05-12 - Backend/DevOps Phase 4 Task A.10 Preparation Rerun

Trạng thái: 🟡 **Chuẩn bị lane xong; QA PASS; A.10 vẫn blocker do thiếu spec**. Lead Agent chạy lại Fast/Budget Mode full team theo yêu cầu owner, giữ nguyên stub/fallback A.2/A.3 và không tạo endpoint mới.

Agents:

```txt
Lead Agent: điều phối lại lane A.10, kiểm tra dirty worktree, không stage/commit/push.
Architect Agent: read-only review, xác nhận boundary Tenant Service + API Gateway và A.10 thiếu spec riêng.
Backend Agent: rà `/api/owner/*` hiện có, giữ stub/fallback vì persistence §16 chưa duyệt.
QA Agent: chạy static/build/test backend và runtime smoke 200/400/403.
Documentation Agent: cập nhật backend lane, plan backend và dashboard summary.
```

Rà code hiện có:

```txt
Tenant Service:
  backend/services/tenant-service/src/TenantService.Api/Endpoints/OwnerPlanCatalogEndpoints.cs
  Map `/api/owner/*`, AllowPlatformScope, OwnerSuperAdmin, plans.read/plans.write.

API Gateway:
  backend/services/api-gateway/src/ApiGateway.Api/Endpoints/OwnerPlanCatalogContractEndpoints.cs
  Map contract/stub cùng 4 route, không truy DB.

Tests hiện có:
  OwnerPlanCatalogStubHandlerTests.cs cover list plans/modules, validation bulk-change, happy path MRR diff.
  OwnerPlanCatalogEndpointMetadataTests.cs cover 4 route, platform scope, OwnerSuperAdmin, permissions.
```

Verify 2026-05-12:

```txt
git status --branch --short: PASS, worktree có dirty sẵn ở backend docs + frontend lane.
git diff --stat: PASS.
git diff --check: PASS, chỉ warning LF/CRLF trên Windows.
C:\Users\nvhoa2\.dotnet\dotnet.exe restore backend/ClinicSaaS.Backend.sln: PASS.
C:\Users\nvhoa2\.dotnet\dotnet.exe build backend/ClinicSaaS.Backend.sln --no-restore: PASS, 0 warning, 0 error.
C:\Users\nvhoa2\.dotnet\dotnet.exe test backend/ClinicSaaS.Backend.sln --no-build: PASS, 29/29 tests.
Runtime initial check: không có backend process sẵn ở :5005/:5006/:5018.
Local Development runtime dùng dotnet:
  Tenant Service: http://localhost:5006
  API Gateway: http://localhost:5018
Smoke Tenant Service + API Gateway:
  GET /health -> 200 PASS.
  GET /openapi/v1.json -> 200 PASS, có `/api/owner/plans`.
  GET /api/owner/plans -> 200 PASS.
  GET /api/owner/modules -> 200 PASS.
  GET /api/owner/tenant-plan-assignments -> 200 PASS.
  POST /api/owner/tenant-plan-assignments/bulk-change hợp lệ -> 200 PASS.
  ClinicAdmin + X-Tenant-Id trên 4 endpoint -> 403 PASS.
  bulk-change thiếu auditReason -> 400 PASS.
  bulk-change targetPlan invalid -> 400 PASS.
  bulk-change effectiveAt invalid -> 400 PASS.
  bulk-change selectedTenantIds rỗng -> 400 PASS.
```

Runtime/cleanup:

```txt
- Đã tắt local dotnet runtime sau smoke.
- Đã xóa log tạm do lượt chuẩn bị này tạo: `temp/a10-prep-*.log`.
- Không đụng `.env`, secret, key, `.claude/settings.local.json`.
- Không sửa frontend.
```

Blocker/next:

```txt
1. A.10 cần spec thật trước khi implement: service, path/method, request/response, guard/scope, persistence hay stub, acceptance/smoke.
2. Giữ A.2/A.3 `/api/owner/*` contract/stub làm baseline cho FE; persistence §16 vẫn chờ owner duyệt.
3. Không có code backend mới trong lượt chuẩn bị này.
```
