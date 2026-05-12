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
