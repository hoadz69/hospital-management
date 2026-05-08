# Báo Cáo Clinic SaaS Multi-Tenant Platform

File này là báo cáo sản phẩm/kiến trúc chính của repo `hospital-management`. Đây không phải file report sau mỗi task; report tiến độ/handoff sau task nằm ở `docs/current-task.md` và câu trả lời cuối của agent.

## 1. Mục Tiêu Sản Phẩm

Clinic SaaS / Hospital Management Platform là nền tảng SaaS cho phép chủ platform bán website và hệ thống quản trị cho nhiều phòng khám/bệnh viện.

Mỗi phòng khám là một tenant riêng. Chủ platform dùng Owner Super Admin để tạo tenant, cấu hình gói dịch vụ, domain, template, publish website và theo dõi vận hành. Từng phòng khám dùng Clinic Admin để quản lý nội dung website, slider, dịch vụ, nhân sự, lịch hẹn, khách hàng, thanh toán và báo cáo.

Public Website của mỗi tenant render theo dữ liệu riêng: logo, màu sắc, template, slider, dịch vụ, bác sĩ/nhân sự, form đặt lịch và domain.

## 2. Source Of Truth

- Báo cáo chính: `clinic_saas_report.md`
- Ghi chú kiến trúc: `architech.txt`
- Handoff task: `docs/current-task.md`
- Luật agent: `AGENTS.md`
- Luật Claude Code: `CLAUDE.md`
- PDF export từ Figma/FigJam:
  - `Clinic SaaS Architecture - Source of Truth.pdf`
  - `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`
- Figma/FigJam architecture:
  - `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth`
  - `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture`

## 3. Kiến Trúc Tổng Thể

Luồng tổng quát:

```txt
Frontend Apps -> API Gateway/BFF -> Microservices -> Databases / Cache / Event Bus
```

Ba frontend chính:

1. `public-web`: website bệnh nhân nhìn thấy.
2. `clinic-admin`: portal quản trị riêng của từng phòng khám.
3. `owner-admin`: portal quản trị toàn platform.

Backend chia theo bounded context. Mỗi service dùng Clean Architecture để business logic không nằm trong controller hoặc infrastructure.

## 4. Tech Stack Định Hướng

### Frontend

- Vue 3
- Vite
- TypeScript
- Pinia
- Vue Router
- TanStack Query hoặc composable query layer nếu cần cache API
- Tailwind CSS hoặc utility/design-token system tương đương
- Shared UI components map theo Figma
- API client có tenant context

Định hướng monorepo:

```txt
apps/
  public-web/
  clinic-admin/
  owner-admin/
packages/
  ui/
  design-tokens/
  api-client/
  shared-types/
```

### Backend

- .NET service architecture
- ASP.NET Core
- Clean Architecture
- CQRS/use-case pattern khi có lợi
- MediatR hoặc explicit application use-case pattern
- Entity Framework Core hoặc Dapper tùy service; nếu dùng SQL migration thì phải giữ convention snake_case
- PostgreSQL cho dữ liệu quan hệ/giao dịch
- MongoDB cho CMS, layout JSON, template config
- Redis cho cache, domain mapping, rate limit, slot lock
- Kafka/Event Bus cho workflow bất đồng bộ khi scale
- SignalR/WebSocket gateway cho realtime về sau

Ghi chú version: `.NET 11` là định hướng ban đầu trong tài liệu. Trước khi triển khai thật, agent phải kiểm tra runtime stable/LTS tại thời điểm code và ghi rõ quyết định.

Backend nên chia service theo thư mục:

```txt
services/
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
```

## 5. Service Boundaries

| Service | Trách nhiệm |
|---|---|
| Identity Service | Đăng nhập, phân quyền, RBAC, token, claim tenant. |
| Tenant Service | Tenant, plan, module enablement, lifecycle phòng khám. |
| Website CMS Service | Logo, màu, menu, slider, page content, content block. |
| Template Service | Thư viện template, apply mode, template config. |
| Domain Service | Subdomain, custom domain, DNS verification, SSL, publish state. |
| Booking Service | Lịch hẹn, slot, booking status, public booking flow. |
| Catalog Service | Dịch vụ, bảng giá, nhân sự, lịch làm việc. |
| Customer Service | Hồ sơ khách hàng, metadata bệnh án/ghi chú/tệp đính kèm. |
| Billing Service | Gói thuê bao, hóa đơn, thanh toán, gia hạn. |
| Report Service | KPI, dashboard, analytics, báo cáo cross-tenant cho Owner. |
| Notification Service | Email, SMS, Zalo-ready notification pipeline. |
| Realtime Gateway | SignalR/WebSocket cho lịch hẹn và dashboard realtime. |

