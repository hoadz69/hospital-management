# Clinic SaaS - Report khởi tạo project, Agent workflow và Figma/Codex

## 1. Trạng thái Figma hiện tại

ChatGPT/Figma tool đã đọc được file Figma mới.

UI Figma mới:

```txt
https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?node-id=0-1&p=f&t=5lnVCBz1BThNHg48-0
```

Link ngắn nên dùng trong prompt:

```txt
https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC
```

File UI hiện có các nhóm màn chính:

```txt
Client:
- Client - Homepage Desktop
- Client - Appointment Booking
- Client - Mobile Home
- Client - Homepage Dynamic Preview

Admin / Clinic Admin:
- Admin - Dashboard Overview
- Admin - Dynamic Website Settings
- Admin - Homepage Slider Setup
- Admin - Clinic Template Library
- Admin - Logo & Brand Settings
- Admin - Menu & Content Builder
- Admin - Appointments Management
- Admin - Patients Management
- Admin - Doctors Management
- Admin - Services & Pricing
- Admin - Records Management
- Admin - Payments & Invoices
- Admin - Reports Analytics
- Admin - System Settings

SaaS flow:
- 00 - Architecture Source of Truth
- 01 - System Concept Overview
- 02 - Super Admin Dashboard
- 03 - Create Clinic Tenant Flow
- 04 - Domain Setup Flow
- 05 - Template Selection Flow
- 06 - Clinic Admin Dashboard
- 07 - Clinic Website Settings
- 08 - Public Homepage Dynamic
- 09 - Public Booking Flow
- 10 - Mobile Responsive Set

Design system:
- Design System - Project Colors & UI Tokens
```

Architecture FigJam đang dùng:

```txt
https://www.figma.com/board/zVIS2cgoNqwC21lZbpApjp/Clinic-SaaS-Architecture---Source-of-Truth?t=L0tWxOID86LOXPh0-0
https://www.figma.com/board/j4vDRWSIRSckcAYvXHocMc/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?t=L0tWxOID86LOXPh0-0
```

---

## 2. Mục tiêu project

Project không phải website phòng khám đơn lẻ. Đây là nền tảng **Clinic SaaS Multi Tenant** để bán website/admin cho nhiều phòng khám.

Mỗi phòng khám là một tenant riêng.

Hệ thống có 3 lớp sản phẩm:

```txt
Owner Super Admin
  Quản lý nhiều phòng khám, gói dịch vụ, domain, template, billing, monitoring.

Clinic Admin
  Admin riêng từng phòng khám, quản lý website settings, slider, dịch vụ, nhân sự, lịch hẹn, khách hàng, thanh toán, báo cáo.

Public Website
  Website ngoài domain riêng, render động theo tenant settings: logo, màu, template, slider, dịch vụ, booking.
```

---

## 3. Có cần Lead Agent không?

Có. Nên có **Lead / Orchestrator Agent**.

Các agent hiện tại là agent chuyên môn: Architect, Figma UI, Frontend, Backend, Database, DevOps, QA, Documentation. Nhưng phải có Lead Agent để điều phối, nếu không mỗi agent dễ tự làm một hướng.

Lead Agent làm nhiệm vụ:

```txt
1. Đọc source of truth
2. Tóm tắt yêu cầu owner
3. Chia task cho agent phù hợp
4. Viết success criteria
5. Tạo/cập nhật docs/current-task.md
6. Tạo/cập nhật temp/plan.md
7. Review plan
8. Cho agent chuyên môn implement
9. Bắt QA verify
10. Bắt Documentation Agent cập nhật docs
11. Report lại owner
```

Flow agent nên là:

```txt
Owner
  ↓
Lead / Orchestrator Agent
  ↓
Architect Agent
  ↓
Figma UI Agent / Frontend Agent / Backend Agent / Database Agent / DevOps Agent
  ↓
QA Agent
  ↓
Documentation Agent
```

---

## 4. File support đang dùng trong repo

### 4.1 `docs/agent-playbook.md`

