# Clinic SaaS Multi Tenant - Roadmap & Status Tracker

> Cập nhật hiện hành 2026-05-10: project đang chạy 2 workstream song song. Backend/DevOps đã hoàn tất Phase 2 API Runtime Smoke Gate, 5 smoke case PASS đủ trên server `116.118.47.78` sau 2 vòng fix (Dapper type handler + reorder positional record `TenantListRow`); Phase 2 chuyển Done. Frontend Phase 3 Owner Admin Tenant Slice **Implementation Done — chờ owner duyệt commit** (commit `7f6366d`). Pre-Phase 4 Hardening đã apply (P1.1/1.2/1.3/1.4/1.5/1.6/1.7/1.8). UI Redesign V3 (page Figma `65:2` 76 frame, 7 section grouping) đã chốt làm **source of truth Phase 4+** (xem section 7.1). Dashboard nằm ở `docs/current-task.md`; chi tiết lane nằm ở `docs/current-task.backend.md`, `docs/current-task.frontend.md`, `temp/plan.backend.md`, `temp/plan.frontend.md`.

File này dùng làm **source of truth cho lộ trình dài hạn + trạng thái hiện tại** của project.
Mục tiêu là sang session khác, tài khoản khác, hoặc sau nhiều ngày quay lại vẫn biết đang ở đâu và làm tiếp bước nào.

---

## 0. Lưu file này ở đâu?

Nên lưu trong repo tại:

```txt
docs/roadmap/clinic-saas-roadmap.md
```

Nếu chưa có folder thì tạo:

```txt
docs/roadmap/
```

## 0.1 Vai trò từng loại tài liệu

```txt
docs/roadmap/clinic-saas-roadmap.md
  Lưu roadmap dài hạn, phase, trạng thái đã làm/chưa làm, thứ tự tiếp theo.

docs/current-task.md
  Project Coordination Dashboard. Chỉ ghi tổng quan ngắn và trỏ sang lane files.

docs/current-task.backend.md
  Handoff riêng cho Backend/DevOps workstream.

docs/current-task.frontend.md
  Handoff riêng cho Frontend workstream.

temp/plan.md
  Index tương thích cũ cho plan, không chứa plan chi tiết của một lane.

temp/plan.backend.md
  Plan chi tiết Backend/DevOps workstream.

temp/plan.frontend.md
  Plan chi tiết Frontend workstream.

docs/architecture/
  Lưu kiến trúc, service boundaries, tenant isolation, quyết định backend/frontend.

docs/decisions/
  Lưu quyết định kỹ thuật quan trọng nếu cần, ví dụ ADR về monorepo, database, auth.

Figma / FigJam
  Dùng để nhìn flow, UI, architecture diagram. Không nên là nơi duy nhất lưu roadmap chi tiết.
```

## 0.2 Quy tắc cập nhật

Sau mỗi phase/task lớn, bắt buộc cập nhật status trong roadmap:

```txt
✅ Done        Nếu đã hoàn thành và verify pass.
🟡 In Progress Nếu đang làm.
🔜 Next        Nếu là bước tiếp theo.
❌ Blocked     Nếu bị chặn.
```

Đồng thời phải cập nhật ít nhất:

```txt
1. docs/current-task.md
2. docs/roadmap/clinic-saas-roadmap.md
```

Khi task thuộc lane cụ thể, cập nhật thêm file lane tương ứng:

```txt
Backend/DevOps:
  docs/current-task.backend.md
  temp/plan.backend.md

Frontend:
  docs/current-task.frontend.md
  temp/plan.frontend.md
```

Nếu task ảnh hưởng architecture thì cập nhật thêm:

```txt
docs/architecture/*
FigJam Architecture nếu cần
```

Nếu task ảnh hưởng UI thì cập nhật:

```txt
Figma UI hoặc ghi rõ Figma chưa update
```

---

# 1. Project Summary

Project là **Clinic SaaS Multi Tenant**, không phải website phòng khám đơn lẻ.

Mục tiêu:

```txt
Owner Super Admin
  Quản lý nhiều phòng khám/tenant, gói, domain, template, billing, monitoring.

Clinic Admin
  Admin riêng từng phòng khám, quản lý website settings, slider, dịch vụ, bác sĩ, lịch hẹn, bệnh nhân, thanh toán, báo cáo.

Public Website
  Website public render động theo tenant: domain, logo, theme, slider, services, staff, booking.
```

---

# 2. Source of Truth

## 2.1 UI Figma

```txt
https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC
```

## 2.2 Architecture FigJam

```txt
https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT
```

## 2.3 Repo docs

```txt
clinic_saas_khoi_tao_project_report_v2.md
codex_prompts_clinic_saas_v2.md
clinic_saas_report.md
AGENTS.md
CLAUDE.md
architech.txt
docs/current-task.md
docs/current-task.backend.md
docs/current-task.frontend.md
temp/plan.md
temp/plan.backend.md
temp/plan.frontend.md
docs/architecture/
docs/roadmap/clinic-saas-roadmap.md
```

---

# 3. Current Status

## 3.1 Current Phase

```txt
Parallel workstreams:
- Backend/DevOps: Phase 2 API Runtime Smoke Gate ✅ Done 2026-05-10 (5 smoke case PASS đủ trên server). Pre-Phase 4 Hardening (P1.1, P1.2, P1.3) committed.
- Frontend: Phase 3 Owner Admin Tenant Slice 🟡 Implementation Done — chờ owner duyệt commit (commit 7f6366d). Pre-Phase 4 Hardening (P1.6, P1.7) committed. V3v2 76 frame ready cho Phase 4 Wave A.
- Database: Pre-Phase 4 Hardening (P1.5 init.sql header drift warning) committed.
- DevOps: Pre-Phase 4 Hardening (P1.4 nginx SPA fallback, P1.8 docker-compose dev bind) committed.
- Docs: V3v2 source of truth Phase 4+ ready (76 frame, 7 section grouping, page Figma 65:2). 5 Wave plan đã chốt.
```

## 3.2 Current Workstreams

Backend/DevOps lane:

```txt
Task: Phase 2 API Runtime Smoke Gate.
Task file: docs/current-task.backend.md.
Plan file: temp/plan.backend.md.
Trạng thái: PostgreSQL server bootstrap pass, schema Phase 2 apply pass, API smoke qua tenant-service/api-gateway chưa pass.
Done guard: Không chuyển Phase 2 Done nếu POST/GET/PATCH/duplicate 409 smoke chưa pass.
```

Frontend lane:

```txt
Task: Phase 3 Owner Admin Tenant Slice.
Task file: docs/current-task.frontend.md.
Plan file: temp/plan.frontend.md.
Trạng thái: UI Redesign V2 ready, plan frontend đã lập, chờ owner duyệt implement.
Done guard: Không code frontend nếu owner chưa duyệt plan rõ.
```

