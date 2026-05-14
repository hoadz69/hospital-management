# Fullstack Task Template

Lead Agent: run fullstack task `<TASK_ID>` in non-interactive runner mode.

## Mode

- Fullstack lane with explicit contract first.
- No commit.
- No push.
- No stage.
- No screenshot by default.

## Read

- `AGENTS.md`
- `docs/agent-playbook.md`
- `docs/current-task.md`
- `docs/current-task.backend.md`
- `docs/current-task.frontend.md`
- `temp/plan.backend.md`
- `temp/plan.frontend.md`
- `rules/coding-rules.md`
- Backend/database/frontend rules theo scope.
- Figma frame cụ thể nếu task UI-to-code/visual yêu cầu.

## Scope

- Task: `<mô tả fullstack task>`.
- Contract source: `<docs/api/... hoặc backend/shared/contracts/...>`.
- Allowed files:
  - `backend/**` nếu backend subtask được duyệt.
  - `frontend/**` nếu frontend subtask được duyệt.
  - `docs/current-task.backend.md`
  - `docs/current-task.frontend.md`
  - `temp/plan.backend.md`
  - `temp/plan.frontend.md`

## Out Of Scope

- API contract ngoài file đã chốt.
- Migration destructive.
- Figma edit.
- Screenshot nếu không có visual QA.
- Commit/push/stage.

## Implement

1. Nếu chưa có contract rõ, tạo/update contract plan rồi dừng.
2. Backend và frontend chỉ chạy song song nếu contract đã chốt.
3. Backend giữ service boundary, tenant isolation, transaction boundary.
4. Frontend bám contract, giữ mock fallback đúng nghĩa dev/offline nếu cần.
5. Chạy GitNexus impact/detect-changes theo gate khi sửa runtime code.
6. Cập nhật lane checkpoint.

## Verify

```txt
git diff --check
dotnet restore backend/ClinicSaaS.Backend.sln
dotnet build backend/ClinicSaaS.Backend.sln --no-restore
dotnet test backend/ClinicSaaS.Backend.sln --no-build
cd frontend && npm run typecheck
cd frontend && npm run build
```

Thêm API/UI smoke thật khi env có và task yêu cầu.

## Stop If

- Contract chưa rõ.
- BE/FE contract lệch.
- Tenant isolation chưa rõ.
- Runtime env bắt buộc thiếu.
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