### Mapping Theo Technical Architecture PDF

- `API Gateway / BFF` nhận request từ Owner Super Admin Portal, Clinic Admin Portal và Public Website.
- `Identity Service`, `Tenant Service`, `Domain Service`, `Booking Service`, `Website CMS Service`, `Template Service`, `Catalog Service`, `Customer Service`, `Billing Service`, `Report Service`, `Notification Service` là các service chính được board technical xác nhận.
- `Realtime Gateway` là nhánh tương lai cho socket clients: live booking, notifications, dashboards.
- `CDN / Edge / WAF` đứng trước frontend/public traffic.
- `Kafka / Event Bus` dùng cho domain events và async workflows giữa services.
- `PostgreSQL` là relational source of truth.
- `MongoDB` lưu CMS content, template config, page JSON.
- `Redis` dùng cache, session, rate limit, tenant config cache.

## 6. Clean Architecture Chuẩn

Mỗi service nên có cấu trúc:

```txt
ServiceName/
  src/
    ServiceName.Api/
    ServiceName.Application/
    ServiceName.Domain/
    ServiceName.Infrastructure/
  tests/
    ServiceName.UnitTests/
    ServiceName.IntegrationTests/
```

Layer rules:

- API: controller/minimal API, middleware, auth, mapping, OpenAPI.
- Application: use case, command/query, DTO, validation, transaction boundary.
- Domain: entity, value object, domain event, business rule.
- Infrastructure: database, Redis, Kafka, external providers.

## 7. Database Strategy

PostgreSQL là source of truth cho dữ liệu có quan hệ và transaction:

- tenants
- users
- roles
- permissions
- plans
- subscriptions
- domains
- appointments
- services
- staff
- invoices
- payments

MongoDB dùng cho dữ liệu linh hoạt:

- homepage layout JSON
- slider configuration
- page content
- template configuration
- theme configuration
- form schema
- custom blocks
- draft/published content versions

Redis dùng cho:

- tenant config cache
- domain mapping cache
- rate limiting
- temporary booking slot lock
- session/cache nếu cần
- pub/sub tạm thời nếu chưa dùng Kafka sâu

Kafka/Event Bus dùng cho event:

- TenantCreated
- DomainVerified
- WebsitePublished
- AppointmentCreated
- AppointmentCancelled
- InvoicePaid
- SubscriptionExpired
- TemplateApplied
- NotificationRequested

## 8. Multi-Tenant Rules

Mọi request phải resolve tenant context trước khi truy cập tenant-owned data.

Tenant có thể resolve từ:

- domain: `nhakhoaabc.vn`
- subdomain: `nhakhoaabc.clinicos.vn`
- internal header: `X-Tenant-Id`
- JWT claim: `tenant_id`

Rule bắt buộc:

```txt
Không service nào được query tenant-owned data khi thiếu tenant_id.
Clinic Admin chỉ được truy cập tenant của họ.
Owner Super Admin mới được cross-tenant.
```

## 9. Domain Flow

1. Owner tạo tenant.
2. Hệ thống cấp subdomain mặc định, ví dụ `nhakhoaabc.clinicos.vn`.
3. Nếu khách mua custom domain, Owner nhập domain.
4. Domain Service hiển thị DNS record cần trỏ.
5. Chủ domain cấu hình DNS.
6. Domain Service verify DNS.
7. Hệ thống cấp SSL.
8. Website được publish.
9. Domain mapping cache vào Redis.

## 10. Template System

Template là config, không phải codebase riêng cho từng phòng khám.

```json
{
  "templateKey": "dental",
  "theme": {
    "primary": "#0E7C86",
    "secondary": "#2563EB",
    "radius": "8px"
  },
  "modules": {
    "heroSlider": true,
    "services": true,
    "staff": true,
    "pricing": true,
    "reviews": false,
    "news": false
  },
  "defaultContent": {
    "heroTitle": "Nụ cười khỏe đẹp cho cả gia đình",
    "ctaText": "Đặt lịch ngay"
  }
}
```

