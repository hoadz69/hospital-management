# Codex Prompts - Clinic SaaS Multi Tenant

Dùng cùng với:

```txt
UI Figma: https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?node-id=0-1&p=f&t=5lnVCBz1BThNHg48-0
UI Figma short: https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC
Architecture FigJam: https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT
Report: docs/clinic_saas_report_vi.md
Agent rules: AGENTS.md
Lead rules: LEAD_AGENT.md
```

---

## 0. Kiểm tra Figma MCP/account

```txt
Kiểm tra Figma MCP hiện tại.

Yêu cầu:
1. Chạy mcp list nếu môi trường hỗ trợ.
2. Xác định có server figma không.
3. Xác định có truy cập được file UI Figma này không:
https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?node-id=0-1&p=f&t=5lnVCBz1BThNHg48-0

Nếu không truy cập được, hãy báo rõ:
- MCP chưa login
- login sai tài khoản
- file chưa được share
- hoặc thiếu quyền edit/read

Không tạo file Figma mới.
```

---

## 1. Khởi động Lead Agent

```txt
Bạn là Lead / Orchestrator Agent của project Clinic SaaS Multi Tenant.

Đọc các source of truth:
- AGENTS.md
- LEAD_AGENT.md
- docs/clinic_saas_report_vi.md
- docs/current-task.md nếu có
- UI Figma: https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?node-id=0-1&p=f&t=5lnVCBz1BThNHg48-0
- Architecture FigJam: https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT

Nhiệm vụ hiện tại:
Khởi tạo project skeleton theo architecture và UI đã thiết kế.

Chưa code ngay.

Hãy làm:
1. Tóm tắt lại yêu cầu owner
2. Chia task cho từng agent
3. Viết success criteria
4. Viết out-of-scope
5. Tạo hoặc cập nhật docs/current-task.md
6. Tạo temp/plan.md
7. Đề xuất thứ tự implement MVP

Không tạo Figma file mới.
Không đổi architecture nếu chưa ghi rõ trong plan.
```

---

## 2. Đọc source of truth và tóm tắt

```txt
Đầu tiên, chưa code.

Hãy đọc:
- Figma UI Design
- FigJam Architecture
- docs/clinic_saas_report_vi.md
- AGENTS.md
- LEAD_AGENT.md

Sau đó tóm tắt lại project theo các mục:
1. Mục tiêu sản phẩm
2. Các loại user
3. Các app frontend cần có
4. Các service backend cần có
5. Các database/cache/event bus cần có
6. Các màn UI cần code trước
7. Service boundaries
8. Tenant isolation strategy
9. Thứ tự triển khai MVP

Nếu không đọc được Figma/FigJam, hãy báo lý do và dùng report/docs trong repo làm fallback.
```

---

## 3. Tạo monorepo skeleton

```txt
Duyệt plan. Bắt đầu tạo monorepo skeleton cho Clinic SaaS.

Yêu cầu structure:

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

Chỉ tạo skeleton, README, package config, solution files, docker compose cơ bản.
Chưa implement business logic sâu.
Sau khi làm xong, report file đã tạo/sửa và cách verify.
```

---

## 4. Tạo design tokens

```txt
Tạo package packages/design-tokens dựa trên Figma UI Design.

Cần có:
- colors
- typography
- spacing
- border radius
- shadows
- component size tokens
- status colors
- admin sidebar tokens
- public website theme tokens
- template theme tokens: dental, eye, skin, pediatric, general

Xuất thành:
- TypeScript constants
- Tailwind config preset
- CSS variables

Không hard-code token trong từng component.
```

---

## 5. Tạo shared UI components

```txt
Tạo package packages/ui cho Vue 3.

Dựa trên Figma, tạo component dùng chung:
- AppButton
- AppInput
- AppSelect
- AppCard
- AppTable
- StatusPill
- MetricCard
- AdminSidebar
- AdminTopbar
- TemplateCard
- SliderPreviewCard
- BookingStep
- EmptyState
- Modal
- FormSection

Yêu cầu:
- Dùng TypeScript
- Dùng design tokens
- Không nối API thật
- Có mock/demo usage
- Bám layout, màu, spacing, radius theo Figma
```

---

## 6. Owner Super Admin app

```txt
Implement Owner Super Admin app shell trong apps/owner-admin.

Dựa theo Figma screens:
- 02 Super Admin Dashboard
- 03 Create Clinic Tenant Flow
- 04 Domain Setup Flow
- 05 Template Selection Flow

Routing cần có:
/dashboard
/clinics
/clinics/create
/domains
/templates
/plans
/billing
/monitoring
/support
/settings

Yêu cầu:
- Dùng shared UI components
- Dùng mock data
- Chưa nối backend
- Có sidebar/topbar/layout
- Có responsive cơ bản
- Không implement business logic sâu
```

---

## 7. Clinic Admin app

