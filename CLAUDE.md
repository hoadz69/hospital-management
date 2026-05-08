# CLAUDE.md - Clinic SaaS / Hospital Management

Claude Code phải tuân thủ cùng luật dự án với Codex. File này là entrypoint riêng cho Claude, còn luật chung nằm trong `AGENTS.md`.

## ⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT CODE

1. `AGENTS.md`
2. `clinic_saas_report.md`
3. `architech.txt`
4. `docs/current-task.md`
5. `docs/agent-playbook.md`
6. `docs/session-continuity.md`
7. `docs/owner-code-notes.md`
8. Các file trong `rules/` khi bắt đầu viết code thật

## Quy Trình Trước Khi Code

- Với implementation task, tạo/cập nhật `temp/plan.md` và chờ owner duyệt, trừ khi owner nói rõ là triển khai ngay.
- Khi owner chỉ hỏi/phân tích, không tự code.
- Với task dọn tài liệu/config do owner yêu cầu trực tiếp, được sửa ngay nhưng phải giữ scope nhỏ.
- Không chạy build/test/start nếu task không cần.
- Không commit nếu owner chưa yêu cầu.
- Không dùng server/database/token/SSH thật khi owner chưa cung cấp trong phiên hiện tại.
- Sau mỗi lần làm xong phải report lại cho owner: đã làm gì, sửa file nào, kiểm tra gì, còn thiếu/bị chặn gì, bước tiếp theo là gì. Không được im lặng sau khi chạy tool hoặc sửa file.

## Language Rule

- Mọi câu trả lời cho owner, plan, report, handoff, roadmap update và tài liệu hướng dẫn agent phải viết bằng tiếng Việt.
- Chỉ dùng tiếng Anh khi đó là tên code, tên file, API endpoint, command, log/error gốc, keyword kỹ thuật chuẩn, hoặc nội dung trích nguyên văn cần giữ nguyên.
- Nếu tạo/cập nhật `temp/plan.md`, `docs/current-task.md`, `docs/roadmap/clinic-saas-roadmap.md` hoặc command docs, phần mô tả phải ưu tiên tiếng Việt; không viết plan/report chính bằng tiếng Anh.

## Coding Discipline Từ Tài Liệu Cũ

- Think before coding: nêu assumptions, ambiguity và tradeoff cho task lớn. Nếu có nhiều cách hiểu, trình bày ra; không tự chọn im lặng. Nếu không rõ, dừng lại và hỏi.
- Simplicity first: không thêm feature, abstraction hoặc configurability ngoài yêu cầu.
- Surgical changes: chỉ sửa những gì trace trực tiếp về yêu cầu. Mỗi dòng thay đổi phải trace được về request của owner.
- Goal-driven execution: với task nhiều bước, nêu success criteria và cách verify cho từng bước trước khi làm hoặc trong `temp/plan.md`.
- Đọc code hiện có trước khi viết mới.
- Không xóa file hoặc dead code ngoài scope nếu owner chưa yêu cầu.
- Nếu thay đổi của mình tạo import/variable/function unused, phải dọn phần unused do chính mình tạo.
- Security/tenant context thiếu hoặc invalid thì throw/fail rõ, không fallback hardcoded.
- External data phải null-check trước khi đọc property.
- Background work dùng queue/background service, không fire-and-forget tùy tiện.
- Transaction boundary phải rõ với command ghi nhiều DB operations.

## Hướng Dự Án

Sản phẩm đích là nền tảng Clinic SaaS multi-tenant:

- Public Website cho bệnh nhân.
- Clinic Admin Portal cho từng tenant/phòng khám.
- Owner Super Admin Portal cho chủ platform.
- API Gateway/BFF đứng trước các service.
- Backend service dùng Clean Architecture.
- Tenant isolation là yêu cầu bắt buộc trong mọi query/command liên quan dữ liệu tenant.

Structure project hiện tại:

```txt
frontend/
  apps/
  packages/

backend/
  services/
  shared/

infrastructure/
docs/
temp/
```

Không tạo source mới ở root-level `apps/`, `packages/`, hoặc `services/`.

## Stack Định Hướng

- Frontend: Vue 3, Vite, TypeScript, Pinia, Vue Router, shared UI components, design tokens từ Figma.
- Backend: .NET service architecture, ASP.NET Core, Clean Architecture, CQRS/use-case pattern khi có lợi.
- Data: PostgreSQL, MongoDB, Redis.
- Async/realtime: Kafka/Event Bus và SignalR/WebSocket gateway về sau.

Ghi chú version: `.NET 11` trong báo cáo là định hướng ban đầu. Trước khi implementation thật, phải kiểm tra bản .NET stable/LTS hiện tại và ghi rõ quyết định.

## Figma

Figma/FigJam là source of truth cho UI, module map, service map và data flow:

- PDF export đã đọc:
  - `Clinic SaaS Architecture - Source of Truth.pdf`
  - `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`
- Figma Design UI:
  `https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?t=L0tWxOID86LOXPh0-0`
- Architecture Source of Truth:
  `https://www.figma.com/board/zVIS2cgoNqwC21lZbpApjp/Clinic-SaaS-Architecture---Source-of-Truth?t=L0tWxOID86LOXPh0-0`
- Technical Architecture:
  `https://www.figma.com/board/j4vDRWSIRSckcAYvXHocMc/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?t=L0tWxOID86LOXPh0-0`

Dùng Figma MCP khi tool có sẵn. Nếu MCP chưa hiện, restart Claude Code/Codex sau khi cấu hình MCP.

## Claude Agents

Các định nghĩa agent dành cho Claude Code nằm trong `.claude/agents/`. Khi cần chia vai, dùng đúng agent theo domain:

- `architect-agent`
- `figma-ui-agent`
- `frontend-agent`
- `backend-agent`
- `database-agent`
- `devops-agent`
- `qa-agent`
- `documentation-agent`

## Codex Compatibility

Codex không dùng trực tiếp `.claude/agents/*.md`. Để giữ đồng bộ với Codex:

- Luật chung nằm trong `AGENTS.md`.
- Vai trò agent dùng chung nằm trong `docs/agent-playbook.md`.
- Setup Codex/Figma MCP nằm trong `docs/codex-setup.md`.
- Khi cập nhật rule cho Claude, phải cập nhật phần tương ứng trong `AGENTS.md` hoặc `docs/agent-playbook.md` nếu rule đó cũng áp dụng cho Codex.

## Handoff Bắt Buộc

Cuối mỗi lượt làm việc, Claude Code phải trả lời owner bằng báo cáo ngắn:

- đã hoàn thành gì,
- file đã sửa/tạo,
- đã kiểm tra bằng lệnh gì hoặc vì sao chưa kiểm tra,
- việc còn thiếu/bị chặn,
- bước tiếp theo đề xuất.

Nếu dừng giữa chừng, cập nhật `docs/current-task.md` với:

- trạng thái hiện tại,
- việc đã xong,
- việc bị chặn,
- lệnh/hành động tiếp theo,
- file đã thay đổi.
