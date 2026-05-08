# Current Task - Chuẩn Hóa Repo Clinic SaaS

Last updated: 2026-05-08

## Mục Tiêu

Tiếp tục task trước nhưng sửa theo yêu cầu mới của owner:

- Không replace trắng các file hướng dẫn của agent/Claude.
- Phục hồi và bổ sung các rule bắt buộc, ví dụ “⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT CODE”.
- Bỏ cách diễn đạt liệt kê tên dự án cũ không cần thiết.
- Toàn bộ tài liệu chính dùng tiếng Việt.
- Đọc lại Figma nếu tool cho phép; nếu phiên hiện tại không có MCP/browser không đọc được thì ghi rõ giới hạn.
- Làm rõ file báo cáo chính là `clinic_saas_report.md`; `clinic_saas_report_vi.md` đã bị loại bỏ vì trùng nội dung.

## Đã Hoàn Thành Trong Pass Này

- Kiểm tra git: local repo hiện chỉ có một commit `45898e1`, không có commit trước để checkout trực tiếp.
- Cập nhật `AGENTS.md` bằng tiếng Việt, thêm lại block bắt buộc đọc trước khi code.
- Cập nhật `CLAUDE.md` bằng tiếng Việt, thêm lại block bắt buộc đọc trước khi code.
- Chuyển `clinic_saas_report.md` thành báo cáo tiếng Việt đầy đủ.
- Đã loại bỏ `clinic_saas_report_vi.md` vì repo/report chính đã dùng tiếng Việt trong `clinic_saas_report.md`.
- Cập nhật `architech.txt` thành index kiến trúc tiếng Việt.
- Cập nhật rules backend/coding/testing bằng tiếng Việt và thêm rule bắt buộc.
- Cập nhật `plan.md` sang kế hoạch tiếng Việt cho Clinic SaaS.
- Đọc thêm báo cáo cũ owner cung cấp tại `C:\Users\nvhoa2\Downloads\clinic_saas_report_vi.md`.
- Bổ sung lại các tri thức tốt từ tài liệu cũ: simplicity, surgical changes, anti-patterns Clean Architecture, transaction, background job, security context, null-check.
- Bổ sung Figma Design link và danh sách nhóm màn hình UI từ báo cáo cũ.
- Tạo `docs/agent-playbook.md` cho Codex/agent dùng chung.
- Tạo `.claude/agents/*.md` để hỗ trợ Claude Code theo các vai Architect, Figma UI, Frontend, Backend, Database, DevOps, QA, Documentation.
- Tạo `docs/codex-setup.md` để ghi rõ cấu trúc Codex dùng: `AGENTS.md`, `docs/agent-playbook.md`, `rules/*.md`, MCP global.
- Cập nhật `.claude/commands/*.md` để dùng tiếng Việt và khớp Clinic SaaS.
- Chuyển `deploy.sh`, `kill-and-start.ps1`, `test_api_full_flow.sh` thành placeholder an toàn cho Clinic SaaS, không còn chạy server/API cũ.
- Cập nhật `todo-refactor.md` theo tiến độ mới.
- Cài Figma MCP cho Codex global bằng `codex mcp add figma --url https://mcp.figma.com/mcp`.
- OAuth Figma MCP cho Codex đã hoàn tất.
- Kiểm tra `codex mcp list` cho thấy server `figma` enabled, OAuth.
- Owner đã export hai FigJam board thành PDF trong repo:
  - `Clinic SaaS Architecture - Source of Truth.pdf`
  - `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`
- Đã đọc text layer của cả hai PDF bằng `npx pdf-parse`.
- Đã tạo screenshot tạm ở `temp/pdf-screens/` để kiểm tra bố cục trực quan.
- Đã cập nhật `clinic_saas_report.md`, `architech.txt`, `docs/agent-playbook.md`, `docs/codex-setup.md` theo nội dung PDF thật.
- Đã bổ sung rule bắt buộc report sau mỗi lần làm xong vào `AGENTS.md`, `CLAUDE.md`, `docs/agent-playbook.md`.
- Đã rà lại `tai lieu cu/` và bổ sung nốt các rule còn hữu ích: fail-fast config placeholder, không hardcode path, không expose stack trace, không public `NotImplementedException`, parameterized SQL, health-check API, UTF-8 JSON payload, ghi port/environment khi test.
- Owner đã yêu cầu xóa `tai lieu cu/`; thư mục này đã được xóa sau khi kế thừa rule/tài liệu cần thiết.
- Owner cung cấp thêm guideline CLAUDE.md về Think Before Coding, Simplicity First, Surgical Changes, Goal-Driven Execution; đã đối chiếu và bổ sung phần còn thiếu vào `AGENTS.md`, `CLAUDE.md`, `rules/coding-rules.md`, `docs/agent-playbook.md`, `.claude/commands/plan.md`, `.claude/commands/coding.md`.
- Owner chỉ ra `rules/backend-coding-rules.md` bản mới bị rút gọn quá mức so với rule cũ. Đã viết lại file này theo cấu trúc rule cũ, giữ chi tiết còn phù hợp và chuyển hóa sang Clinic SaaS: config, shared config, no hardcoded path, Unicode, validation, error contract, naming, command/query, repository, DI, logging, tenant isolation, security, testing, anti-pattern checklist.
- Tạo `docs/session-continuity.md` để hướng dẫn session sau cách dùng `plan.md`, `temp/plan.md`, `docs/current-task.md`, và khi nào được cập nhật `rules/*.md`.
- Tạo `docs/owner-code-notes.md` dành cho owner đọc lại sau này khi repo đã có code thật: lưu ý trước/sau khi code, cập nhật plan/handoff/rules ở đâu, khi nào không nên sửa rules.
- Dọn file dư để giảm rối:
  - Xóa `clinic_saas_report_vi.md` vì trùng với `clinic_saas_report.md`.
  - Xóa `docs/agents/` vì trống và không được Claude/Codex dùng.
  - Xóa `docs/code-review-2026-04-13.md`; giữ rule review trong `rules/coding-rules.md`.
  - Xóa `thông tin hệ thống.md` vì chỉ là bản tóm tắt, nội dung đầy đủ nằm trong `clinic_saas_report.md`.
  - Gộp checklist từ `todo-refactor.md` vào `docs/current-task.md` rồi xóa `todo-refactor.md`.

