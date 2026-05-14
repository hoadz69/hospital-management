# Frontend Task Template

Lead Agent: run frontend task `<TASK_ID>` in non-interactive runner mode.

## Mode

- Frontend lane.
- No commit.
- No push.
- No stage.
- No screenshot by default.

## Read

- `AGENTS.md`
- `docs/current-task.md`
- `docs/current-task.frontend.md`
- `temp/plan.frontend.md`
- `rules/coding-rules.md`
- `docs/codex-setup.md` nếu cần GitNexus.
- Figma frame cụ thể nếu task UI-to-code/visual yêu cầu; mặc định Phase 4+ dùng V3 node `65:2`.

## Scope

- Task: `<mô tả frontend task>`.
- Allowed files:
  - `frontend/**`
  - `docs/current-task.frontend.md`
  - `temp/plan.frontend.md`

## Out Of Scope

- `backend/**`
- Database/migration.
- Figma edit.
- Screenshot nếu task không nói rõ visual QA.
- Commit/push/stage.

## Implement

1. Chạy GitNexus gate nếu sửa route/store/API client/shared package/component reusable.
2. Bám shared UI/design tokens và Figma handoff nếu có.
3. Không tự đoán API contract; nếu contract chưa rõ thì dừng.
4. Giữ tenant context ở API client/request layer.
5. Tạo loading/empty/error/success/conflict state nếu flow cần.
6. Cập nhật checkpoint lane nếu task dài hoặc sửa nhiều file.

## Verify

```txt
git diff --check
cd frontend && npm run typecheck
cd frontend && npm run build
```

Route smoke chỉ chạy khi task đổi route/UI behavior/API flow.

## Stop If

- Contract chưa rõ.
- Figma frame thiếu với visual task.
- Backend runtime dependency chưa DONE.
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