Trạng thái liên quan:

```txt
- Phase 1.3 API Boundary Standardization: Done và verified.
- Phase 2 implementation đã commit local: 71923f8 feat: implement tenant mvp backend slice.
- Phase 2 static/build/config verify pass sau khi gom toàn bộ worktree.
- Phase 2 full CRUD smoke local vẫn blocked vì Docker daemon/PostgreSQL local chưa chạy.
- Server riêng đã bootstrap Docker/PostgreSQL smoke pass.
- Phase 2 chưa Done vì chưa chạy API smoke qua tenant-service/api-gateway.
- Phase 2 chưa đánh dấu Done cho tới khi smoke DB create/list/get/update/duplicate conflict pass.
- UI Redesign V2 đã cập nhật trực tiếp trong Figma source of truth, page `UI Redesign V2 - 2026-05-09`, không tạo Figma file mới.
- UI Redesign V2 đã polish tiếp trong cùng file Figma: fixed typography/text clipping, homepage slider, booking states, builder panels, tenant operations table/detail/conflict/wizard preview và design token handoff.
```

Scope:

```txt
temp/plan.md là index.
docs/current-task.md là dashboard.
temp/plan.backend.md
temp/plan.frontend.md
docs/current-task.backend.md
docs/current-task.frontend.md
docs/roadmap/clinic-saas-roadmap.md

Implementation scope đã commit:
backend/services/tenant-service
backend/services/api-gateway
backend/shared/contracts
infrastructure/postgres
```

Scope guard hiện tại:

```txt
- Không implement thêm feature mới.
- Backend/DevOps không sửa frontend.
- Frontend không sửa backend.
- Không real auth/JWT.
- Không billing/domain/template/CMS/booking.
- Không tạo Figma file mới.
- Không push khi owner chưa yêu cầu.
- Không ghi IP server thật, private key hoặc secret vào repo.
- Không expose PostgreSQL public.
- Không đánh dấu Phase 2 Done nếu API smoke chưa pass.
```

## 3.3 Đã hoàn thành

```txt
✅ Project skeleton
✅ Frontend workspace placeholder
✅ Backend .NET skeleton phase 1
✅ Shared backend primitives
✅ Build backend pass
✅ Frontend build/typecheck pass
✅ Infrastructure docker compose config pass
✅ Docs/rules/prompt đã sync sang structure mới
✅ Roadmap Sync
```

---

# 4. Project Structure Đã Chốt

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
  roadmap/
  decisions/

temp/
  plan.md
```

---

# 5. Roadmap Tổng Thể

## Phase 0 - Foundation

### Status

```txt
✅ Done
```

### Mục tiêu

Tạo nền project để frontend/backend/agent làm tiếp mà không lệch architecture.

### Đã làm

```txt
- Tạo frontend workspace.
- Tạo 3 frontend app placeholder.
- Tạo frontend shared packages placeholder.
- Tạo backend service folders.
- Tạo infrastructure placeholders.
- Tạo docs skeleton.
- Sync docs/rules/prompt sang structure frontend/backend/infrastructure.
```

### Verify

```txt
frontend npm install/typecheck/build pass.
docker compose config pass.
```

---

## Phase 1 - Backend Foundation

### Phase 1.1 - .NET Service Skeleton

#### Status

```txt
✅ Done
```

#### Scope

```txt
backend/services/api-gateway
backend/services/identity-service
backend/services/tenant-service
```

#### Đã làm

```txt
- Tạo backend root solution.
- Tạo service solutions.
- Tạo Clean Architecture layers:
  - Api
  - Application
  - Domain
  - Infrastructure
  - Tests
- Tạo placeholder endpoints.
- Build pass.
```

---

### Phase 1.2 - Shared Backend Primitives

#### Status

```txt
✅ Done
```

#### Scope

```txt
backend/shared/building-blocks
backend/shared/contracts
backend/shared/observability
backend/services/api-gateway
backend/services/identity-service
backend/services/tenant-service
```

#### Đã làm

```txt
BuildingBlocks:
- TenantContext
- ITenantContextAccessor
- TenantContextAccessor
- TenantResolutionResult
- Result/Error model
- Guard
- Options placeholder

Contracts:
- TenantReference
- UserContext
- RoleNames
- PermissionCodes
- AuthRbacRequirement
- DomainEvent base
- TenantCreatedEvent

Observability:
- CorrelationIdMiddleware
- LoggingPropertyNames
- TraceContext
- HealthCheckTags
```

#### Verify

```txt
dotnet restore backend/ClinicSaaS.Backend.sln
dotnet build backend/ClinicSaaS.Backend.sln --no-restore
```

Build pass 0 warnings, 0 errors.

---

### Phase 1.3 - API Boundary Standardization

#### Status

```txt
✅ Done
```

#### Mục tiêu

Chuẩn hóa boundary cho API trước khi tạo database thật.

#### Scope

```txt
api-gateway
identity-service
tenant-service
shared building-blocks/contracts/observability
```

#### Cần làm

```txt
- Consolidate tenant middleware dùng chung nếu hợp lý.
- Đọc tenant từ X-Tenant-Id.
- Đọc tenant từ JWT claim tenant_id placeholder.
- Cho phép endpoint platform-scoped không cần tenant.
- Endpoint tenant-scoped phải có tenant context.
- Auth/RBAC placeholder rõ hơn:
  - RequireRole
  - RequirePermission
  - UserContext
- OpenAPI/Swagger config thật nếu package dùng được.
- Chuẩn hóa system endpoints:
  - /health
  - /api/_system/tenant-context
  - /api/_system/auth-rbac-placeholder
  - /api/_system/openapi-placeholder
