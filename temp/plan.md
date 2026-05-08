# Kế Hoạch - Phase 2 Tenant MVP Backend

Ngày cập nhật: 2026-05-09
Trạng thái: Đã cập nhật theo quyết định kiến trúc của owner, chờ owner duyệt lại trước khi implement.
Chế độ thực hiện: Chỉ cập nhật plan/docs. Không code implementation, không commit, không tạo Figma file.

## 1. Tóm Tắt Task

Tạo vertical slice backend thật đầu tiên cho Clinic SaaS:

```txt
Owner Super Admin tạo phòng khám/tenant
↓
API Gateway expose API platform
↓
Tenant Service validate và lưu tenant
↓
PostgreSQL lưu tenant root/profile/domain/module
```

Phase này biến skeleton hiện tại thành Tenant MVP Backend tối thiểu. Trọng tâm là tenant lifecycle, PostgreSQL persistence, API Gateway forwarding và tenant isolation. Không làm billing/domain verification/template/CMS trong phase này.

## 2. Quyết Định Kiến Trúc Đã Chốt

- Không dùng EF Core.
- Tenant Service persistence dùng `Dapper` + `Npgsql`.
  - `Npgsql` là PostgreSQL .NET driver.
  - `Dapper` là micro ORM chạy trên `NpgsqlConnection`.
- Không dùng EF migrations.
- Migration dùng SQL migration files. Migration runner sẽ được quyết định sau khi owner duyệt hướng `DbUp` hoặc `FluentMigrator`.
- Runtime production-safe đề xuất là `.NET 10 LTS`, không tự nâng `.NET 11` preview.
- Nếu máy chưa có .NET 10 SDK, tạo task riêng để cài SDK và upgrade target.
- Giữ `net9.0` tạm thời nếu cần build local cho tới khi owner duyệt upgrade runtime.
- Phase 2 API Gateway dùng typed `HttpClient` forwarding.
- `YARP` là reverse proxy option để cân nhắc ở phase gateway hardening sau, không thêm ở Phase 2.
- `TenantStatus` chốt gồm:
  - `Draft`
  - `Active`
  - `Suspended`
  - `Archived`
- `TenantModule` dùng string `module_code` để bật/tắt module theo tenant.
- Chưa implement billing/plan logic thật.
- Thêm `ModuleCodes` constants nếu phù hợp với contracts/domain trong lúc implement.

## 3. Hiện Trạng Code Sau Khi Kiểm Tra

- Các project backend trong scope hiện đang target `net9.0`.
- Chưa có package `EF Core`, `Dapper`, `Npgsql`, `YARP` trong scope `tenant-service`/`api-gateway`.
- `TenantService.Infrastructure` hiện chỉ bind `PostgreSqlOptions`, chưa có persistence thật.
- `ApiGateway.Infrastructure` hiện chưa có typed `HttpClient` tới Tenant Service.
- `infrastructure/postgres/init.sql` đang là bootstrap placeholder, cần cập nhật theo schema Phase 2 khi implement.
- `backend/shared/contracts/Authorization/PermissionCodes.cs` đã có `tenants.read` và `tenants.write`.

## 4. Phase Hiện Tại Trong Roadmap

Phase hiện tại:

```txt
Phase 2 - Tenant MVP Backend
```

Trạng thái:

```txt
Plan đã cập nhật theo quyết định owner, chờ owner duyệt lại trước khi implement.
```

Phase trước:

```txt
Phase 1.3 - API Boundary Standardization: Done và đã verify.
```

Phase kế tiếp sau khi Phase 2 pass:

```txt
Phase 3 - Owner Admin Tenant Slice
```

## 5. Scope Sau Khi Owner Duyệt

Được sửa:

```txt
backend/services/tenant-service
backend/services/api-gateway
backend/shared/contracts
infrastructure/postgres
docs/current-task.md
docs/roadmap/clinic-saas-roadmap.md
temp/plan.md
```

Khu vực code dự kiến:

