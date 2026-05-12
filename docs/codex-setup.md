# Codex Setup - Clinic SaaS

File nay chi giu setup/tooling cho Codex. Workflow canonical nam trong `AGENTS.md` va `docs/agent-playbook.md`; khong lap lai full workflow tai day.

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