```md
# Agent Playbook / Orchestrator Checklist - Clinic SaaS

Bạn là Lead Agent cho project Clinic SaaS Multi Tenant.

## Vai trò

Bạn không phải agent code chính. Bạn là người điều phối toàn bộ agent team.

Bạn phải:
- Đọc AGENTS.md
- Đọc clinic_saas_report.md
- Đọc architech.txt
- Đọc docs/current-task.md nếu có
- Đọc Figma UI/FigJam/PDF export nếu task liên quan UI/architecture
- Biến yêu cầu owner thành success criteria rõ ràng
- Chọn agent phù hợp
- Bắt agent tạo plan trước khi implement
- Review plan
- Chia task lớn thành task nhỏ
- Kiểm tra tenant isolation
- Kiểm tra service boundary
- Kiểm tra UI có bám Figma không
- Kiểm tra test/verify sau khi implement
- Cập nhật documentation thông qua Documentation Agent

## Agent team

- Architect Agent: architecture, service boundary, data ownership, tenant isolation.
- Figma UI Agent: đọc Figma và chuyển UI thành component.
- Frontend Agent: Vue apps, routing, layout, state, API client.
- Backend Agent: .NET microservices theo Clean Architecture.
- Database Agent: PostgreSQL, MongoDB, migration, index, seed.
- DevOps Agent: Docker, env, CI/CD, deploy.
- QA Agent: test plan, unit/integration/E2E/manual test.
- Documentation Agent: README, setup guide, architecture docs, current-task.

## Quy trình bắt buộc

1. Understand
   - Tóm tắt yêu cầu owner.
   - Xác định task thuộc Product, UI, Frontend, Backend, Database, DevOps, QA hay Docs.

2. Scope
   - Viết success criteria.
   - Viết out-of-scope.
   - Xác định file/module dự kiến bị ảnh hưởng.

3. Plan
   - Gọi Architect Agent nếu task ảnh hưởng architecture/service/data flow.
   - Gọi Figma UI Agent nếu task ảnh hưởng UI.
   - Gọi Frontend/Backend/Database/DevOps Agent tùy phần implement.
   - Tạo hoặc cập nhật temp/plan.md.

4. Approval
   - Với task lớn hoặc đổi architecture, chờ owner duyệt plan.
   - Nếu owner yêu cầu làm trực tiếp, vẫn phải ghi plan ngắn vào temp/plan.md.

5. Implement
   - Cho agent chuyên môn implement.
   - Không để agent khác sửa lẫn phần không thuộc trách nhiệm.

6. Verify
   - Chạy lint/build/test nếu có.
   - Với UI, đối chiếu Figma.
   - Với backend/database, kiểm tra tenant_id, permission, API contract.
   - Với multi-tenant data, không được thiếu tenant context.

7. Report
   - Báo owner:
     - Đã làm gì
     - Sửa file nào
     - Kiểm tra gì
     - Còn thiếu gì
     - Bước tiếp theo là gì

## Luật quan trọng

- Không tự tạo Figma file mới nếu owner đã đưa link file cũ.
- Không đổi architecture nếu chưa cập nhật architecture docs/FigJam.
- Không tạo tenant-owned data mà thiếu tenant_id.
- Không dùng secret thật.
- Không xóa file cũ nếu owner chưa yêu cầu.
- Không implement business logic trong controller.
- Không để endpoint public trả NotImplementedException.
- Không merge task khi chưa verify.
```

### 4.2 `docs/current-task.md`

```md
# Current Task

## Task
Khởi tạo monorepo skeleton cho Clinic SaaS Multi Tenant.

## Goal
Tạo structure frontend apps, shared packages, backend services, infrastructure và docs.

## Source of Truth
- UI Figma: https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?node-id=0-1&p=f&t=5lnVCBz1BThNHg48-0
- Architecture FigJam: https://www.figma.com/board/zVIS2cgoNqwC21lZbpApjp/Clinic-SaaS-Architecture---Source-of-Truth?t=L0tWxOID86LOXPh0-0
- Technical Architecture FigJam: https://www.figma.com/board/j4vDRWSIRSckcAYvXHocMc/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?t=L0tWxOID86LOXPh0-0
- Report: clinic_saas_report.md
- Agent rules: AGENTS.md, docs/agent-playbook.md

## Success Criteria
- Có frontend/apps/public-web
- Có frontend/apps/clinic-admin
- Có frontend/apps/owner-admin
- Có frontend/packages/ui
- Có frontend/packages/design-tokens
- Có frontend/packages/api-client
- Có frontend/packages/shared-types
- Có frontend/packages/config
- Có backend/services skeleton
- Có backend/shared skeleton
- Có infrastructure/docker
- Có docker-compose cơ bản
- Có README chạy local
- Chưa implement business logic sâu
- Mọi tenant-owned concept phải có tenant context strategy

## Assigned Agents
- Lead Agent: điều phối
- Architect Agent: review structure/service boundaries
- Figma UI Agent: đối chiếu Figma screens
- Frontend Agent: create frontend skeleton
- Backend Agent: create backend skeleton
- Database Agent: database plan
- DevOps Agent: docker compose/env
- QA Agent: verify build commands
- Documentation Agent: update docs

## Verification
- package manager install chạy được
- frontend apps có placeholder route
- backend solution restore được nếu đã tạo
- docker compose config valid
- docs ghi rõ cách chạy local
```