```

#### Không làm

```txt
- Không connect PostgreSQL.
- Không tạo migration.
- Không implement tenant CRUD.
- Không sửa frontend.
- Không implement JWT thật.
```

#### Done khi

```txt
- Root backend solution restore/build pass.
- 3 service có API boundary thống nhất.
- docs/current-task.md được cập nhật.
- docs/roadmap/clinic-saas-roadmap.md được cập nhật.
```

#### Đã làm

```txt
- Shared tenant middleware resolve tenant từ X-Tenant-Id.
- Shared tenant middleware resolve tenant từ placeholder JWT claim tenant_id.
- Platform-scoped endpoints không cần tenant context.
- Tenant-scoped endpoints fail 400 nếu thiếu tenant context.
- RequireRole và RequirePermission placeholder gắn metadata endpoint.
- UserContext placeholder và IUserContextAccessor.
- Swashbuckle OpenAPI/Swagger config thật cho 3 Api projects.
- Chuẩn hóa system endpoints.
```

#### Verify

```txt
dotnet restore backend/ClinicSaaS.Backend.sln: pass.
dotnet build backend/ClinicSaaS.Backend.sln --no-restore: pass, 0 warnings, 0 errors.
api-gateway smoke: /health, tenant-context, auth-rbac-placeholder, /openapi/v1.json, /swagger pass.
```

---

# 6. Phase 2 - Tenant MVP Backend

### Status

```txt
✅ Done - 2026-05-10. Server PostgreSQL bootstrap pass; tenant-service + api-gateway smoke containers chạy ổn định; 5 smoke case PASS đủ qua api-gateway sau 2 vòng fix Dapper.
```

### Mục tiêu

Tạo vertical slice backend thật đầu tiên cho SaaS:

```txt
Owner Super Admin tạo phòng khám mới
↓
tenant-service lưu tenant vào PostgreSQL
↓
api-gateway expose API
```

### Scope

```txt
backend/services/tenant-service
backend/services/api-gateway
backend/shared/contracts
infrastructure/postgres
docs/architecture
```

### Domain model tối thiểu

```txt
Tenant
ClinicProfile
TenantStatus
TenantDomain
TenantModule
PlanReference
```

### Use cases tối thiểu

```txt
CreateTenant
GetTenantById
ListTenants
UpdateTenantStatus placeholder
```

### PostgreSQL schema bước đầu

```txt
tenants
tenant_profiles
tenant_domains
tenant_modules
```

### API endpoints đề xuất

```txt
POST /api/tenants
GET /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
```

### Rules

```txt
- Không dùng EF Core.
- Tenant Service persistence dùng Dapper + Npgsql.
- Migration dùng SQL files, không dùng EF migrations.
- Nếu cần migration runner, ưu tiên DbUp; FluentMigrator chỉ cân nhắc nếu cần fluent C# migration.
- Runtime production-safe đề xuất .NET 10 LTS; giữ net9.0 tạm nếu chưa có task upgrade riêng.
- API Gateway Phase 2 dùng typed HttpClient forwarding.
- YARP để cân nhắc ở phase gateway hardening sau.
- TenantStatus: Draft, Active, Suspended, Archived.
- TenantModule dùng string module_code; chưa làm billing/plan logic thật.
- Không dùng connection string thật.
- Dùng env/example config.
- Tenant root entity có id riêng.
- Domain phải unique.
- Tenant-owned data phải có tenant_id nếu thuộc dữ liệu con của tenant.
- Không làm billing/domain/template thật trong phase này.
```

### Done khi

```txt
- Tenant domain entities có trong Domain layer.
- Application use cases có command/query cơ bản.
- Infrastructure dùng Dapper + Npgsql, không DbContext/EF Core.
- Có SQL migration file hoặc SQL schema bootstrap theo plan.
- API endpoints build được.
- Root backend solution build pass.
- Full CRUD smoke qua PostgreSQL pass trước khi đánh dấu Done.
```

### Verify ngày 2026-05-09

```txt
git status --short: pass, xác nhận có staged Phase 2 changes.
git diff --cached --stat: 40 files, 3101 insertions, 9 deletions.
git diff --cached --name-only: pass.
dotnet restore backend/ClinicSaaS.Backend.sln: fail do PATH trỏ dotnet x86 không có SDK.
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln: pass.
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore: pass, 0 warnings, 0 errors.
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build: exit code 0.
docker compose -f infrastructure/docker/docker-compose.dev.yml config: pass.
```

### Commit và verify sau khi owner yêu cầu ngày 2026-05-09

```txt
git add -A: pass.
git diff --cached --check: pass.
git commit: 71923f8 feat: implement tenant mvp backend slice.
git status sau commit: clean.
branch: main ahead origin/main 1 commit, chưa push.
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln: pass.
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore: pass, 0 warnings, 0 errors.
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build: exit code 0.
docker compose -f infrastructure/docker/docker-compose.dev.yml config: pass.
docker info: fail vì Docker daemon chưa chạy.
```

Test note:

```txt
Test projects hiện là placeholder csproj, chưa có test framework package/test cases thực tế.
```

Runtime startup smoke:

```txt
tenant-service /health: 200.
tenant-service /openapi/v1.json: 200.
api-gateway /health: 200.
api-gateway /openapi/v1.json: 200.
```

Blocked runtime DB smoke:

```txt
Docker daemon không chạy:
open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.

Chưa verify được:
- POST /api/tenants
- GET /api/tenants
- GET /api/tenants/{id}
- PATCH /api/tenants/{id}/status
- duplicate slug/domain trả 409
```

### Server bootstrap theo yêu cầu owner ngày 2026-05-09

```txt
Mục tiêu: dùng server owner cung cấp để chạy Docker/PostgreSQL cho Phase 2 DB smoke.
Plan: temp/plan.backend.md.
Deploy note: docs/deployment/server-bootstrap.md.
Guardrail: không commit, không push, không ghi IP/private key/secret vào repo, không expose PostgreSQL public.
```

Kết quả bootstrap:

```txt
OS: Ubuntu 22.04.5 LTS.
CPU/RAM/disk: 4 cores, 5.8 GiB RAM, root disk 64G.
Docker Engine: 29.4.3.
Docker Compose plugin: v5.1.3.
PostgreSQL container: clinic-saas-postgres, postgres:16-alpine, Up.
Docker network: clinic-saas-network.
Docker volume: clinic-saas-postgres-data.
PostgreSQL port publish: none.
Schema Phase 2 apply pass.
Tables verified: platform.tenants, platform.tenant_profiles, platform.tenant_domains, platform.tenant_modules.
UFW không mở 5432/tcp.
```

### Smoke Pass 2026-05-10

Containers smoke (bind-mount publish output, network nội bộ):

```txt
clinic-saas-tenant-service-smoke -> 127.0.0.1:5006, mount /opt/clinic-saas/runtime-smoke/tenant-service:/app:ro
clinic-saas-api-gateway-smoke   -> 127.0.0.1:5005
clinic-saas-postgres            -> 5432/tcp internal only
```

Hai vòng fix backend trong session 2026-05-10:

```txt
Vòng 1: thêm Dapper SqlMapper.TypeHandler<DateTimeOffset> để Npgsql 6+ trả timestamptz về DateTime vẫn map được sang DateTimeOffset của row record.
Vòng 2: reorder positional record TenantListRow cho khớp thứ tự cột SELECT trong ListAsync (Dapper 2.1 với positional record matching theo position type, không qua type handler trong giai đoạn lookup constructor).
```

Kết quả 5 smoke case qua `api-gateway` (`127.0.0.1:5005`):

```txt
POST /api/tenants -> 201
duplicate slug/domain -> 409
GET /api/tenants -> 200
GET /api/tenants/{id} -> 200
PATCH /api/tenants/{id}/status -> 200
```

Tenant test trong DB sau smoke: `demo-clinic-smoke-002`, `demo-clinic-smoke-003` (cả hai status `Active` sau PATCH). Không expose 5432 public, không ghi connection string thật vào file tracked.

---

# 7. Phase 3 - Owner Admin Tenant Slice

### Status

```txt
🟡 Implementation Done — chờ owner duyệt commit (commit 7f6366d). Pre-Phase 4 Hardening (P1.6 + P1.7) committed.
```

### Mục tiêu

Nối frontend Owner Super Admin với Tenant backend để có luồng đầu tiên chạy được từ UI -> API -> DB.

### Scope

```txt
frontend/apps/owner-admin
frontend/packages/api-client
frontend/packages/shared-types
frontend/packages/ui
frontend/packages/design-tokens nếu cần map token Figma V2
```

Backend/services chỉ là API contract dependency ở Phase 3 frontend planning. Frontend workstream không sửa backend trong slice đầu nếu owner không yêu cầu rõ.

### UI routes

```txt
/clinics
/clinics/create
```

### Cần làm

```txt
- Owner Admin list tenant.
- Owner Admin create tenant form.
- Mock auth/role:
  - role: OwnerSuperAdmin
  - permissions: tenants.read, tenants.write