Apply modes:

- Apply full template: đổi style, layout, content, slider, service mẫu.
- Apply style only: chỉ đổi màu, typography, spacing, radius, layout.
- Apply content only: chỉ nạp nội dung mẫu, không đổi style hiện tại.

### Template Library Theo Source Of Truth PDF

Global Template Library có các template ban đầu:

- Eye clinic template.
- Dental template.
- Pediatric template.
- General clinic template.
- Dermatology template.
- Custom template.

Template Config quản lý:

- Title and description.
- Slide image.
- CTA button and link.
- Schedule visibility.
- Homepage modules.
- Apply style only.
- Apply full template.
- Apply content only.

## 10.1 Product Flow Theo Source Of Truth PDF

### Owner Super Admin

Owner Super Admin quản lý các nhóm:

- Dashboard.
- Monitoring and Support.
- Create Clinic Flow.
- Global Template Library.
- Domain Management.
- Tenant Management.

Create Clinic Flow gồm:

1. Input clinic info.
2. Choose package.
3. Create default subdomain.
4. Choose template.
5. Publish and send login.

Tenant Management gồm:

- Create tenant.
- Assign owner account.
- Enable modules by plan.
- Suspend or renew.

Domain Management gồm:

- Default subdomain.
- Custom domain.
- DNS verification.
- SSL certificate.
- Publish website.

Plan and Billing gồm:

- Starter plan.
- Growth plan.
- Premium plan.
- Invoice and renewal.

### Clinic Admin

Clinic Admin quản lý các nhóm:

- Dashboard.
- Appointments.
- Homepage Slider.
- Website Settings.
- Staff and Schedule.
- Services and Pricing.
- Payments and Reports.
- Customers and Records.

Website Settings gồm:

- Logo.
- Brand colors.
- Clinic info.
- Menu and content.

Homepage Slider liên kết tới Homepage và quản lý:

- Title and description.
- Slide image.
- CTA button and link.
- Schedule visibility.
- Homepage modules.

### Public Website

Public Website gồm:

- Homepage.
- Booking Flow.
- Contact and Map.
- Staff Pages.
- Service Pages.
- Mobile Website.

## 11. Realtime / Socket Tương Lai

Realtime chưa cần làm ngay từ đầu, nhưng kiến trúc phải chừa chỗ cho:

- Lịch hẹn mới hiện realtime trong Clinic Admin.
- Thông báo thanh toán.
- Dashboard live.
- Nhắc lịch.
- Chat/tư vấn nếu sau này làm.
- Trạng thái online của nhân sự.

Hướng kỹ thuật:

- SignalR nếu backend .NET là runtime chính.
- WebSocket Gateway riêng nếu cần tách scale.
- Redis pub/sub hoặc Kafka event đẩy sang Realtime Gateway.

## 12. Figma Và FigJam

Các board do owner cung cấp:

- Architecture Source of Truth:
  `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth?node-id=0-1&p=f&t=QK2zZcH4us1dCsN3-0`
- Technical Architecture:
  `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?node-id=0-1&p=f&t=apHcjCTAhX1BfVlv-0`
- Figma Design UI cũ được owner cung cấp trong tài liệu:
  `https://www.figma.com/design/Mz2oB5doTl6alla1KwTUWL`

Figma Design UI dự kiến có các nhóm màn hình:

- 00 Architecture note
- 01 System Concept
- 02 Super Admin
- 03 Create Tenant
- 04 Domain
- 05 Template
- 06 Clinic Admin
- 07 Website Settings
- 08 Public Homepage
- 09 Booking
- 10 Mobile

Quy ước:

- Figma/FigJam là source of truth cho UI, module map, service map và data flow.
- Khi thay đổi UI layout, module, service boundary, data flow hoặc domain flow, phải cập nhật tài liệu tương ứng.
- Figma MCP đã được cấu hình trong `.mcp.json` và Codex global config.
- Hai PDF export từ FigJam đã được đọc ngày 2026-05-08 và dùng để cập nhật báo cáo này.
- Figma MCP đã cài và OAuth thành công cho Codex, nhưng hiện bị quota Starter plan. Khi quota có lại, đọc trực tiếp board để đối chiếu với PDF export.

### Nội Dung Đã Xác Nhận Từ PDF Export

`Clinic SaaS Architecture - Source of Truth.pdf` xác nhận product map:

