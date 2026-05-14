# AUTO-RUNNER-V2-SMOKE

Lead Agent: run a safe smoke task for Agent Runner V2.

## Mode

- Docs/workflow smoke only.
- No commit.
- No push.
- No stage.
- No backend/frontend runtime source changes.
- Do not use shell commands. Use `apply_patch` only if editing is needed.

## Scope

Ensure `docs/current-task.md` has a short checkpoint confirming Agent Runner V2 could run a generated queue task.

## Allowed Files

- `docs/current-task.md`
- `docs/agent-queue.md`

## Implement

If this section is already present, do not add a duplicate. If it is missing, add one short section near the end of `docs/current-task.md`:

```md
## Agent Runner V2 Smoke - 2026-05-14

- Result: generated queue task executed by runner.
- Scope: docs-only smoke, no source runtime changes.
- Guardrail: no commit/push/stage.
```

Do not edit backend, frontend, migrations, Figma, screenshots, or logs.

## Verify

Outer runner will run:

```txt
git diff --check
```

## Stop If

- You cannot edit `docs/current-task.md`.
- Any source runtime file would need changes.

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