```txt
backend/services/tenant-service/src/TenantService.Domain/Tenants/*
backend/services/tenant-service/src/TenantService.Application/Tenants/*
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/*
backend/services/tenant-service/src/TenantService.Api/Endpoints/*
backend/services/api-gateway/src/ApiGateway.Application/Tenants/*
backend/services/api-gateway/src/ApiGateway.Infrastructure/Tenants/*
backend/services/api-gateway/src/ApiGateway.Api/Endpoints/*
backend/shared/contracts/Tenancy/*
backend/shared/contracts/Events/*
backend/services/tenant-service/tests/*
backend/services/api-gateway/tests/*
infrastructure/postgres/*
```

## 6. Out Of Scope

- Không code implementation trước khi owner duyệt lại plan.
- Không dùng EF Core.
- Không dùng EF migrations.
- Không upgrade runtime lên `.NET 10` hoặc `.NET 11` trong Phase 2 nếu chưa có task riêng được duyệt.
- Không thêm YARP ở Phase 2.
- Không sửa frontend.
- Không làm Owner Admin UI hoặc Clinic Admin UI.
- Không real login/JWT provider.
- Không real RBAC enforcement ngoài metadata placeholder Phase 1.3.
- Không billing/subscription logic thật.
- Không domain DNS verification, SSL, publish workflow thật.
- Không template apply, Website CMS, Booking, Catalog, Customer, Report, Notification hoặc Realtime implementation.
- Không Kafka/Event Bus publishing.
- Không production connection string, IP server thật, token hoặc secret.
- Không tạo Figma file.
- Không commit.

## 7. Chia Việc Theo Agent Checklist

Architect Agent:

- Xác nhận Tenant Service sở hữu tenant lifecycle và persistence.
- Xác nhận API Gateway không truy cập database tenant.
- Xác nhận Owner Super Admin endpoint là platform-scoped và có thể thao tác cross-tenant.
- Xác nhận dữ liệu con theo tenant có `tenant_id` và index phù hợp.

Backend Agent:

- Tạo domain model Tenant tối thiểu.
- Tạo use case application rõ transaction boundary.
- Dùng Dapper + Npgsql trong Infrastructure.
- Tạo endpoint trong Tenant Service.
- Tạo typed `HttpClient` forwarding trong API Gateway.
- Giữ middleware, OpenAPI, correlation id, tenant scope metadata và RBAC placeholder hiện có.

Database Agent:

- Thiết kế SQL schema cho `platform.tenants`, `platform.tenant_profiles`, `platform.tenant_domains`, `platform.tenant_modules`.
- Tạo SQL migration files nếu owner duyệt implement.
- Cập nhật `infrastructure/postgres/init.sql` để mirror schema local bootstrap.
- Chưa implement migration runner nếu chưa cần.

DevOps Agent:

- Giữ config PostgreSQL qua env/example.
- Không thêm secret thật.
- Ghi rõ env var local cho Tenant Service và gateway base URL.

QA Agent:

- Verify build/test.
- Smoke create/list/get/update status qua gateway.
- Test duplicate slug/domain trả `409`.
- Test tenant child table đều có `tenant_id`.

Documentation Agent:

- Cập nhật `docs/current-task.md`.
- Cập nhật `docs/roadmap/clinic-saas-roadmap.md`.
- Ghi rõ verify command và gap còn lại sau implementation.

Frontend Agent và Figma UI Agent:

- Không tham gia Phase 2.

## 8. Domain Model Tối Thiểu

Model:

```txt
Tenant
ClinicProfile
TenantStatus
TenantDomain
TenantModule
PlanReference
```

`Tenant`:

- `Id`
- `Slug`
- `DisplayName`
- `Status`
- `Plan`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `ActivatedAtUtc`
- `SuspendedAtUtc`
- `ArchivedAtUtc`

`ClinicProfile`:

- `TenantId`
- `ClinicName`
- `ContactEmail`
- `PhoneNumber`
- `AddressLine`
- `Specialty`

`TenantDomain`:

- `Id`
- `TenantId`
- `DomainName`
- `NormalizedDomainName`
- `DomainType` (`DefaultSubdomain`, `Custom`)
- `Status` placeholder (`Pending`, `Active`, `Suspended`)
- `IsPrimary`
- `CreatedAtUtc`
- `VerifiedAtUtc`

`TenantModule`:

- `TenantId`
- `ModuleCode`
- `IsEnabled`
- `SourcePlanCode`
- `CreatedAtUtc`
- `UpdatedAtUtc`

`PlanReference`:

- `PlanCode`
- `DisplayName`

`TenantStatus` chốt:

```txt
Draft
Active
Suspended
Archived
```

`ModuleCodes` đề xuất nếu cần:

```txt
website
booking
catalog
customers
billing
reports
notifications
```

Không implement plan/billing rule thật trong Phase 2; `module_code` chỉ là cấu hình bật/tắt module theo tenant.

## 9. PostgreSQL Schema Plan

Schema:

```txt
platform
```

Tables:

```txt
platform.tenants
platform.tenant_profiles
platform.tenant_domains
platform.tenant_modules
```

Ràng buộc/index bắt buộc:

- `platform.tenants.id` primary key.
- `platform.tenants.slug` unique sau normalize.
- `platform.tenants.status` check trong `Draft`, `Active`, `Suspended`, `Archived`.
- `platform.tenant_profiles.tenant_id` primary key và foreign key tới `platform.tenants(id)`.
- `platform.tenant_domains.id` primary key.
- `platform.tenant_domains.tenant_id` indexed.
- `platform.tenant_domains.normalized_domain_name` unique.
- `platform.tenant_domains (tenant_id, is_primary)` indexed.
- `platform.tenant_modules (tenant_id, module_code)` primary key.
- `platform.tenant_modules.tenant_id` indexed.

Lưu ý tenant isolation:

- `platform.tenants` là tenant root/platform-owned.
- Các bảng con của tenant phải có `tenant_id`.
- Clinic Admin sau này chỉ được query theo tenant context; Owner Super Admin mới được list cross-tenant.

## 10. Kế Hoạch Persistence: Dapper + Npgsql

Không dùng:

```txt
EF Core
DbContext
EF migrations
```

Dùng:

```txt
NpgsqlConnection
Dapper query/execute
SQL rõ ràng trong repository/query store
Transaction boundary bằng NpgsqlTransaction
```

Hướng implement sau khi duyệt:

- `TenantService.Infrastructure` thêm package `Npgsql` và `Dapper`.
- Tạo connection factory đọc `PostgreSqlOptions`.
- Tạo repository/query service dùng SQL parameterized.
- `CreateTenant` chạy trong transaction gồm insert `tenants`, `tenant_profiles`, `tenant_domains`, `tenant_modules`.
- Map lỗi unique violation PostgreSQL sang conflict domain/application result.
- Không hard-code connection string thật.

## 11. Kế Hoạch Migration: SQL Files Trước, Runner Sau

Migration không dùng EF.

Đề xuất cấu trúc SQL files:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/
  0001_create_tenant_mvp.sql
