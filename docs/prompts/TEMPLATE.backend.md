# Backend Task Template

Lead Agent: run backend task `<TASK_ID>` in non-interactive runner mode.

## Mode

- Backend lane.
- No commit.
- No push.
- No stage.
- No screenshot.

## Read

- `AGENTS.md`
- `docs/current-task.md`
- `docs/current-task.backend.md`
- `temp/plan.backend.md`
- `rules/coding-rules.md`
- `rules/backend-coding-rules.md`
- `rules/database-rules.md` nếu chạm persistence/schema/query/seed.
- `docs/codex-setup.md` nếu cần GitNexus.

## Scope

- Task: `<mô tả backend task>`.
- Allowed files:
  - `backend/**`
  - `infrastructure/postgres/init.sql` nếu scope có migration/bootstrap.
  - `docs/current-task.backend.md`
  - `temp/plan.backend.md`

## Out Of Scope

- `frontend/**`
- Figma edit.
- Screenshot.
- Production secret/IP/key/token.
- Commit/push/stage.

## Implement

1. Chạy GitNexus gate nếu task chạm API/route/shared/security/DB/refactor.
2. Giữ service boundary và tenant isolation.
3. Giữ API Gateway forward-only nếu contract thuộc service khác.
4. Giữ Tenant Service dùng Dapper + Npgsql; không tự đổi sang EF migration.
5. Thêm hoặc cập nhật test đúng scope.
6. Cập nhật checkpoint lane nếu task dài hoặc sửa nhiều file.

## Verify

```txt
git diff --check
dotnet restore backend/ClinicSaaS.Backend.sln
dotnet build backend/ClinicSaaS.Backend.sln --no-restore
dotnet test backend/ClinicSaaS.Backend.sln --no-build
```

Thêm runtime smoke thật nếu task yêu cầu và env có sẵn. Không dùng stub/mock để đánh dấu E2E Done.

## Stop If

- Contract chưa rõ.
- Migration destructive.
- Tenant isolation/security context chưa rõ.
- Runtime env bắt buộc bị thiếu.
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
