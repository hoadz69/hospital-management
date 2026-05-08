# Coding Rules - Clinic SaaS

## ⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT CODE

- Đọc `AGENTS.md`, `CLAUDE.md`, `clinic_saas_report.md`, `architech.txt` và `docs/current-task.md`.
- Với implementation task, làm việc từ `temp/plan.md` đã được owner duyệt.
- Với task nhiều bước, plan phải nêu success criteria và verify check cho từng bước.
- Giữ thay đổi đúng phạm vi task.
- Không thêm secret, IP server thật, database password, token hoặc SSH key vào repo.
- Nếu gặp file chưa đúng hướng Clinic SaaS, cập nhật nội dung thay vì xóa trắng nếu owner chưa yêu cầu.

## Kiến Trúc

- Ưu tiên composition hơn inheritance.
- Hàm nhỏ, đơn nhiệm.
- Rõ ràng hơn là ẩn ý.
- Không dùng magic number; đặt tên hằng số có nghĩa.
- Không để TODO không có giải thích.
- Không thêm feature ngoài yêu cầu.
- Không thêm abstraction/configurability cho single-use code.
- Nếu code có thể viết đơn giản hơn đáng kể, phải simplify trước khi submit.
- Backend service dùng Clean Architecture:
  - Api
  - Application
  - Domain
  - Infrastructure
- Business logic không nằm trong controller.
- Domain không phụ thuộc infrastructure.
- Application layer sở hữu use case, command/query, validation và transaction boundary.
- Infrastructure layer sở hữu persistence, cache, event bus, external providers.

## Clean Architecture Anti-Patterns Cần Tránh

### 1. Cross-Service Infrastructure Injection

Không inject repository/infrastructure của service A vào service B. Nếu hai service cần cùng dữ liệu, mỗi service sở hữu repository/contract riêng đúng boundary. Nếu cần logic service khác, gọi qua API/event/contract.

### 2. Fire-And-Forget Background Work

Không dùng `Task.Run` tùy tiện trong request handler. Long-running work phải đi qua queue/channel/background service hoặc event bus.

### 3. Transaction Và Rethrow

Nếu cần persist trạng thái lỗi trước khi trả failure, không rethrow làm rollback mất trạng thái đã set. Transaction boundary phải được thiết kế rõ ở Application layer.

### 4. Transactional Command

Command ghi nhiều DB operations cần transaction boundary rõ ràng. Có thể dùng marker/interface tương đương `ITransactionalCommand` nếu project chọn CQRS pipeline.

### 5. Security Context

TenantId/UserId thiếu hoặc invalid phải fail rõ ràng. Không fallback sang hardcoded dev tenant/user.

### 6. Null Check

Data từ request, webhook, JSON, provider ngoài hoặc API response phải check null trước khi đọc property.

### 7. Exception Handling

Không catch-all trong handler nếu global middleware đã xử lý. Chỉ catch khi cần cleanup, map lỗi domain cụ thể hoặc persist trạng thái cụ thể.

## Multi-Tenant

- Tenant-owned data access phải có tenant context.
- Tenant-owned PostgreSQL table phải có `tenant_id` và index phù hợp.
- Tenant-owned MongoDB document phải có `tenant_id` và index phù hợp.
- Clinic Admin không được truy cập tenant khác.
- Owner Super Admin là role cross-tenant duy nhất.
- Public website resolve tenant từ domain/subdomain.

## Frontend

- Dùng Vue 3 + Vite + TypeScript.
- Dùng shared UI components và design tokens.
- UI phải bám Figma khi Figma source có sẵn.
- Không tự invent layout nếu Figma đã định nghĩa.
- Tenant context nằm ở API client/request layer.

## Backend

- Dùng async API cho I/O.
- Validate input ở application boundary.
- Ưu tiên explicit use case thay vì service quá lớn.
- Dùng structured errors và logging.
- Không catch rồi nuốt exception làm mất rollback.
- Không fallback sang tenant/user/security context hardcoded.
- Startup/config phải fail-fast nếu thiếu config bắt buộc; không chạy tiếp với placeholder.
- Không hardcode absolute path hoặc environment-specific path.
- Không expose stack trace ra client ngoài Development.
- Không để public endpoint throw `NotImplementedException`.

## Database Naming

- Table name: lowercase `snake_case`, số nhiều khi phù hợp.
- Column name: lowercase `snake_case`.
- Primary key: `pk_<table>`.
- Foreign key: `fk_<table>_<ref_or_column>`.
- Index: `idx_<table>_<columns>`.
- Unique index: `ux_<table>_<columns>`.
- Tránh quoted PascalCase trong SQL.

## Database

- PostgreSQL: dữ liệu quan hệ và transactional.
- MongoDB: CMS, page JSON, template config, layout data.
- Redis: cache, tenant config, domain mapping, rate limit, temporary locks.
- Kafka/Event Bus: async domain events.

## Verification

- Trước khi implement, biến yêu cầu thành mục tiêu kiểm chứng được.
- Ví dụ: "add validation" phải xác định input invalid nào bị chặn; "fix bug" phải xác định cách reproduce và cách xác nhận đã hết lỗi.
- Với task nhiều bước, mỗi bước cần có verify check tương ứng.
- Nếu chưa có test structure, verify bằng manual/API check đúng phạm vi.
- Với tenant feature, phải kiểm cả case được phép và bị cấm.
- Với UI feature, kiểm desktop và mobile.
- Với API, health-check trước khi test flow.
- Ghi rõ port/environment/mode trong report test.
- Nếu dừng giữa chừng, cập nhật `docs/current-task.md`.

## Code Review Notes

- Không tự tạo hoặc cập nhật file review archive sau mỗi lần review nếu owner chưa yêu cầu.
- Kết quả review mặc định report trực tiếp cho owner: findings, file/line, severity, test gap.
- Nếu owner muốn lưu review lâu dài, tạo file mới theo yêu cầu rõ, ví dụ `docs/reviews/YYYY-MM-DD-topic.md`.