```

Khuyến nghị:

- Phase 2 có thể tạo SQL migration file và cập nhật `infrastructure/postgres/init.sql`.
- Chưa cần implement migration runner nếu scope Phase 2 chỉ cần local bootstrap + manual apply.
- Nếu cần runner sớm, ưu tiên `DbUp`.

So sánh `DbUp` vs `FluentMigrator`:

- `DbUp`: hợp với hướng SQL-first, đơn giản, dễ đọc, ít abstraction, tốt cho service nhỏ và migration file thuần SQL.
- `FluentMigrator`: mạnh hơn nếu muốn migration bằng C# fluent API, nhiều convention hơn, nhưng thêm abstraction không cần thiết ở MVP.

Khuyến nghị cho project này:

```txt
Chọn DbUp khi cần migration runner, vì owner đã chốt SQL migration files và Dapper/Npgsql.
Chưa implement runner trong bước đầu nếu chưa cần; chỉ ghi rõ đường dẫn migration và cách apply thủ công/local.
```

## 12. Kế Hoạch Use Case

Use cases:

```txt
CreateTenant
GetTenantById
ListTenants
UpdateTenantStatus
```

`CreateTenant`:

- Validate clinic display name.
- Validate slug/default subdomain format.
- Normalize slug/domain.
- Validate `plan_code` có giá trị.
- Create `Tenant`.
- Create `ClinicProfile`.
- Create default `TenantDomain`.
- Create `TenantModule` records từ request hoặc default module set.
- Enforce unique slug/domain.
- Persist trong transaction.
- Return tenant response.

`GetTenantById`:

- Owner Super Admin đọc tenant bất kỳ theo id.
- Trả `404` nếu không có.

`ListTenants`:

- Owner Super Admin list tenant cross-tenant.
- Filter tối thiểu: status, search text.
- Pagination đơn giản với default limit bảo thủ.

`UpdateTenantStatus`:

- Owner Super Admin update status.
- Validate status thuộc `Draft`, `Active`, `Suspended`, `Archived`.
- Không trigger billing/domain side effect trong Phase 2.

## 13. Kế Hoạch API

Tenant Service endpoints:

```txt
POST /api/tenants
GET /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
```

API Gateway endpoints:

```txt
POST /api/tenants
GET /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
```

Endpoint metadata:

- Platform-scoped: `AllowPlatformScope()`.
- Role metadata: `RequireRole(RoleNames.OwnerSuperAdmin)`.
- Permission metadata:
  - read endpoints: `PermissionCodes.TenantsRead`
  - write endpoints: `PermissionCodes.TenantsWrite`

HTTP behavior:

- `201 Created` cho `POST /api/tenants`.
- `200 OK` cho get/list/update success.
- `400 Bad Request` cho invalid input.
- `404 Not Found` cho tenant không tồn tại.
- `409 Conflict` cho duplicate slug/domain.
- Error response nên tương thích `ProblemDetails` khi thực tế phù hợp.

## 14. Kế Hoạch API Gateway

Phase 2 dùng typed `HttpClient` forwarding:

- Register Tenant Service base URL qua config/env, không hard-code production.
- Dùng shared contracts trong `ClinicSaaS.Contracts`.
- Propagate correlation id.
- Không truy cập database từ API Gateway.
- Tenant management endpoints là platform-scoped.

Config đề xuất:

```json
{
  "Services": {
    "TenantService": {
      "BaseUrl": "http://localhost:5003"
    }
  }
}
```

`YARP`:

- Không dùng trong Phase 2.
- Ghi nhận là option cho phase gateway hardening sau nếu cần reverse proxy, route table, transform, resilience policy chuẩn hơn.

## 15. Kế Hoạch Runtime Target

Quyết định:

- Không tự nâng `.NET 11` preview.
- Production-safe target đề xuất: `.NET 10 LTS`.
- Code hiện tại đang `net9.0`.
- Nếu local chưa có .NET 10 SDK, tạo task riêng:
  - cài .NET 10 SDK,
  - nâng `TargetFramework`,
  - verify toàn bộ backend,
  - cập nhật docs.
- Phase 2 có thể giữ `net9.0` tạm thời để build local nếu owner chưa duyệt runtime upgrade.

## 16. Tiêu Chí Done

Phase 2 chỉ Done khi:

- Tenant domain model có trong Domain layer.
- Application use cases có create/get/list/update status.
- Infrastructure dùng Dapper + Npgsql, không EF Core.
- Persistence có connection factory, repository/query service và transaction boundary rõ.
- SQL schema/migration file có đủ bảng, FK, unique constraint, `tenant_id` cho bảng con và index.
- `infrastructure/postgres/init.sql` được đồng bộ schema local bootstrap.
- Tenant Service expose endpoints Phase 2.
- API Gateway expose matching endpoints qua typed `HttpClient` forwarding.
- Shared contracts có request/response DTO cần thiết.
- Build/test backend pass.
- Smoke test tạo/list/get/update status tenant qua gateway pass khi local PostgreSQL được cấu hình.
- Duplicate slug/domain trả `409`.
- `docs/current-task.md` và `docs/roadmap/clinic-saas-roadmap.md` được cập nhật sau implementation/verification.

## 17. Kế Hoạch Verify

Static/build:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build
```