```txt
Implement Clinic Admin app shell trong apps/clinic-admin.

Dựa theo Figma screens:
- 06 Clinic Admin Dashboard
- 07 Clinic Website Settings
- Slider setup
- Services & pricing
- Staff
- Appointments
- Customers
- Payments
- Reports

Routing cần có:
/dashboard
/website-settings
/sliders
/templates
/services
/staff
/appointments
/customers
/payments
/reports

Yêu cầu:
- Dữ liệu mock phải tenant-scoped
- Có tenant context mock
- Dùng shared UI components
- Không cho Clinic Admin thấy dữ liệu tenant khác
```

---

## 8. Public Website app

```txt
Implement Public Website app trong apps/public-web.

Dựa theo Figma screens:
- 08 Public Homepage Dynamic
- 09 Public Booking Flow
- 10 Mobile Responsive Set

Routing cần có:
/
/services
/services/:slug
/staff
/booking
/contact

Website phải tenant-aware:
- lấy tenant từ domain/subdomain mock
- load theme config
- load logo
- load slider
- load services
- load booking config

Dùng mock tenant config trước.
```

---

## 9. Backend skeleton phase 1

```txt
Tạo backend skeleton phase 1 cho:
- api-gateway
- identity-service
- tenant-service

Mỗi service dùng Clean Architecture:
- Api
- Application
- Domain
- Infrastructure

Implement cơ bản:
- tenant context middleware
- auth placeholder
- RBAC placeholder
- tenant CRUD placeholder
- plan/module assignment placeholder
- OpenAPI docs
- PostgreSQL connection setup

Không đưa business logic vào controller.
Không dùng secret thật.
Không để endpoint public trả NotImplementedException.
```

---

## 10. Backend skeleton phase 2

```txt
Tạo backend skeleton phase 2 cho:
- website-cms-service
- template-service
- domain-service

Website CMS dùng MongoDB cho:
- tenant settings
- menu
- homepage blocks
- slider
- page content

Template service quản lý:
- dental
- eye
- skin
- pediatric
- general
- custom
- apply full
- apply style only
- apply content only

Domain service quản lý:
- default subdomain
- custom domain
- DNS verification placeholder
- SSL status placeholder
- publish status

Tất cả dữ liệu tenant-owned phải có tenant_id.
```

---

## 11. Backend skeleton phase 3

```txt
Tạo backend skeleton phase 3 cho:
- booking-service
- catalog-service
- customer-service
- billing-service
- report-service
- notification-service
- realtime-gateway

Catalog service:
- services
- pricing
- staff
- working schedule

Booking service:
- appointment
- slots
- booking status
- tenant isolation
- event AppointmentCreated

Billing service:
- subscription
- invoice
- renewal status

Report service:
- KPI query models
- dashboard stats placeholder

Notification service:
- email/SMS/Zalo-ready placeholder

Realtime gateway:
- future SignalR/WebSocket placeholder

PostgreSQL cho transactional data.
MongoDB cho flexible CMS/report snapshots nếu cần.
Redis placeholder cho cache/slot lock.
Kafka placeholder cho domain events.
```

---

## 12. Docker/DevOps

```txt
Tạo local Docker Compose cho project.

Cần có:
- PostgreSQL
- MongoDB
- Redis
- Kafka
- Kafka UI nếu nhẹ
- optional mailhog
- optional reverse proxy local

Chuẩn bị env structure:
- .env.example
- apps env
- services env

Không dùng secret thật.
Không hard-code credentials production.
Có README hướng dẫn chạy local.
```

---

## 13. QA verify

```txt
QA Agent, hãy tạo test plan và verify checklist.

Focus:
- tenant isolation
- auth/permission
- routing frontend
- UI bám Figma
- booking flow
- template apply modes
- domain verification flow
- admin/public consistency
- backend service boundaries

Nếu có thể, chạy:
- install
- lint
- typecheck
- build
- test
- docker compose config

Report lại:
- pass/fail
- lỗi
- file liên quan
- bước sửa tiếp theo
```

---

## 14. Documentation Agent

```txt
Documentation Agent, cập nhật tài liệu sau khi skeleton được tạo.

Cần cập nhật:
- README.md
- AGENTS.md nếu thiếu
- LEAD_AGENT.md nếu thiếu
- docs/current-task.md
- docs/setup/local-development.md
- docs/architecture/overview.md
- docs/architecture/service-boundaries.md
- docs/architecture/tenant-isolation.md

Nội dung phải khớp với Figma UI, FigJam Architecture và clinic_saas_report_vi.md.
```

---

## 15. Report sau mỗi bước

```txt
Report lại theo format:

1. Đã làm gì
2. File đã tạo/sửa
3. Cách verify
4. Kết quả verify
5. Còn thiếu hoặc bị chặn gì
6. Bước tiếp theo đề xuất

Không chuyển sang bước tiếp theo nếu chưa report.
```

---

## 16. Nếu Codex bị loạn

```txt
Dừng implement.

Hãy đọc lại:
- AGENTS.md
- LEAD_AGENT.md
- docs/current-task.md
- docs/clinic_saas_report_vi.md

Sau đó tóm tắt:
1. Task hiện tại là gì
2. File nào đã sửa
3. Có lệch architecture không
4. Có thiếu tenant isolation không
5. Cần rollback hay tiếp tục

Không sửa thêm file trước khi trả lời.
```
