# FE-OWNER-ADMIN-UI-SMOKE

Lead Agent: run Owner Admin UI smoke through Agent Runner.

## Mode

- Frontend QA smoke.
- No source edits.
- No commit.
- No push.
- No stage.
- No screenshot.
- Do not run shell commands inside Codex worker; outer runner will run verify commands.

## Scope

- Owner Admin Vite app.
- Routes:
  - `/dashboard`
  - `/clinics`
  - `/clinics/create`
  - `/clinics/tenant-aurora-dental`

## Out Of Scope

- Backend/API runtime smoke.
- Figma.
- Visual screenshot.
- Editing `frontend/**`.
- Editing docs beyond runner checkpoint.

## Implement

Do not modify files. Return a handoff report only. The outer runner will execute the real UI smoke commands.

## Verify

Outer runner will run:

```txt
git diff --check
cd frontend && npm run typecheck
cd frontend && npm run build
frontend owner-admin route smoke
```

## Stop If

- Any source file edit would be required.
- Verify fails.

## Report

```txt
Lane:
Action:
Files changed:
Verify:
Skipped/blocker:
Dirty:
Next:
```