- API client gọi:
  - GET /api/tenants
  - POST /api/tenants
- Hiển thị trạng thái success/error.
```

### Không làm

```txt
- Không làm login thật.
- Không làm billing thật.
- Không làm domain setup thật.
- Không làm template apply thật.
```

### Done khi

```txt
- Owner Admin tạo tenant mới được qua UI.
- Tenant lưu vào PostgreSQL.
- Refresh list thấy tenant mới.
- Build frontend/backend pass.
```

---

# 7.1 UI Redesign V3 - Source of Truth Phase 4+

### Status

```txt
🟢 Active source of truth Phase 4+ (chốt 2026-05-10).
```

### Tổng quan

UI Redesign V3 là source of truth UI cho mọi phase từ Phase 4 trở đi.

```txt
Figma file key : 1nbJ5XkrlDgQ3BmzpXzhCC
Page V3 id     : 65:2 "UI Redesign V3 - 2026-05-10"
Total frame    : 76 (16 V3v1 từ vòng đầu + 60 V3v2 expand 2026-05-10)
Section count  : 7
Direction      : Calm Clinical Premium (Public) + Clinical SaaS Cockpit (Admin)
```

V1/V2/V3 status policy:

```txt
V1 (page Figma 0:1, 40 frame UI Kit) : functional baseline historical, KHÔNG sửa, KHÔNG xóa.
V2 (page Figma 36:2, empty)          : historical/partial baseline, KHÔNG sửa, KHÔNG xóa.
V3 (page Figma 65:2, 76 frame)       : active source of truth Phase 4+.
```

### 7.1.1 Section Grouping (7 section)

```txt
Section 0  OPEN HERE Overview & TOC                  entry frame 104:2
Section 1  Public Website (15 frame V3v2 + 2 V3v1)   entry frame 105:2 About
Section 2  Booking (5 frame V3v2 + 1 V3v1)           entry frame 110:2 Landing
Section 3  Clinic Admin (21 frame V3v2 + 4 V3v1)     entry frame 113:2 Brand
Section 4  Owner Admin (10 frame V3v2 + 7 V3v1)      entry frame 124:2 Dashboard cross-tenant
Section 5  Design System (6 frame V3v2 + 1 V3v1)     entry frame 127:2 Component Inventory 38
Section 6  Handoff (2 frame V3v2 + 1 V3v1)           entry frame 129:2 Implementation Waves
```

Frame ID chính trong từng section (đối chiếu khi code wave tương ứng):

```txt
Section 1 Public:
  68:2 Homepage Desktop, 78:2 Homepage Mobile, 105:2 About,
  106:2 Services List, 106:179 Service Detail,
  107:2 Doctors List, 107:180 Doctor Profile,
  108:2 Pricing, 108:132 Contact+Map, 108:197 FAQ, 108:250 Errors 4-variant,
  109:2 Blog List wireframe, 109:70 Blog Detail wireframe,
  109:122 Dynamic Preview wireframe, 109:144 Pricing Variants wireframe,
  109:168 Mobile Responsive wireframe, 109:251 Sitemap Tree wireframe.

Section 2 Booking:
  81:2 Booking Flow Desktop (V3v1),
  110:2 Landing, 110:91 Insurance Step, 110:165 Reschedule, 110:251 Cancel,
  111:2 Mobile Full Flow 5-step.

Section 3 Clinic Admin:
  83:2 Builder, 84:2 Publish dialog, 84:28 Conflict, 84:40 Annotation Builder,
  113:2 Brand & Logo, 113:157 Slider Editor,
  116:2 Template Library, 116:207 Template Apply Dialog,
  117:2 Domain Settings, 117:176 DNS Verify State,
  119:2 Services & Pricing,
  121:2 Dashboard wireframe, 121:167 Menu Builder wireframe,
  121:340 Publish History wireframe, 121:433 Appointments Calendar wireframe,
  122:2 Appointment Drawer wireframe, 122:81 Patients List wireframe,
  122:279 Patient Detail wireframe, 122:377 Doctors List wireframe,
  123:2 Doctor Schedule wireframe, 123:171 Records APSO wireframe,
  123:251 Payments wireframe, 123:398 Reports wireframe,
  123:490 Notifications wireframe, 123:594 System Settings wireframe.

Section 4 Owner Admin:
  85:2 Tenant Operations, 87:2 Tenant Detail Drawer,
  88:2 Create Tenant Wizard, 88:112 Empty state,
  88:119 Conflict slug/domain, 88:127 Command palette, 88:180 Annotation Owner,
  124:2 Dashboard cross-tenant, 124:292 Tenant Lifecycle Confirm Modal,
  125:2 Domain DNS Retry, 125:122 SSL Pending,
  126:2 Plan/Module Catalog wireframe, 126:173 Plan Assignment wireframe,
  126:300 Audit Log wireframe, 126:458 Monitoring & Support wireframe,
  126:612 Billing Tổng wireframe, 126:710 Platform Settings wireframe.

Section 5 Design System:
  66:2 Design Tokens Reference (V3v1),
  127:2 Component Inventory, 127:410 Layout Grid + Responsive Rules,
  127:518 A11y WCAG 2.2 Poster,
  128:2 Motion Timeline wireframe, 128:84 Performance Budget wireframe,
  128:178 Route Mapping wireframe.

Section 6 Handoff:
  89:2 Handoff Notes (V3v1),
  129:2 Implementation Waves,
  129:197 Open Questions & Risks (9 risk, 5 cần owner decision).
