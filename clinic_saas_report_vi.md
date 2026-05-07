# Báo Cáo Clinic SaaS Multi-Tenant Platform

Tài liệu này là source of truth hiện tại cho repo `hospital-management`. Repo được copy từ project cũ nên mọi thông tin cũ liên quan `ez-sales-bot`, chatbot, Telegram, merchant knowledge upload, database production cũ, hoặc server cũ đều không còn được coi là đúng.

## 1. Mục Tiêu Sản Phẩm

Đây không phải website phòng khám đơn lẻ. Đây là nền tảng SaaS để chủ platform bán website và hệ thống admin cho nhiều phòng khám.

Mỗi phòng khám là một tenant riêng. Chủ platform có Owner Super Admin để tạo tenant, gán gói dịch vụ, cấu hình domain, chọn template, publish website và theo dõi vận hành. Mỗi phòng khám có Clinic Admin để quản lý nội dung website, slider, dịch vụ, nhân sự, lịch hẹn, khách hàng, thanh toán và báo cáo.

Public Website của từng tenant render theo dữ liệu riêng: logo, màu sắc, template, slider, dịch vụ, bác sĩ/nhân sự, form đặt lịch và domain.

## 2. Kiến Trúc Tổng Thể

Luồng tổng quát:

```txt
Frontend Apps -> API Gateway/BFF -> Microservices -> Databases / Cache / Event Bus
```

Ba frontend chính:

1. `public-web`: website bệnh nhân nhìn thấy.
2. `clinic-admin`: portal quản trị riêng của từng phòng khám.
3. `owner-admin`: portal quản trị toàn platform.

Backend chia theo bounded context. Mỗi service dùng Clean Architecture để tránh business logic nằm trong controller hoặc infrastructure.

## 3. Tech Stack Định Hướng

### Frontend

- Vue 3
- Vite
- TypeScript
- Tailwind CSS hoặc utility/design-token system tương đương
- Pinia
- Vue Router
- API client có tenant context
- Shared UI components map theo Figma

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
- PostgreSQL cho dữ liệu quan hệ/giao dịch
- MongoDB cho CMS, layout JSON, template config
- Redis cho cache, domain mapping, rate limit, slot lock
- Kafka/Event Bus cho workflow bất đồng bộ khi scale
- SignalR/WebSocket gateway cho realtime về sau

Ghi chú version: `.NET 11` đang được ghi là định hướng yêu cầu ban đầu. Trước khi triển khai thật, agent phải kiểm tra runtime stable/LTS mới nhất tại thời điểm code và ghi rõ quyết định.

## 4. Service Boundaries

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

## 5. Clean Architecture Chuẩn

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

## 6. Database Strategy

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

## 7. Multi-Tenant Rules

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

## 8. Domain Flow

1. Owner tạo tenant.
2. Hệ thống cấp subdomain mặc định, ví dụ `nhakhoaabc.clinicos.vn`.
3. Nếu khách mua custom domain, Owner nhập domain.
4. Domain Service hiển thị DNS record cần trỏ.
5. Chủ domain cấu hình DNS.
6. Domain Service verify DNS.
7. Hệ thống cấp SSL.
8. Website được publish.
9. Domain mapping cache vào Redis.

## 9. Template System

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

## 10. Figma Và FigJam

Các file do owner cung cấp:

- Architecture Source of Truth:
  `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth?node-id=0-1&p=f&t=QK2zZcH4us1dCsN3-0`
- Technical Architecture:
  `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?node-id=0-1&p=f&t=apHcjCTAhX1BfVlv-0`

Quy ước:

- Figma/FigJam là source of truth cho UI, module map, service map và data flow.
- Khi thay đổi UI layout, module, service boundary, data flow hoặc domain flow, phải cập nhật lại tài liệu tương ứng.
- Figma MCP đã được cấu hình cho Codex global và `.mcp.json` project cho Claude-style MCP. Nếu tool chưa hiện, restart agent session.

## 11. Agent Plan Cho Codex Và Claude Code

### Architect Agent

Giữ service boundary, tenant isolation, data ownership và FigJam alignment.

Prompt nền:

```txt
You are the Architect Agent for a multi-tenant Clinic SaaS platform.
Use FigJam architecture and clinic_saas_report_vi.md as source of truth.
Map each feature to frontend app, API Gateway/BFF, backend service, database, cache, and event bus.
Never create tenant-owned data access without tenant isolation.
```

### Figma UI Agent

Đọc Figma, sinh Vue components và design tokens.

```txt
You are the Figma UI Agent.
Convert Figma screens into Vue 3 + Vite + TypeScript components.
Keep layout, spacing, colors, radius, typography, and component hierarchy aligned with Figma.
Create reusable sidebar, topbar, table, button, form field, status pill, template card, and booking step components.
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

## 12. Thứ Tự Triển Khai Đề Xuất

1. Chuẩn hóa repo shell và tài liệu nguồn.
2. Xóa/đổi tên các dấu vết project cũ.
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

## 13. Việc Cần Owner Cung Cấp Sau

- Tên domain/subdomain mặc định của platform.
- Database/server/dev/staging/prod connection.
- Chính sách auth: tự xây Identity hay dùng provider ngoài.
- Danh sách module MVP.
- Link Figma UI design nếu khác hai board architecture.
- Quyết định runtime .NET stable/LTS cuối cùng.