## Figma Status

- Đã thử đọc lại hai board Figma/FigJam.
- Phiên chat hiện tại không expose Figma MCP namespace/tool vì MCP được load khi khởi động phiên.
- Đã mở process Codex mới sau khi cài MCP để đọc board.
- Process Codex mới gọi được Figma MCP nhưng Figma trả lỗi quota: `You've reached the Figma MCP tool call limit on the Starter plan.`
- Browser/search không trích xuất được nội dung board; chỉ có thể xác nhận link nguồn trong tài liệu.
- PDF export đã được dùng làm nguồn thay thế nên báo cáo hiện đã có product map và technical map thật từ FigJam export.

## Bị Chặn

- Cần quota Figma MCP có lại hoặc nâng gói/quota Figma để trích xuất nội dung board thật.
- Sau khi quota có lại, cần restart Codex/Claude Code để MCP tools hiện trong phiên chính và đối chiếu lại với PDF.
- Cần owner cung cấp server/database thật trước khi viết config deploy thực tế.

## Bước Tiếp Theo

1. Chờ quota Figma MCP có lại hoặc dùng tài khoản/gói có quota.
2. Restart agent session để Figma MCP được load.
3. Dùng Figma MCP đọc trực tiếp hai board để đối chiếu với PDF:
   - `Cw0evT4maoKnQX5G23tJpT`
   - `Fwpls2wzNxzGdpDuGGYSxi`
4. Nếu board online khác PDF export, cập nhật `clinic_saas_report.md`.
5. Tạo `temp/plan.md` cho bước scaffold frontend/backend đầu tiên và chờ owner duyệt.

## Checklist Còn Lại Từ `todo-refactor.md`

- Add README cho local setup.
- Confirm backend runtime version.
- Confirm real server/database environments.
- Confirm MVP module list.
- Create `temp/plan.md` cho first implementation pass.
- Generate monorepo structure sau khi owner duyệt.

## Files Changed Trong Pass Này

- `.claude/agents/architect-agent.md`
- `.claude/agents/figma-ui-agent.md`
- `.claude/agents/frontend-agent.md`
- `.claude/agents/backend-agent.md`
- `.claude/agents/database-agent.md`
- `.claude/agents/devops-agent.md`
- `.claude/agents/qa-agent.md`
- `.claude/agents/documentation-agent.md`
- `.claude/commands/coding.md`
- `.claude/commands/db-migrate.md`
- `.claude/commands/deploy.md`
- `.claude/commands/plan.md`
- `.claude/commands/test-api.md`
- `AGENTS.md`
- `CLAUDE.md`
- `clinic_saas_report.md`
- `architech.txt`
- `deploy.sh`
- `kill-and-start.ps1`
- `test_api_full_flow.sh`
- `docs/agent-playbook.md`
- `docs/codex-setup.md`
- `docs/session-continuity.md`
- `docs/owner-code-notes.md`
- `rules/coding-rules.md`
- `rules/backend-coding-rules.md`
- `rules/backend-testing-rules.md`
- `rules/coding-rules.md`
- `plan.md`
- `docs/current-task.md`
- `clinic_saas_report_vi.md` deleted
- `docs/agents/` deleted
- `docs/code-review-2026-04-13.md` deleted
- `thông tin hệ thống.md` deleted
- `todo-refactor.md` deleted

## Ghi Chú Về `tai lieu cu`

- Thư mục `tai lieu cu/` đã được xóa theo yêu cầu owner.
- Nội dung cũ trong thư mục này không phải runtime/source of truth hiện tại.
- Tính đến 2026-05-08, các rule/kỹ thuật còn phù hợp đã được chuyển vào `AGENTS.md`, `CLAUDE.md`, `rules/*.md`, `docs/agent-playbook.md`, `docs/codex-setup.md`, `clinic_saas_report.md`.
