# Codex Setup - Clinic SaaS

File nay chi giu setup/tooling cho Codex. Workflow canonical nam trong `AGENTS.md` va `docs/agent-playbook.md`; khong lap lai full workflow tai day.

Phan biet nhanh:
- `AGENTS.md`: bootstrap rule bat buoc, guardrails, reading policy, lane policy. Doc dau tien trong moi task.
- `docs/codex-setup.md`: huong dan tool cho Codex nhu GitNexus, MCP, Figma/browser, fallback khi tool loi. Chi doc khi task cham setup/tooling hoac `AGENTS.md` tro toi.

## File Can Biet

- `AGENTS.md`: bootstrap guardrails va reading policy, doc dau tien.
- `docs/agent-playbook.md`: Feature Team Execution Workflow, Short Lead Prompt, Fast/Budget Mode, Full Team Mode, report format.
- `docs/session-continuity.md`: checkpoint/resume protocol.
- `docs/current-task.md`: dashboard tong ngan.
- Lane files: `docs/current-task.frontend.md`, `temp/plan.frontend.md`, `docs/current-task.backend.md`, `temp/plan.backend.md`.

## MCP / Tooling

- Figma MCP dung khi owner yeu cau Figma/visual, task restyle lon, screenshot cho thay UI lech, hoac chuan bi commit UI lon.
- Browser/Playwright MCP dung cho UI smoke/visual QA khi task yeu cau; khong chup screenshot mac dinh cho task nho.
- Web research dung khi owner yeu cau research/latest/inspiration hoac thong tin co the thay doi.
- Khong tao Figma file moi neu owner chua yeu cau ro.

## Agent Queue / Auto Runner

- Codex non-interactive dung qua `codex exec`.
- Runner file: `scripts/agent-runner.ps1`.
- Queue file: `docs/agent-queue.md`.
- Prompt folder: `docs/prompts/`.
- Runner docs: `docs/agent-runner.md`.
- Log folder: `temp/agent-runner/`; day la artifact, co the xoa sau khi doc ket qua va khong stage/commit mac dinh.
- Chay dry-run truoc: `powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -DryRun`.
- Chay 1 task: `powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -Once`.
- Chay batch nho: `powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -MaxTasks 3`.
- Neu output co websocket/handshake/rate-limit/quota/model capacity/provider error thi runner mark `BLOCKED` va dung.
- Khong dung runner de commit/push/stage.
- Khong dua feature runtime vao queue neu chua co contract, verify va stop condition ro.

## GitNexus

- Uu tien Codex: source huong dan chinh la `AGENTS.md`, `docs/agent-playbook.md`, `docs/codex-setup.md` va skill/MCP trong user profile Codex. `.claude/**` chi la wrapper phu khi owner thinh thoang dung Claude, khong dua vao default context.
- Repo da co GitNexus index local trong `.gitnexus/` va MCP global cho Codex/Cursor. MCP server moi co the can mo session Codex moi de thay resources/tools.
- Kiem tra nhanh: `gitnexus status`, `gitnexus list`.
- Re-index sau thay doi lon: `gitnexus analyze --skip-agents-md --no-stats --name hospital-management`.
- Bao ve context custom: khong chay `gitnexus analyze` mac dinh neu khong muon tool cap nhat `AGENTS.md` hoac wrapper phu.
- Auto-use gate: Codex tu chay GitNexus khi task code co blast radius khong ro, cham API contract, route/store, shared package, backend shared, tenant/security, DB/persistence, refactor/rename, bug chua ro nguon, hoac symbol co kha nang reuse.
- Duoc bo qua GitNexus voi docs nho, text/CSS cuc bo, file/dong da ro, hoac chi chay verify command.
- Lenh thuong dung: `gitnexus query "<concept>"`, `gitnexus context <symbol>`, `gitnexus impact <symbol> --include-tests`, `gitnexus detect-changes --scope all`.
- Truoc review/commit split neu co diff code runtime: chay `gitnexus detect-changes --scope all`.
- Neu GitNexus fail/unavailable: fallback `rg`/doc source lien quan va report ngan.
- `.gitnexusignore` loai archive, binary design/reference va generated artifacts de graph tap trung vao source/docs active.

Quy trinh goi nhanh:
1. Chay `gitnexus status` neu task code co blast radius khong ro.
2. Dung `gitnexus query` hoac `gitnexus context` de tim symbol/flow lien quan.
3. Dung `gitnexus impact <symbol> --include-tests` truoc khi sua symbol co kha nang reuse.
4. Sau khi co diff code runtime hoac truoc commit split, chay `gitnexus detect-changes --scope all`.
5. Neu index stale va task can graph moi, chay analyze voi `--skip-agents-md --no-stats`.

## Archive Policy

- `docs/archive/**` va `temp/archive/**` la cold storage.
- Khong doc archive trong task thuong ngay va khong dua vao default context.
- Chi doc archive khi owner yeu cau ro, active file tro toi section archive cu the, hoac can bang chung debug cu.

## Runtime / Secret Notes

- Khong ghi secret, token, private key, server IP that, connection string that vao repo/docs/log.
- Backend/API smoke uu tien runtime server test khi owner/session cung cap env phu hop; chi ghi ten bien/session, khong ghi gia tri nhay cam.
- Generated screenshots/logs/test artifacts khong stage/commit neu owner chua yeu cau ro.

## Fast Report Reminder

```txt
Lane:
Action:
Files changed:
Verify:
Skipped/blocker:
Dirty:
Next:
```