Infrastructure:

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
```

SQL migration/manual check sau implementation:

```txt
1. Kiểm tra SQL migration file trong Tenant Service.
2. Kiểm tra `infrastructure/postgres/init.sql` mirror schema.
3. Apply SQL vào local PostgreSQL nếu smoke test cần database thật.
```

Runtime smoke sau khi owner duyệt và implement:

```txt
1. Start local PostgreSQL với env/example config.
2. Run tenant-service.
3. Run api-gateway.
4. POST /api/tenants qua api-gateway.
5. GET /api/tenants qua api-gateway.
6. GET /api/tenants/{id} qua api-gateway.
7. PATCH /api/tenants/{id}/status qua api-gateway.
8. Tạo trùng slug/domain và verify 409.
```

Secret/scope:

```powershell
git diff | Select-String -Pattern "sk-|api_key|password|connectionString|ConnectionStrings|secret|token"
git diff --cached | Select-String -Pattern "sk-|api_key|password|connectionString|ConnectionStrings|secret|token"
git diff --name-only
```

## 18. Rủi Ro Và Điểm Owner Cần Duyệt

Owner đã chốt:

- Không EF Core.
- Dapper + Npgsql cho Tenant Service persistence.
- SQL migration files, không EF migrations.
- Typed `HttpClient` forwarding cho API Gateway Phase 2.
- `TenantStatus`: `Draft`, `Active`, `Suspended`, `Archived`.
- `TenantModule.module_code` là string.

Cần owner duyệt lại trước khi implement:

- Chấp nhận giữ `net9.0` tạm thời cho Phase 2 nếu chưa có task upgrade `.NET 10 LTS`.
- Chấp nhận chưa implement migration runner ngay; chỉ tạo SQL migration file/init SQL trước.
- Nếu cần runner sớm, chấp nhận hướng ưu tiên `DbUp`.
- Chấp nhận `ModuleCodes` constants ở contracts/domain nếu implementation thấy cần.

Rủi ro:

- Worktree hiện có nhiều thay đổi backend chưa commit từ phase trước; implementation phải giữ nguyên và không revert.
- Smoke test database phụ thuộc Docker/env local.
- Auth vẫn là placeholder nên Phase 2 chỉ verify metadata/behavior, chưa verify real authorization.
- SQL-first migration cần discipline đặt version file rõ để tránh lệch schema khi nhiều service lớn lên.

## 19. Điểm Dừng

Dừng sau khi cập nhật plan. Không implement cho tới khi owner duyệt lại plan.

Câu duyệt hợp lệ:

- "Tôi duyệt plan"
- "Duyệt, làm tiếp"
- "Bắt đầu implement"
- "Quất theo plan"

---

# Ghi Chú Lịch Sử Từ Các Task Trước

# Plan - Clinic SaaS Current Task

Ngay: 2026-05-09
Trang thai: Da dong bo tai lieu/rules/prompt theo structure hien tai
Pham vi: Docs/rules/prompt/FigJam note, khong implement business logic sau

## 1. Structure Da Scaffold

```txt
frontend/
  apps/
    public-web/
    clinic-admin/
    owner-admin/
  packages/
    ui/
    design-tokens/
    api-client/
    shared-types/
    config/

backend/
  services/
    api-gateway/
    identity-service/
    tenant-service/
    website-cms-service/
    template-service/
    domain-service/
    booking-service/
    catalog-service/
    customer-service/
    billing-service/
    report-service/
    notification-service/
    realtime-gateway/
  shared/
    building-blocks/
    contracts/
    observability/

infrastructure/
  docker/
  postgres/
  mongodb/
  redis/
  kafka/
  scripts/

docs/
  architecture/
  api/
  setup/