```

### 7.1.2 Token V3 (chốt)

```txt
Brand:
  #1E5BD6 clinical-blue, #14B8A6 mint, #FFB7A0 peach.

Surface:
  #F8FAFC ivory, #FFFFFF elevated, #F1F5F9 muted, #E2E8F0 border.

Text:
  #0F172A primary, #475569 secondary, #94A3B8 muted.

Status 2026:
  #16A34A success, #D97706 warning, #DC2626 danger,
  #0284C7 info, #64748B draft, #7C3AED specialty.

Admin sidebar:
  #0F172A.

Typography (Inter scale):
  display 48/56, headline 32/40, title 24/32, body 16/24, caption 12/16, mono 13/20.

Spacing (4-base):
  4 / 8 / 12 / 16 / 20 / 24 / 32 / 40 / 56 / 72 / 96.

Radius:
  card 16, button 12, input 10, pill 999, sheet-top 24.

Shadow:
  elevation/1 0 1px 2px rgba(15,23,42,.04)
  elevation/2 0 8px 24px rgba(15,23,42,.06)
  elevation/3 0 16px 48px rgba(15,23,42,.10)

Motion:
  duration xs 120 / sm 180 / md 240 / lg 320 / xl 480 ms
  ease/standard cubic-bezier(.2,0,0,1)
```

### 7.1.3 Component & Composable Cần Build (Phase 4+)

```txt
Tổng component cần build: 38 V3v2 mới + 22 V3v1 (đã có frame Figma).
Composable foundation: 6 chính + 6 phụ trợ.

Public (12):
  AboutPillarCard, MissionStatement, AccreditationRow, InstituteCard,
  ServiceDetailLayout, DoctorListFilterBar, DoctorProfileCard,
  PricingTierCard, FeatureComparisonTable, ContactZoneBlock,
  FaqAccordionGroup, BlogStickyToc.

Public dynamic + error (4):
  TenantPreviewFrame, ResponsivePreviewToggle, ErrorStateLayout, MaintenanceCountdown.

Booking (4):
  BookingLandingHero, InsuranceStepForm, RescheduleConfirm, MobileBottomSheetSlotPicker.

Clinic Admin (12):
  KpiTileStrip, TodayTimelineRow, BrandUploadDual, SliderItemEditor,
  MenuBuilderTree, TemplateApplyDialog, DomainStateBadge, DnsTable,
  AppointmentCalendarView, PatientDetailTabs, DoctorScheduleGrid, RecordEditorApso.

Owner Admin (4):
  MrrChart, PlanModuleMatrix, AuditLogDrawer, ServiceHealthCard.

V3v1 đã có (22):
  HeroPrimary, TrustStrip, SpecialtyCard, DoctorCard,
  BookingStepper, SlotPickerGrid, SlotPickerScrollSnap, BottomSheet,
  StickyBookBar, FAQAccordion, TestimonialCard, InsuranceChip, FooterMain,
  FilterRail, DoctorListCard, ConfirmationPanel, ConfirmationSuccess,
  ConflictBanner, SessionExpiredCard, NoSlotEmpty,
  BuilderShell, PropertyTabs, SectionList, PropertyField, ColorTokenPicker,
  CanvasPreview, PublishConfirmDialog, ConflictRefreshBanner,
  KPITile, DataTable, TenantRow, StatusPill, PlanBadge, ModuleChips,
  DomainStateRow, TenantDetailDrawer, CreateTenantWizard, EmptyState,
  TenantSwitcher, CommandPalette.

Composable foundation (6):
  useReducedMotion, useViewTransition, useFocusTrap,
  useTenantContext, useCommandPalette, useReveal.

Composable phụ trợ (6):
  usePublicTenantResolver, useSlotLock, useInsuranceVerify,
  useScheduleConflict, useDomainVerifyPoll, useReportExport.
```

### 7.1.4 Phase 4+ Implementation Plan - 5 Wave

#### Wave A - V3 Token Foundation + Owner Admin restyle + shared UI/composables

```txt
Effort  : ~34 dev-day, 4 tuần, 2 FE
Status  : 🟡 Planning (chờ owner duyệt)
Frame Figma source:
  66:2 Tokens, 127:2 Component Inventory, 127:410 Layout Grid,
  127:518 A11y, 124:2 Dashboard cross-tenant, 124:292 Lifecycle modal,
  125:2 DNS Retry, 125:122 SSL Pending,
  85:2 / 87:2 / 88:2 / 88:112 / 88:119 / 88:127 Owner Admin V3v1 enhance,
  104:2 OPEN HERE.
