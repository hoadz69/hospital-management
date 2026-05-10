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

## Browser MCP Cho UI Research

Đã cấu hình thêm global MCP cho browser/research workflow:

```bash
codex mcp add playwright -- npx -y @playwright/mcp@latest
```

Kết quả kiểm tra bằng `codex mcp list`:

- MCP server `playwright` đã enabled.
- Command: `npx`.
- Args: `-y @playwright/mcp@latest`.
- Auth: không cần OAuth/API key.

Lưu ý: MCP tools thường chỉ xuất hiện sau khi mở phiên Codex mới. Playwright MCP hỗ trợ mở/render website thật khi có URL hoặc search engine, nhưng không thay thế Figma MCP. Figma MCP dùng để đọc/sửa Figma; browser/search MCP dùng cho research UI inspiration.

Search MCP như Brave/Perplexity thường cần API key. Không cấu hình các MCP này bằng secret giả; chỉ thêm khi owner cung cấp key rõ qua cơ chế an toàn. Đã kiểm tra `@modelcontextprotocol/server-fetch` trên npm ngày 2026-05-09 nhưng package này không tồn tại dưới tên đó, nên chưa thêm Fetch MCP.

## Figma Board Status

Đã kiểm tra lại bằng Figma MCP ngày 2026-05-08 sau khi owner đổi tài khoản/link Figma:

- UI Design: `https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?t=L0tWxOID86LOXPh0-0`
- Architecture Source of Truth: `https://www.figma.com/board/zVIS2cgoNqwC21lZbpApjp/Clinic-SaaS-Architecture---Source-of-Truth?t=L0tWxOID86LOXPh0-0`
- Technical Architecture: `https://www.figma.com/board/j4vDRWSIRSckcAYvXHocMc/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?t=L0tWxOID86LOXPh0-0`

Kết quả: cả 3 link đều đọc được trong phiên hiện tại.

- UI Design file key `1nbJ5XkrlDgQ3BmzpXzhCC` trả metadata canvas `Clinic Website UI Kit`.
- Architecture Source of Truth board key `zVIS2cgoNqwC21lZbpApjp` trả FigJam canvas.
- Technical Architecture board key `j4vDRWSIRSckcAYvXHocMc` trả FigJam canvas.

Không được tự bịa section/node/data-flow nếu phiên sau không đọc được Figma MCP.

Owner đã export hai board thành PDF trong repo:

- `Clinic SaaS Architecture - Source of Truth.pdf`
- `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`

Hai PDF này đã được đọc bằng text extraction/screenshot ngày 2026-05-08 và đã được dùng để cập nhật `clinic_saas_report.md`, `architech.txt`.

## Cách Tiếp Tục Khi Cần Đọc Figma

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
- Web Research Agent
- Figma UI Agent
- Frontend Agent
- Backend Agent
- Database Agent
- DevOps Agent
- QA Agent
- Documentation Agent

Nếu owner không yêu cầu parallel/subagent, dùng các role này như checklist tư duy trong cùng một phiên Codex.
