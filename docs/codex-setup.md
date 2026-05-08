# Codex Setup - Clinic SaaS

File này ghi rõ phần Codex cần để làm việc đúng trong repo `hospital-management`.

## Cấu Trúc Codex Dùng Trong Repo

Codex không đọc trực tiếp `.claude/agents/*.md` như Claude Code. Cấu trúc dùng cho Codex là:

- `AGENTS.md`: luật chung bắt buộc, đọc đầu tiên.
- `clinic_saas_report.md`: báo cáo/source of truth chính.
- `architech.txt`: index kiến trúc ngắn.
- `docs/current-task.md`: trạng thái task/handoff.
- `docs/agent-playbook.md`: role/prompt nền cho các agent.
- `rules/*.md`: coding/testing rules.

## Figma MCP Cho Codex

Đã cấu hình global MCP cho Codex bằng lệnh:

```bash
codex mcp add figma --url https://mcp.figma.com/mcp
```

Kết quả kiểm tra:

```bash
codex mcp list
codex mcp get figma
```

Trạng thái hiện tại:

- MCP server `figma` đã enabled.
- Transport: `streamable_http`.
- URL: `https://mcp.figma.com/mcp`.
- Auth: OAuth đã hoàn tất.

## Figma Board Status

Đã thử đọc hai board bằng process Codex mới sau khi cài MCP:

- `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth`
- `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture`

Kết quả: Figma MCP hoạt động nhưng Figma trả lỗi quota:

```txt
You've reached the Figma MCP tool call limit on the Starter plan.
```

Vì vậy hiện chưa trích xuất được nội dung thật của board. Không được tự bịa section/node/data-flow.

Owner đã export hai board thành PDF trong repo:

- `Clinic SaaS Architecture - Source of Truth.pdf`
- `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`

Hai PDF này đã được đọc bằng text extraction/screenshot ngày 2026-05-08 và đã được dùng để cập nhật `clinic_saas_report.md`, `architech.txt`.

## Cách Tiếp Tục Khi Quota Có Lại

1. Mở phiên Codex mới để MCP tool được load.
2. Chạy lại kiểm tra:
   ```bash
   codex mcp list
   codex mcp get figma
   ```
3. Dùng Figma MCP đọc hai board.
4. Đối chiếu nội dung MCP với PDF export.
5. Cập nhật:
   - `clinic_saas_report.md`
   - `architech.txt`
   - `docs/current-task.md`

## Khi Cần Agent Trong Codex

Codex dùng vai trò trong `docs/agent-playbook.md`. Không cần tạo `.claude/agents` cho Codex.

Nếu owner yêu cầu chạy nhiều agent/parallel work, dùng role tương ứng:

- Architect Agent
- Figma UI Agent
- Frontend Agent
- Backend Agent
- Database Agent
- DevOps Agent
- QA Agent
- Documentation Agent

Nếu owner không yêu cầu parallel/subagent, dùng các role này như checklist tư duy trong cùng một phiên Codex.