- Clinic SaaS Platform gồm Owner Super Admin và Clinic Tenant.
- Owner Super Admin quản lý tenant, domain, template, plan/billing, monitoring/support và create clinic flow.
- Clinic Admin quản lý dashboard, appointments, website settings, homepage slider, staff/schedule, services/pricing, payments/reports, customers/records.
- Public Website gồm homepage, booking flow, contact/map, staff pages, service pages, mobile website.
- Template system gồm global template library, template config và ba apply modes.

`Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf` xác nhận technical map:

- Figma UI Design và FigJam Architecture là input cho Codex Agents.
- Codex Agents gồm Architect/Backend/Frontend/Database/DevOps/QA.
- Users đi qua CDN/Edge/WAF tới frontend apps.
- Frontend apps gồm Owner Super Admin Portal, Clinic Admin Portal, Public Website.
- API Gateway/BFF đứng trước microservices.
- Mỗi .NET service dùng Clean Architecture: API Layer -> Application Layer -> Domain Layer, cùng Infrastructure Layer.
- Data layer gồm PostgreSQL, MongoDB, Redis, Kafka/Event Bus.
- Realtime Gateway là socket future cho live booking, notifications, dashboards.

## 13. Agent Plan Cho Codex Và Claude Code

### Architect Agent

Giữ service boundary, tenant isolation, data ownership và FigJam alignment.

Prompt nền:

```txt
You are the Architect Agent for a multi-tenant Clinic SaaS platform.
Use FigJam architecture and clinic_saas_report.md as source of truth.
Map each feature to frontend app, API Gateway/BFF, backend service, database, cache, and event bus.
Never create tenant-owned data access without tenant isolation.
```

### Figma UI Agent

Đọc Figma, sinh Vue components và design tokens. Giữ layout, spacing, colors, radius, typography và component hierarchy khớp Figma.

Prompt nền:

```txt
You are the Figma UI Agent.
Read Figma/FigJam when tools are available and convert screens into Vue 3 + Vite + TypeScript components.
Use shared design tokens.
Keep layout, spacing, colors, radius, typography, and component hierarchy aligned with Figma.
Create reusable components for sidebar, topbar, table, button, form field, status pill, template card, and booking steps.
```

### Frontend Agent

Tạo `public-web`, `clinic-admin`, `owner-admin`, routing, guard, API client, tenant context.

### Backend Agent

Tạo .NET services theo Clean Architecture. Không đưa business logic vào controller.

### Database Agent

Thiết kế PostgreSQL schema, MongoDB collections, indexes, migrations và seed data. Mọi tenant-owned table/collection phải có tenant index phù hợp.

### DevOps Agent

Chuẩn bị local Docker Compose, env structure, CI/CD, deploy, rollback, domain verification và SSL automation.

### QA Agent

Kiểm tenant isolation, auth permissions, booking flow, template apply modes, domain verification và admin/public consistency.

### Documentation Agent

Cập nhật README, architecture docs, setup guide, deployment guide và troubleshooting.

Chi tiết prompt/ownership từng agent nằm trong `docs/agent-playbook.md` và `.claude/agents/*.md`.

## 14. Thứ Tự Triển Khai Đề Xuất

1. Chuẩn hóa repo shell và tài liệu nguồn.
2. Chuẩn hóa các file script/compose/rules theo Clinic SaaS.
3. Tạo monorepo structure.
4. Trích design tokens từ Figma.
5. Tạo shared UI package.
6. Tạo shell cho Owner Super Admin.
7. Tạo shell cho Clinic Admin.
8. Tạo shell cho Public Website.
9. Tạo API Gateway/BFF.
10. Tạo Identity + Tenant Service.
11. Tạo Website CMS + Template Service.
12. Tạo Domain Service.
13. Tạo Booking + Catalog Service.
14. Tạo Billing + Report Service.
15. Thêm PostgreSQL/MongoDB/Redis/Kafka Docker Compose.
16. Thêm test và tài liệu vận hành.

## 15. Việc Cần Owner Cung Cấp Sau

- Tên domain/subdomain mặc định của platform.
- Database/server/dev/staging/prod connection.
- Chính sách auth: tự xây Identity hay dùng provider ngoài.
- Danh sách module MVP.
- Link Figma UI design nếu khác hai board architecture.
- Quyết định runtime .NET stable/LTS cuối cùng.
