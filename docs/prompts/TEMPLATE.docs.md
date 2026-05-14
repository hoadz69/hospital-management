# Docs Task Template

Lead Agent: run docs task `<TASK_ID>` in non-interactive runner mode.

## Mode

- Docs/workflow lane.
- No backend/frontend feature source changes.
- No commit.
- No push.
- No stage.
- No screenshot.

## Read

- `AGENTS.md`
- `docs/agent-playbook.md` nếu task đổi workflow.
- `docs/codex-setup.md` nếu task đổi Codex/tooling.
- `docs/session-continuity.md` nếu task đổi checkpoint/resume.
- Current-task/plan lane liên quan nếu task cập nhật trạng thái.

## Scope

- Task: `<mô tả docs task>`.
- Allowed files:
  - `docs/**`
  - `temp/plan*.md`

## Out Of Scope

- `backend/**`
- `frontend/**`
- Database/migration.
- Figma edit.
- Commit/push/stage.

## Implement

1. Sửa tài liệu đúng scope, không append lịch sử dài.
2. Không bịa verify result.
3. Giữ dashboard ngắn, lane detail nằm trong lane file.
4. Không đọc archive nếu không cần.

## Verify

```txt
git diff --check
```

## Stop If

- Rule mới conflict với `AGENTS.md`.
- Cần owner decision.
- Verify fail.
- Model/provider fail.

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
