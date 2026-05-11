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

## Prompt Ngắn Cho Lead Agent Trong Codex

Codex phải xử lý các prompt ngắn sau như Feature Team Execution Workflow triggers:

```txt
Lead Agent: bắt đầu A5.2
Lead Agent: bắt đầu A5.3
Lead Agent: làm tiếp A5.4
Lead Agent: verify A5.2
Lead Agent: chia commit A5.1b
```

Owner không cần paste danh sách "Agents tham gia". Codex Lead tự đọc `docs/agent-playbook.md` + `docs/agents/lead-agent.md`, tự phân lane, tự chọn agents theo scope, rồi report rõ lane/agents/verify/docs/dirty files. Nếu phiên có subagent runtime thì có thể spawn; nếu không, giả lập tuần tự bằng checklist agent docs.

Các prompt trên là action trigger thật. Codex không được chỉ acknowledge, không được chỉ nói đã đọc `AGENTS.md`, và không được dừng sau khi tóm tắt guardrail. Tối thiểu phải chạy `git status --branch --short`, `git diff --stat`, đọc dashboard/lane current-task/lane plan/handoff liên quan, phân lane, chọn agents, quyết định action (`implement`/`resume`/`verify`/`commit-split`/`plan-only`), thực hiện action và report.

Guardrail mặc định: không commit, không push, không stage; không stage/commit artifact/log/screenshot/generated files; không stage `.claude/settings.local.json`; không sửa ngoài scope; không xóa source/docs/plan dirty nếu chưa rõ chủ sở hữu. Nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap thì `bắt đầu` hoặc `làm tiếp` phải implement/resume đúng scope. Chỉ dừng approval gate khi task mới, scope chưa rõ, cross-lane lớn chưa có plan, có rủi ro destructive/secret/security, hoặc owner nói rõ "chỉ lập plan", "chưa code", "đợi tôi duyệt".

Fast Mode là mặc định cho `Lead Agent: làm/tiếp tục/fix/verify A7` hoặc `Lead Agent: làm tiếp`: đọc tối thiểu, không gọi full subagents/Figma/screenshot nếu không cần, không paste log dài và report tóm tắt. Full Team Mode chỉ chạy khi owner nói rõ `chạy Feature Team`, `full team`, `completion gate`, `visual QA`, `Figma check`, `screenshot verify`, hoặc task chạm nhiều vùng/rủi ro cao.

Token budget: PASS chỉ ghi PASS; FAIL chỉ paste lỗi liên quan; không paste full diff/log; không tóm tắt lại `AGENTS.md`; docs update chỉ ghi file + section. Fast Mode final report mặc định gồm `Lane`, `Action`, `Files changed`, `Verify`, `Skipped/blocker`, `Dirty`, `Next`.

Prompt ngắn chuẩn owner có thể dùng mà không cần lặp guardrail:

```txt
Lead Agent: tiếp tục A7 fast mode
Lead Agent: làm A7 fast mode
Lead Agent: fix lỗi A7
Lead Agent: visual QA A7 budget mode
Lead Agent: làm toàn bộ A7 full team
Lead Agent: chạy Feature Team cho A7
Lead Agent: finalize A7, cleanup artifacts và push
```

## Cách Codex Dùng QA Screenshot / Artifact Workflow

Workflow này không phải skill tự chạy. Khi Codex đóng vai Lead Agent hoặc QA Agent:

1. Đọc `AGENTS.md`, `docs/agents/qa-agent.md` và `docs/agents/lead-agent.md`.
2. Chỉ chụp screenshot khi owner yêu cầu visual QA, trước commit UI lớn, có lỗi visual cần chứng minh trước/sau, hoặc task là restyle/layout lớn; không chụp mọi route nhỏ.
3. Mặc định tối đa 1 desktop chính + 1 mobile chính, thêm tối đa 2 ảnh nếu route thật sự quan trọng; nếu cần nhiều route, tạo contact sheet/collage như `frontend/test-results/a7-visual-contact-sheet.png`.
4. Report route/state, viewport nếu có, screenshot path, component/UI state đã test, pass/fail và visual issue.
5. Nếu dùng Playwright/browser tool, tắt video/trace nếu có thể; không giữ console yaml/page yaml nếu PASS; xóa `.playwright-mcp` artifacts nếu không cần.
6. Không stage/commit screenshot/log/yaml/generated artifacts.
7. Sau khi owner đã review hoặc task/test hoàn tất, Lead Agent cleanup artifact untracked theo rule trong `AGENTS.md`; trước và sau cleanup chạy `git status --short`, và không xóa tracked/source/docs/plan dirty.