```

## 2. Nguyen Tac Da Giu

- Khong dung root-level `apps/`, `packages/`, `services/`.
- Khong implement business logic sau.
- Khong noi database that.
- Khong code full UI chi tiet.
- Khong tao Figma file moi.
- Khong commit.
- Moi placeholder lien quan tenant-owned data deu ghi ro tenant context/`tenant_id`.

## 3. Frontend Skeleton

- `frontend/package.json`: npm workspace cho frontend.
- `frontend/apps/public-web`: tenant-aware public app shell.
- `frontend/apps/clinic-admin`: tenant-scoped admin shell.
- `frontend/apps/owner-admin`: platform-scoped owner shell.
- `frontend/packages/ui`: `AppButton`, `StatusPill` placeholder.
- `frontend/packages/design-tokens`: token constants placeholder.
- `frontend/packages/api-client`: tenant context guard placeholder.
- `frontend/packages/shared-types`: shared tenant mock/type placeholder.
- `frontend/packages/config`: shared frontend config placeholder.

## 4. Backend Skeleton

- `backend/services/*`: moi service co README, `src/*Api`, `src/*Application`, `src/*Domain`, `src/*Infrastructure`, `tests`.
- `backend/shared/building-blocks`: shared backend primitive placeholder.
- `backend/shared/contracts`: shared DTO/event contract placeholder.
- `backend/shared/observability`: logging/tracing/metrics convention placeholder.

## 5. Infrastructure Skeleton

- `infrastructure/docker/docker-compose.dev.yml`: local placeholder cho PostgreSQL, MongoDB, Redis, Kafka.
- `infrastructure/docker/docker-compose.prod.yml`: production compose placeholder.
- `infrastructure/docker/nginx/default.conf`: reverse proxy placeholder.
- `infrastructure/postgres/init.sql`: schema/table placeholder co `tenant_id` index.
- `infrastructure/mongodb/init.js`: collection/index placeholder co `tenant_id`.
- `infrastructure/redis/redis.conf`: Redis local config.
- `infrastructure/kafka/topics.dev.json`: topic placeholder.
- `infrastructure/kafka/create-topics.sh`: topic creation placeholder.
- `infrastructure/scripts/*.ps1`: local dev scripts placeholder.

## 6. Docs Skeleton

- `docs/architecture/overview.md`
- `docs/architecture/service-boundaries.md`
- `docs/architecture/tenant-isolation.md`
- `docs/api/README.md`
- `docs/setup/local-development.md`
- `docs/setup/troubleshooting.md`

## 7. Verify Da Chay

Frontend:

```powershell
cd frontend
npm install
npm run typecheck
npm run build
```

Infrastructure:

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
docker compose -f infrastructure/docker/docker-compose.prod.yml config
```

File scaffold:

```powershell
rg --files frontend backend infrastructure docs
```

Ket qua:

- `rg --files frontend backend infrastructure docs`: pass.
- `docker compose -f infrastructure/docker/docker-compose.dev.yml config`: pass.
- `docker compose -f infrastructure/docker/docker-compose.prod.yml config`: pass.
- `cd frontend; npm install`: pass, 0 vulnerabilities theo npm audit summary.
- `cd frontend; npm run typecheck`: pass cho `clinic-admin`, `owner-admin`, `public-web`.
- `cd frontend; npm run build`: pass cho `clinic-admin`, `owner-admin`, `public-web`.
- Da don `frontend/node_modules` va `dist` generated sau verify; giu `frontend/package-lock.json`.

## 8. Ghi Chu Worktree

- `temp/plan.md` co the bi git ignore theo cau hinh hien tai nen can kiem tra rieng neu can track.
- Cac file modified co san tu truoc nhu `AGENTS.md`, `CLAUDE.md`, `architech.txt`, `clinic_saas_report.md`, `docs/codex-setup.md` khong duoc sua trong buoc scaffold nay.

## 9. Docs Sync Da Hoan Thanh

Muc tieu:

- Doi cac reference root-level `apps/`, `packages/`, `services/` sang `frontend/apps/`, `frontend/packages/`, `backend/services/`.
- Doi legacy report path, legacy lead-agent file reference va link FigJam cu sang source hien tai.
- Giu nguyen Clinic SaaS Multi Tenant va tech stack da chot.
- Khong sua code skeleton, khong tao Figma file moi, khong implement business logic, khong commit.

Figma/FigJam:

- Da search 2 FigJam board va UI design file bang MCP; khong thay text node con chua structure cu.
- Da them note `Current Repo Structure - Clinic SaaS` vao Technical Architecture FigJam de dong nhat structure hien tai.

Verify:

- Legacy report/playbook/link cu: khong con match.
- Structure scan: chi con match trong tree hop le, full path hien tai, route public `/services/:slug`, hoac rule cam root-level structure.
- Khong chay build/typecheck vi task chi sua docs/rules/prompt.

## 10. Buoc Tiep Theo De Xuat

1. Tao .NET project files thuc te cho phase 1: `api-gateway`, `identity-service`, `tenant-service`.
2. Mo rong frontend routing/layout placeholder theo Figma cho 3 apps.
3. Them test/checklist tenant isolation va routing smoke test.

## 11. Backend Phase 1 Plan

Ngay: 2026-05-09
Trang thai: Da hoan thanh backend phase 1 skeleton
Pham vi: chi tao .NET solution/project skeleton that cho:

- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`

Success criteria:

- Moi service co solution file rieng va cac project:
  - `src/<Service>.Api`
  - `src/<Service>.Application`
  - `src/<Service>.Domain`
  - `src/<Service>.Infrastructure`
  - `tests/<Service>.Tests`
- Project references dung Clean Architecture:
  - Api -> Application, Infrastructure
  - Infrastructure -> Application
  - Application -> Domain
  - Tests -> Domain/Application
- Api co placeholder cho tenant context middleware, auth/RBAC, OpenAPI, health check.
- Infrastructure co PostgreSQL config placeholder, khong tao connection that.
- Khong secret, khong Figma file moi, khong commit, khong sua frontend routing/layout.

Runtime note:

- Tai lieu cu co nhac `.NET 11` la dinh huong ban dau, nhung local x64 SDK hien co la .NET SDK 9.0.304.
- De restore/build duoc tren may hien tai, skeleton phase 1 dung `net9.0`.
- Khi owner cai SDK .NET LTS moi hon, co the nang `TargetFramework` sau bang mot task rieng.

Verify du kien:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/services/api-gateway/ApiGateway.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/services/api-gateway/ApiGateway.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/services/identity-service/IdentityService.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/services/identity-service/IdentityService.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/services/tenant-service/TenantService.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/services/tenant-service/TenantService.sln --no-restore
```

Verify da chay them:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Ket qua:

- Restore/build pass cho 3 service solutions.
- Restore/build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build root solution: 0 warnings, 0 errors.

## 12. Shared Backend Primitives Plan

Ngay: 2026-05-09
Trang thai: Owner yeu cau tao shared backend primitives va ap dung toi thieu vao phase 1 services
Pham vi:

- `backend/shared/building-blocks`
- `backend/shared/contracts`
- `backend/shared/observability`
- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`

Success criteria:

- Tao project `ClinicSaaS.BuildingBlocks`, `ClinicSaaS.Contracts`, `ClinicSaaS.Observability`, target `net9.0`.
- BuildingBlocks co placeholder:
  - tenant context/accessor
  - tenant resolution result
  - Result/Error model
  - guard/validation helper
  - options pattern placeholder
- Contracts co placeholder:
  - TenantReference DTO
  - UserContext DTO
  - role/permission constants
  - domain event base
  - TenantCreated event
  - auth/RBAC contract
- Observability co placeholder:
  - logging constants
  - correlation id middleware
  - trace context
  - health check tag constants
- 3 phase 1 services reference shared projects where hop ly.
- Khong implement business logic sau, khong connect PostgreSQL that, khong secret, khong frontend, khong Figma, khong commit.

Verify du kien:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Verify da chay:

- Restore pass cho `backend/ClinicSaaS.Backend.sln`.
- Build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build result: 0 warnings, 0 errors.
- Runtime smoke `api-gateway`:
  - `/health` tra 200.
  - `/api/_system/tenant-context` khong tenant tra 400.
  - `/api/_system/tenant-context` voi `X-Tenant-Id: tenant-smoke` tra tenant context dung.
  - `/openapi/v1.json` tra 200.

Ket qua implementation:

- Tao `ClinicSaaS.BuildingBlocks`, `ClinicSaaS.Contracts`, `ClinicSaaS.Observability`.
- 3 service phase 1 da reference shared projects:
  - Api -> BuildingBlocks, Contracts, Observability
  - Application -> BuildingBlocks, Contracts
  - Infrastructure -> BuildingBlocks
  - Tests -> BuildingBlocks
- Local duplicate tenant context/RBAC/PostgreSQL options placeholders trong 3 services da thay bang shared types.

## 13. Backend Phase 1.2 Plan

Ngay: 2026-05-09
Trang thai: Da hoan thanh
Pham vi:

- `backend/shared/building-blocks`
- `backend/shared/contracts`
- `backend/shared/observability`
- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`

Success criteria:

- Consolidate tenant middleware dung chung neu hop ly, van giu service wrapper neu can de tranh xoa file cu.
- Tenant context resolve tu `X-Tenant-Id` va placeholder JWT claim `tenant_id`.
- Endpoint metadata phan biet platform-scoped va tenant-scoped.
- Tenant-scoped endpoint fail ro rang neu thieu tenant context.
- Co placeholder `RequireRole` va `RequirePermission` o API boundary.
- Co OpenAPI config that neu build duoc voi .NET 9 hien tai.
- Chuan hoa system endpoints:
  - `/health`
  - `/api/_system/tenant-context`
  - `/api/_system/auth-rbac-placeholder`
  - `/api/_system/openapi-placeholder`

Ngoai scope:

- Khong connect PostgreSQL that.
- Khong tao migration.
- Khong implement tenant CRUD.
- Khong tao domain/database model that.
- Khong sua frontend.
- Khong tao Figma file moi.
- Khong commit.

Verify du kien:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Verify da chay:

- Restore pass cho `backend/ClinicSaaS.Backend.sln`.
- Build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build result: 0 warnings, 0 errors.

Ket qua implementation:

- Shared `TenantContextMiddleware` resolve tenant tu `X-Tenant-Id`, principal claim `tenant_id`, va Bearer JWT payload placeholder chua validate.
- Shared endpoint metadata co `AllowPlatformScope` va `RequireTenantContext`.
- Tenant-scoped endpoint fail 400 neu thieu tenant context.
- Shared RBAC placeholder co `RequireRole`, `RequirePermission`, va middleware metadata-only.
- 3 phase 1 services dung shared middleware/system endpoint wrapper.
- OpenAPI JSON duoc map tai `/openapi/v1.json`.

## 14. Backend Phase 1.3 Plan

Ngay: 2026-05-09
Trang thai: Done
Pham vi:

- `backend/shared/building-blocks`
- `backend/shared/contracts`
- `backend/shared/observability`
- `backend/services/api-gateway`
- `backend/services/identity-service`
- `backend/services/tenant-service`
- `docs/current-task.md`
- `docs/roadmap/clinic-saas-roadmap.md`

Success criteria:

- Tenant middleware dung chung resolve tenant tu `X-Tenant-Id` va placeholder JWT claim `tenant_id`.
- Platform-scoped endpoints khong can tenant context.
- Tenant-scoped endpoints fail ro rang neu thieu tenant context.
- Auth/RBAC placeholder co `RequireRole`, `RequirePermission`, va `UserContext` placeholder ro hon.
- 3 Api projects co OpenAPI/Swagger config that neu package build duoc.
- System endpoints chuan hoa tren ca 3 service.
- Cap nhat `docs/current-task.md` va `docs/roadmap/clinic-saas-roadmap.md` sang Done sau verify pass.

Verify du kien:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Verify da chay:

- Restore pass cho `backend/ClinicSaaS.Backend.sln`.
- Build pass cho `backend/ClinicSaaS.Backend.sln`.
- Build result: 0 warnings, 0 errors.
- Runtime smoke `api-gateway`:
  - `/health` tra 200.
  - `/api/_system/tenant-context` khong tenant tra 400.
  - `/api/_system/tenant-context` voi `X-Tenant-Id: tenant-smoke` tra tenant context dung.
  - `/api/_system/auth-rbac-placeholder` tra metadata-only placeholder mode.
  - `/openapi/v1.json` tra 200.
  - `/swagger` tra 200.

Ket qua implementation:

- Shared tenant middleware duoc giu trong `ClinicSaaS.BuildingBlocks`.
- RBAC placeholder co `RequireRole`, `RequirePermission`, `UserContext`, `IUserContextAccessor`.
- Swashbuckle OpenAPI/Swagger config duoc gom vao shared extension.
- 3 Api projects dung cung API boundary pattern.
- `docs/current-task.md` va `docs/roadmap/clinic-saas-roadmap.md` da cap nhat Phase 1.3 Done, Phase 2 Next.