Scope:
  - Token V3 ADD-ONLY layer trong design-tokens/v3/* + CSS custom property.
  - httpClient factory rebuild (owner / clinic / public surface).
  - Composable foundation: useTenantContext, useReducedMotion,
    useFocusTrap, useViewTransition.
  - Owner Admin Phase 3 restyle V3: TenantTable, TenantDetailDrawer,
    CreateTenantWizard, ConflictState, MetricCard, StatusPill update token.
  - Thêm 7 component shared mới: KPITile (sparkline), ModuleChips (x/12),
    PlanBadge, EmptyState, TenantSwitcher, CommandPalette, DomainStateRow.
  - Thêm 4 frame Owner Admin V3v2: Dashboard cross-tenant,
    Tenant Lifecycle Confirm Modal, Domain DNS Retry, SSL Pending.
  - Histoire Vue 3 setup frontend/packages/ui/.histoire/.
Backend dep:
  - Phase 4.1 Domain Service contract OpenAPI (mock OK).
  - Phase 4.2 Template Service stub.
  - Phase 4.3 CMS settings/sliders contract.
QA gate:
  - axe-core CI baseline, Lighthouse CI baseline 4 page Owner Admin.
  - VRT 14 component Phase 3 trước/sau token V3.
```

#### Wave B - Public Website V3

```txt
Effort  : ~36 dev-day, 4-5 tuần, 2 FE
Status  : 🔜 Sau Wave A
Frame Figma source:
  68:2, 78:2, 105:2 đến 109:251.
Scope:
  - Scaffold frontend/apps/public-web/ (router, stores, tenant context).
  - 21 component build: HeroPrimary, TrustStrip, SpecialtyCard, DoctorCard,
    AboutPillarCard, MissionStatement, AccreditationRow, InstituteCard,
    ServiceDetailLayout, DoctorListFilterBar, DoctorProfileCard,
    PricingTierCard, FeatureComparisonTable, FaqAccordionGroup, BlogStickyToc,
    TestimonialCard, InsuranceChip, FooterMain, ContactZoneBlock,
    TenantPreviewFrame, ResponsivePreviewToggle, ErrorStateLayout,
    MaintenanceCountdown.
  - Composable: useReveal, usePublicTenantResolver.
  - Edge resolver subdomain → tenantId qua nginx hoặc Cloudflare Worker meta tag.
Routes mới:
  /, /about, /services, /services/:slug, /doctors, /doctors/:slug,
  /pricing, /contact, /faq, /blog, /blog/:slug,
  /error/404 | 500 | 503 | maintenance.
Backend dep:
  - Phase 4.3 Website CMS production-ready.
  - Catalog Service public endpoints
    (GET /api/public/services | doctors | services/:slug | doctors/:slug).
  - Public BFF tenant resolution endpoint hoặc edge resolver.
  - Domain Service publish state.
QA gate:
  - Lighthouse CI mobile + desktop cho 9 page,
    target LCP <2.5s, INP <200ms, CLS <0.1.
  - axe-core full audit, tap target 44 audit.
```

#### Wave C - Booking V3

```txt
Effort  : ~42 dev-day, 4-5 tuần, 3 FE
Status  : 🔜 Sau Wave A
Frame Figma source:
  81:2, 110:2, 110:91, 110:165, 110:251, 111:2.
Scope:
  - Booking flow desktop 4-step (V3v1) + 5 frame V3v2 mới
    (Landing, Insurance, Reschedule, Cancel, Mobile 5-step).
  - 12 component build: BookingStepper, SlotPickerGrid, SlotPickerScrollSnap,
    BottomSheet, StickyBookBar, BookingLandingHero, InsuranceStepForm,
    RescheduleConfirm, MobileBottomSheetSlotPicker, ConfirmationPanel,
    ConfirmationSuccess, ConflictBanner, SessionExpiredCard, NoSlotEmpty.
  - Composable: useSlotLock (polling/SSE), useInsuranceVerify,
    useScheduleConflict.
Routes mới:
  /booking, /booking/landing, /booking/insurance,
  /booking/reschedule/:appointmentId, /booking/cancel/:appointmentId.
Backend dep:
  - Booking Service:
    POST /api/bookings, GET available-slots,
    PATCH reschedule | cancel,
    slot lock concurrency POST /api/bookings/:id/lock với ETag/version.
  - Insurance verify integration stub.
QA gate:
  - Booking flow E2E happy path Playwright + concurrency 2-user race
    condition mock SSE.
  - Slot picker grid keyboard nav (arrow + enter).
  - Tap target audit mobile bottom sheet.
  - Lighthouse mobile booking page.
```

#### Wave D - Clinic Admin Website Builder V3 + Operational

```txt
Effort  : ~60 dev-day, 6-8 tuần, 3 FE
Status  : 🔜 Sau Wave A (có thể song song với B/C nếu owner duyệt)
Frame Figma source:
  83:2, 84:2, 84:28, 84:40,
  113:2, 113:157, 116:2, 116:207, 117:2, 117:176, 119:2,
  121:2 đến 123:594.
Scope:
  - Scaffold frontend/apps/clinic-admin/.
  - 21 component Clinic Admin build: BuilderShell, PropertyTabs,
    SectionList drag-drop, PropertyField, ColorTokenPicker, CanvasPreview,
    PublishConfirmDialog, ConflictRefreshBanner, BrandUploadDual,
    SliderItemEditor, MenuBuilderTree, TemplateApplyDialog,
    DomainStateBadge, DnsTable, AppointmentCalendarView, PatientDetailTabs,
    DoctorScheduleGrid, RecordEditorApso, KpiTileStrip, TodayTimelineRow,
    DataTable shared admin.
  - Composable: useDomainVerifyPoll (polling), autosave debounce 800ms.
Routes mới:
  /builder, /brand, /slider, /templates, /domain,
  /appointments, /appointments/calendar,
  /patients, /patients/:id, /doctors, /doctors/:id/schedule,
  /services-pricing, /menu-builder, /publish-history,
  /records, /payments, /reports, /notifications, /settings.
Backend dep:
  - Phase 4.3 CMS production write.
  - Phase 4.2 Template apply.
  - Phase 4.1 Domain DNS verify async + SSL provisioning.
  - Catalog Service write (services + doctors + schedule).
  - Customer Service patient endpoints + Records APSO.
  - Booking Service Clinic Admin appointment view.
QA gate:
  - Builder drag-drop kbd alternative (WCAG 2.5.7).
  - Autosave conflict resolution test (409 lock).
  - Calendar 1000-event load test.
  - Axe-core builder + calendar + drawer focus trap.
  - Lighthouse desktop admin.
```

#### Wave E - A11y / Performance / QA Polish + Reports / Audit / Monitoring / Billing

```txt
Effort  : ~30 dev-day, 5-6 tuần, 2 FE + 1 DevOps
Status  : 🔜 Sau Wave B/C/D
Frame Figma source:
  126:2, 126:173, 126:300, 126:458, 126:612, 126:710,
  128:2, 128:84, 128:178, 129:2, 129:197.
Scope:
  - Full a11y audit pass (5 SC WCAG 2.2 mới: 2.4.11, 2.4.12, 2.5.7, 2.5.8,
    3.2.6, 3.3.7).
  - Lighthouse CI gate enforce (LCP <2.5s, INP <200ms, CLS <0.1).
  - VRT screenshot diff toàn 76 surface.
  - Owner Admin Phase 7-8 frame implement: Plan/Module Catalog,
    Plan Assignment, Audit Log, Monitoring & Support, Billing Tổng,
    Platform Settings.
  - Composable: useCommandPalette + cmdk fuzzy, useReportExport (CSV/Excel).
  - Tenant suspended fallback page polish.
Backend dep:
  - Report Service aggregate.
  - Audit Log Service 90-day retention pagination.
  - Monitoring Service 12-service health.
  - Billing Service Stripe / VNPay sandbox.
  - Notification Service queue.
QA gate:
  - Full E2E happy path + edge case Playwright.
  - axe-core CI fail-build threshold.
  - Lighthouse CI fail-build threshold.
  - VRT diff approval.
  - Security review (PII patient handling, audit retention compliance HIPAA decision).
```

### 7.1.5 Top Risk Phase 4+ (V3v2)

```txt
1. CRITICAL  httpClient hardcode `X-Owner-Role: OwnerSuperAdmin`
             — Wave A bắt buộc rebuild factory pattern.
2. HIGH      Token migration drift V2 hex literal vs V3 CSS var
             — Wave A ADD-ONLY strategy không break Phase 3.
3. HIGH      Public Web tenant resolution race LCP
             — Wave B edge resolver inject meta tag.
4. HIGH      Booking slot concurrency
             — Wave C ETag/version + SSE upgrade.
5. HIGH      Builder + Calendar performance INP
             — Wave D virtual scroll + lazy chunk.
6. MEDIUM    Domain DNS verify polling memory leak
             — Wave A useDomainVerifyPoll cleanup.
7. MEDIUM    A11y regression V3v2
             — axe-core CI từ Wave A.
```

### 7.1.6 Open Questions Cần Owner Decision (Frame Figma 129:197)

5 quyết định bắt buộc owner chốt trước khi Wave A/B/C/D/E start tương ứng:

```txt
1. Audit log retention: 90 ngày Linear-style hay 6 năm HIPAA?
   (Block Wave E - Audit Log Service.)

2. PII patient handling rule trong Records APSO + Patient detail.
   (Block Wave D - Records APSO + Patient detail; Wave E - security review.)

3. Drag-drop builder autosave conflict resolution policy:
   optimistic merge / pessimistic lock / manual.
   (Block Wave D - Builder autosave.)

4. Domain DNS race tolerance + retry max attempts.
   (Block Wave A - useDomainVerifyPoll; Wave D - DNS verify async.)

5. Multi-tenant edge case: tenant suspended public fallback content & branding policy.
   (Block Wave B - tenant resolution + suspended page; Wave E - polish.)
```

---

# 8. Phase 4 - Domain / Template / Website Settings

### Status

```txt
🔜 Sau khi Tenant Slice chạy được
```

### Mục tiêu

Sau khi tạo tenant chạy được, thêm khả năng cấu hình website cho từng tenant.

### Services liên quan

```txt
domain-service
template-service
website-cms-service
clinic-admin
owner-admin
```

## Phase 4.1 - Domain Service

### Cần làm

```txt
- Default subdomain.
- Custom domain placeholder.
- DNS verification placeholder.
- SSL status placeholder.
- Publish status.
```

### API đề xuất

```txt
POST /api/tenants/{tenantId}/domains
GET /api/tenants/{tenantId}/domains
POST /api/tenants/{tenantId}/domains/{domainId}/verify
POST /api/tenants/{tenantId}/publish
```

---

## Phase 4.2 - Template Service

### Cần làm

```txt
- Template library:
  - dental
  - eye
  - dermatology
  - pediatric
  - general
  - custom
- Apply modes:
  - apply full template
  - apply style only
  - apply content only
```

### API đề xuất

```txt
GET /api/templates
GET /api/templates/{templateKey}
POST /api/tenants/{tenantId}/template/apply
```

---

## Phase 4.3 - Website CMS Service

### Cần làm

```txt
- website settings
- logo
- brand colors
- menu
- homepage modules
- slider
- page content
```

### MongoDB collections đề xuất

```txt
website_settings
homepage_blocks
sliders
page_contents
template_configs
```

### API đề xuất

```txt
GET /api/tenants/{tenantId}/website/settings
PUT /api/tenants/{tenantId}/website/settings

GET /api/tenants/{tenantId}/website/sliders
POST /api/tenants/{tenantId}/website/sliders
PUT /api/tenants/{tenantId}/website/sliders/{slideId}
DELETE /api/tenants/{tenantId}/website/sliders/{slideId}
```

---

## Phase 4.4 - Clinic Admin Website Settings UI

### Cần làm

```txt
- Clinic Admin Website Settings screen.
- Logo & Brand Settings.
- Homepage Slider Setup.
- Template Library/Config.
- Preview homepage dynamic.
```

### Done khi

```txt
- Clinic Admin đổi logo/màu/slider/template config được.
- Config lưu được.
- Public Website có thể đọc config sau này.
```

---

# 9. Phase 5 - Public Website Dynamic

### Status

```txt
🔜 Sau Phase 4
```

### Mục tiêu

Public website render động theo tenant.

### Scope

```txt
frontend/apps/public-web
backend/services/api-gateway
backend/services/website-cms-service
backend/services/domain-service
backend/services/catalog-service
```

### Cần làm

```txt
- Resolve tenant theo domain/subdomain.
- Load theme/settings.
- Load logo.
- Load slider.
- Load services.
- Load staff.
- Render homepage dynamic.
- Render mobile responsive.
```

### Tenant resolution

```txt
domain/subdomain
↓
domain-service
↓
tenantId
↓
website-cms-service
↓
settings/homepage content
```

### Public routes

```txt
/
/services
/services/:slug
/staff
/booking
/contact
```

### Done khi

```txt
- Public website mở theo tenant/subdomain mock được.
- Homepage đổi theo logo/theme/slider.
- Dữ liệu không lẫn tenant.
```

---

# 10. Phase 6 - Booking / Catalog

### Status

```txt
🔜 Sau Public Website Dynamic
```

### Mục tiêu

Tạo luồng đặt lịch public và quản lý dịch vụ/bác sĩ/lịch làm việc.

### Services liên quan

```txt
catalog-service
booking-service
clinic-admin
public-web
notification-service placeholder
```

### Catalog model

```txt
Service
ServiceCategory
Doctor/Staff
WorkingSchedule
Price
```

### Booking model

```txt
Appointment
BookingSlot
AppointmentStatus
PatientInfo
BookingSource
```

### Public booking flow

```txt
1. Chọn dịch vụ.
2. Chọn bác sĩ nếu có.
3. Chọn ngày/giờ.
4. Nhập thông tin.
5. Xác nhận.
6. Tạo appointment.
```

### Clinic Admin booking management

```txt
- List appointments.
- Filter by status/date/service.
- Confirm/cancel appointment.
- View patient info.
```

### Done khi

```txt
- Public user tạo booking được.
- Clinic Admin thấy booking.
- Tenant isolation đảm bảo.
- Booking event placeholder phát ra AppointmentCreated.
```

---

# 11. Phase 7 - Billing / Reports / Notification / Realtime

### Status

```txt
🔜 Later
```

### Billing

```txt
- subscription
- invoice
- renewal
- payment status
```

### Reports

```txt
- tenant dashboard KPIs
- appointments count
- revenue placeholder
- service performance
```

### Notification

```txt
- email placeholder
- SMS placeholder
- Zalo-ready placeholder
- notification.requested event
```

### Realtime

```txt
- SignalR/WebSocket gateway
- live appointment notification
- dashboard update
```

---

# 12. Phase 8 - Hardening / Production

### Status

```txt
🔜 Later
```

### Cần làm

```txt
- real auth/JWT
- RBAC enforcement thật
- production database config
- secrets management
- CI/CD
- monitoring
- logging/tracing
- backup/restore
- domain SSL automation
- rate limit
- audit log
- security review
```

---

# 13. Current Next Actions

## Ngay bây giờ

Project đang chạy 2 workstream song song, không còn một current task duy nhất.

Backend/DevOps lane:

```txt
✅ 2026-05-10: Phase 2 API Runtime Smoke Gate Done (xem mục 6 và docs/current-task.backend.md).
1. (Optional) Owner duyệt commit backend fix + lane docs.
2. (Optional) Dọn backup directories trên server: tenant-service.bak.20260509T191310Z và .20260509T191905Z khi không cần rollback.
3. (Optional) Bổ sung integration test PostgreSQL bằng Testcontainers cho TenantService.Tests để cover 2 root cause vừa fix.
4. Sẵn sàng support Frontend chuyển từ mock fallback sang real API trong Phase 3.
```

Frontend lane:

```txt
1. Phase 3 Owner Admin Tenant Slice Implementation Done (commit 7f6366d). Pre-Phase 4 Hardening (P1.6 + P1.7) committed.
2. Phase 4 Wave A V3 Foundation: chờ owner duyệt plan trong temp/plan.frontend.md §16.
3. Chờ 5 owner decision (audit retention, PII rule, autosave conflict policy, DNS retry tolerance, tenant suspended fallback) trước Wave B/C/D/E (xem section 7.1.6).
4. Khi Wave A được duyệt, implement theo plan: token V3 ADD-ONLY + httpClient rebuild + Owner Admin restyle + 7 component shared mới + 4 composable foundation + 4 frame Owner Admin V3v2.
5. Không sửa backend, không sửa Figma, không tạo Figma file mới, không commit nếu owner chưa yêu cầu.
```

5 owner decision cần để Phase 4 không bị block:

```txt
1. Audit log retention scope (90d / 6y) — block Wave E.
2. PII patient handling rule — block Wave D + Wave E security review.
3. Builder autosave conflict policy — block Wave D.
4. DNS retry tolerance — block Wave A composable + Wave D.
5. Tenant suspended fallback policy — block Wave B + Wave E.
```

Hiện đã pass:

```txt
restore/build bằng dotnet x64.
docker compose config.
tenant-service startup health/openapi.
api-gateway startup health/openapi.
local commit 71923f8.
server PostgreSQL bootstrap pass.
agent workflow cho UI redesign đã tối ưu: Figma UI Agent, Web Research Agent, Lead Agent và Playwright MCP status.
Figma UI Redesign V2 đã thêm 6 frame: Research Direction + Design Tokens, Public Homepage Desktop, Public Booking Flow Desktop, Clinic Admin Website Builder, Owner Admin Tenant Operations, Public Homepage Mobile.
Figma UI Redesign V2 polish pass: screenshot verify lại 5 frame chính, không tạo Figma file mới, không sửa backend/frontend code.
```

Blocked:

```txt
Backend/DevOps: Full API smoke qua tenant-service/api-gateway chưa pass.
Frontend: Chưa bị block kỹ thuật; đang chờ owner duyệt implement theo plan.
```

## Sau đó

Sau khi Phase 2 API smoke pass, Lead Agent cập nhật backend lane và dashboard để chuyển Phase 2 Done.

Sau khi owner duyệt Phase 3 frontend plan, Frontend Agent bắt đầu implement Owner Admin Tenant Slice theo `temp/plan.frontend.md`.

Prompt tiếp theo nếu owner muốn hoàn tất DB smoke:

```txt
Tiếp tục verify DB runtime smoke Phase 2 Tenant MVP Backend.

Bật Docker/PostgreSQL local nếu cần, không implement feature mới.

Chạy smoke qua api-gateway:
   - POST /api/tenants
   - GET /api/tenants
   - GET /api/tenants/{id}
   - PATCH /api/tenants/{id}/status
   - duplicate slug/domain trả 409

Sau đó cập nhật docs/current-task.backend.md, docs/current-task.md dashboard và docs/roadmap/clinic-saas-roadmap.md theo kết quả thật.
Không commit nếu owner chưa yêu cầu.
```

---

# 14. Status Legend

```txt
✅ Done
🟡 In Progress
🔜 Next / Planned
⏸ Paused
❌ Blocked
```

---

# 15. Important Rules

```txt
- Không tạo Figma file mới nếu user không yêu cầu.
- Không đổi architecture nếu không cập nhật docs.
- Không bỏ tenant isolation.
- Không hard-code secret.
- Không dùng connection string thật.
- Không để Codex tự làm nhiều phase một lúc.
- Mỗi phase phải có plan, implement, verify, report.
- Sau mỗi phase lớn nên commit checkpoint.
- V3 (page Figma 65:2) là UI source of truth Phase 4+. Không tự invent layout
  khi V3 đã có frame detailed visual. Không tạo Figma file mới. Không sửa V1/V2
  baseline (page 0:1 và page 36:2). Frame wireframe annotated chỉ là placeholder
  Phase 6-7, cần upgrade detailed trước khi code wave đó.
```
# Roadmap Update - 2026-05-09 - Phase 3 Frontend Planning

## Phase 3 - Owner Admin Tenant Slice

Trạng thái hiện tại:

```txt
🟡 Planning Done - Chờ owner duyệt implement
```

Ghi chú:

```txt
Phase 2 Tenant API đã implement nhưng DB/API smoke có thể chưa Done.
Owner đã cho phép chuyển sang Phase 3 Frontend Planning.
Frontend Phase 3 sẽ dùng API client real/mock fallback để không block UI shell.
Chưa code frontend trong lượt planning này.
```

Plan hiện nằm tại:

```txt
temp/plan.frontend.md
```

Scope Phase 3 task đầu:

```txt
App trước: frontend/apps/owner-admin
Routes: /dashboard, /clinics, /clinics/create, /clinics/:tenantId
Frame Figma chính: UI Redesign V2 - 2026-05-09 / V2 - Owner Admin Tenant Operations
Không sửa backend.
Không sửa Figma.
Không commit.
Không làm Clinic Admin/Public Website code trước khi Owner Admin Slice xong.
```

Owner cần duyệt plan trước khi implement.

# Roadmap Update - 2026-05-09 - Multi-Workstream Task Lanes

## Task Management

Trạng thái hiện tại:

```txt
docs/current-task.md là Project Coordination Dashboard.
temp/plan.md là index tương thích cũ.
Backend/DevOps dùng docs/current-task.backend.md và temp/plan.backend.md.
Frontend dùng docs/current-task.frontend.md và temp/plan.frontend.md.
```

Ý nghĩa:

```txt
Backend/DevOps có thể tiếp tục Phase 2 API Runtime Smoke Gate mà không ghi đè frontend context.
Frontend có thể tiếp tục Phase 3 Owner Admin Tenant Slice mà không ghi đè backend context.
Lead Agent chịu trách nhiệm đồng bộ dashboard và roadmap khi lane status thay đổi.
```

Guardrail:

```txt
Không agent nào overwrite docs/current-task.md bằng task chi tiết của một lane.
Không dùng temp/plan.md làm plan chi tiết khi đã có lane file phù hợp.
Nếu không chắc task thuộc lane nào, Lead Agent ghi Notes/Unclassified ngắn trong dashboard rồi phân loại sau.
```