---

## 5. Thứ tự khởi tạo project

Không nên bảo Codex code full app ngay. Thứ tự hợp lý:

```txt
1. Lead Agent đọc source of truth
2. Lead Agent tạo docs/current-task.md và temp/plan.md
3. Architect Agent xác nhận architecture + service boundaries
4. Tạo monorepo skeleton
5. Tạo design tokens từ Figma
6. Tạo shared UI components
7. Tạo Owner Super Admin shell
8. Tạo Clinic Admin shell
9. Tạo Public Website shell
10. Tạo API Gateway + Identity + Tenant skeleton
11. Tạo Website CMS + Template + Domain skeleton
12. Tạo Booking + Catalog skeleton
13. Tạo Database migration/collections plan
14. Tạo Docker Compose local
15. Tạo QA test plan
16. Cập nhật documentation
```

---

## 6. Nên code UI từ màn nào?

Vì đây là SaaS bán cho nhiều phòng khám, nên bắt đầu từ platform trước.

```txt
1. Design System / Design Tokens
2. Shared UI Components
3. Owner Super Admin Layout
4. Owner Super Admin Dashboard
5. Create Clinic Tenant Flow
6. Domain Setup Flow
7. Template Selection Flow
8. Clinic Admin Layout
9. Clinic Website Settings
10. Clinic Admin Dashboard
11. Public Homepage Dynamic
12. Public Booking Flow
13. Mobile Responsive
```

Lý do:
- Owner Super Admin là lõi kinh doanh SaaS.
- Create Tenant + Domain + Template là luồng bán hàng/vận hành chính.
- Clinic Admin là phần khách hàng dùng sau khi được onboard.
- Public Website và Booking dùng dữ liệu tenant config, nên làm sau khi đã rõ cấu hình tenant.

---

## 7. Monorepo structure đề xuất

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
  current-task.md

temp/
  plan.md
```

---

## 8. Tech stack

Frontend:
```txt
Vue 3
Vite
TypeScript
Tailwind CSS hoặc Tailwind-style design tokens
Pinia
Vue Router
Shared UI package
Tenant-aware API client
```

Backend:
```txt
.NET service architecture; kiểm tra stable/LTS runtime tại thời điểm implementation thật
Microservices
Clean Architecture trong từng service
API Gateway/BFF
PostgreSQL
MongoDB
Redis
Kafka/Event Bus
Future Socket/SignalR
```

---

## 9. Backend service boundaries

```txt
identity-service
  Auth, RBAC, users, roles, tenant claims

tenant-service
  Clinics/tenants, plans, modules, tenant lifecycle

website-cms-service
  Settings, menu, slider, homepage blocks, page content

template-service
  Template library, apply full/style/content modes

domain-service
  Subdomain, custom domain, DNS verification, SSL status, publish status

booking-service
  Appointments, slots, booking status

catalog-service
  Services, pricing, staff, working schedule

customer-service
  Customers, records metadata

billing-service
  Subscriptions, invoices, renewal, payment status

report-service
  KPIs, analytics, dashboards

notification-service
  Email, SMS, Zalo-ready notification workflows

realtime-gateway
  Future live booking, notification, dashboard updates
```

---

## 10. Tenant isolation rules

Bắt buộc:

```txt
- Mọi tenant-owned table/collection có tenant_id.
- Mọi request Clinic Admin phải resolve được tenant_id.
- Public Website resolve tenant từ domain/subdomain.
- Owner Super Admin mới được cross-tenant.
- Không service nào query tenant-owned data nếu thiếu tenant context.
- Không dùng global cache key nếu không prefix bằng tenant.
```

Ví dụ cache key:

```txt
tenant:{tenantId}:website-settings
tenant:{tenantId}:homepage
tenant:{tenantId}:services
tenant:{tenantId}:booking-slots:{date}
```

---

## 11. Việc cần làm tiếp

```txt
1. Giữ file này và prompt file đồng bộ với `clinic_saas_report.md`.
2. Không tạo thêm root-level `apps/`, `packages/`, `services/`.
3. Dùng `docs/agent-playbook.md` làm checklist agent chung
4. Cập nhật docs/current-task.md
5. Cho Codex chạy prompt theo AGENTS.md và docs/agent-playbook.md
6. Chờ Codex tạo/cập nhật temp/plan.md khi là implementation task
7. Duyệt plan nếu task cần duyệt
8. Bắt đầu bước implementation tiếp theo trong structure hiện tại
```
