# AUTO-RUNNER-CODEX-WORKER-SMOKE

Lead Agent: chạy một task docs-only rất nhỏ để kiểm tra `codex exec` worker path thật của Agent Runner.

## Mode

- Lane docs/workflow.
- Không commit, push, stage.
- Không sửa backend, frontend, database, Figma, screenshot hoặc generated artifact.
- Không chạy shell nếu không cần. Nếu cần sửa file, dùng công cụ edit file an toàn.

## Scope

Thêm một checkpoint ngắn vào cuối `docs/current-task.md` để chứng minh Codex worker đã thực thi qua runner.

## Allowed Files

- `docs/current-task.md`
- `docs/agent-queue.md`

## Implement

Thêm section sau vào cuối `docs/current-task.md`:

```md
## Agent Runner Codex Worker Smoke - 2026-05-14

- Result: codex worker task executed through runner.
- Scope: docs-only smoke, no runtime source changes.
- Guardrail: no commit/push/stage.
```

Không sửa file khác.

## Verify

Outer runner sẽ chạy:

```txt
git diff --check
```

## Stop If

- Không sửa được `docs/current-task.md`.
- Bất kỳ source runtime file nào cần thay đổi.

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
