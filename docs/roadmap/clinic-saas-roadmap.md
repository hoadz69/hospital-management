# Clinic SaaS Multi Tenant - Roadmap & Status Tracker

> Cập nhật hiện hành 2026-05-10: project đang chạy 2 workstream song song. Backend/DevOps đã hoàn tất Phase 2 API Runtime Smoke Gate, 5 smoke case PASS đủ trên server `116.118.47.78` sau 2 vòng fix (Dapper type handler + reorder positional record `TenantListRow`); Phase 2 chuyển Done. Frontend ở Phase 3 Owner Admin Tenant Slice, implementation đã Done với mock fallback, sẵn sàng chuyển sang real API. Dashboard nằm ở `docs/current-task.md`; chi tiết lane nằm ở `docs/current-task.backend.md`, `docs/current-task.frontend.md`, `temp/plan.backend.md`, `temp/plan.frontend.md`.

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
- Backend/DevOps: Phase 2 API Runtime Smoke Gate ✅ Done 2026-05-10 (5 smoke case PASS đủ trên server)
- Frontend: Phase 3 Owner Admin Tenant Slice (Implementation Done với mock fallback, sẵn sàng chuyển real API)
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
🟡 Parallel Planning - Chờ owner duyệt implement
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
1. Giữ Phase 3 Owner Admin Tenant Slice ở trạng thái Planning Done.
2. Chờ owner duyệt implement rõ.
3. Khi được duyệt, implement theo temp/plan.frontend.md.
4. Không sửa backend, không sửa Figma, không commit nếu owner chưa yêu cầu.
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
