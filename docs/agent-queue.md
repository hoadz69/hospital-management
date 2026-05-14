# Agent Queue - Clinic SaaS

## Purpose

File này là queue để runner tự chọn task `READY` và chạy bằng Codex non-interactive. Queue giúp owner không phải quay vòng GPT -> Codex cho từng prompt nhỏ.

## Status Meaning

- `READY`: có thể chạy ngay nếu dependency đã `DONE`.
- `IN_PROGRESS`: runner đang chạy hoặc session chết giữa chừng.
- `BLOCKED`: cần owner/Lead xử lý trước khi chạy tiếp.
- `DONE`: task đã pass verify theo queue.
- `SKIPPED`: task bị bỏ qua có lý do.

## Queue Rules

- Runner chỉ lấy task `READY` đầu tiên không bị `depends_on` block.
- Task có `depends_on` chỉ chạy khi toàn bộ dependency `DONE`.
- Runner V2 có thể gọi planner bằng `-AutoPlan` khi không có task `READY`.
- Planner được phép tạo/cập nhật task trong queue, nhưng không được sửa source runtime.
- Runner chỉ tự re-check task `BLOCKED` khi task có `auto_retry: true` và `blocker_type` thuộc `env_missing` hoặc `auto_recheck`.
- Runner không tự chạy task `BLOCKED` có `blocker_type: owner_required`, `contract_missing`, `verify_failed`, hoặc `provider_failed`.
- Runner không commit/push/stage.
- Prompt feature thật phải tách backend/frontend nếu contract cho phép.
- Screenshot không chạy mặc định.
- Runtime/API/DB task không được đánh dấu `DONE` nếu smoke bắt buộc còn pending.
- Nếu Codex/provider lỗi websocket/handshake/rate-limit/quota/model capacity thì mark `BLOCKED` và dừng.
- Nếu verify fail thì mark `BLOCKED` và dừng.
- Nếu session chết, lần sau runner resume từ queue + git diff + checkpoint.

## Task Schema

Mỗi task nằm trong marker để PowerShell runner parse ổn định:

````md
<!-- task:start TASK-ID -->
```yaml
id: TASK-ID
lane: backend|frontend|docs|devops|database|fullstack
status: READY|IN_PROGRESS|BLOCKED|DONE|SKIPPED
priority: 10
title: Short title
prompt_file: docs/prompts/<file>.md
depends_on: []
blocker_type: none|owner_required|env_missing|contract_missing|verify_failed|provider_failed|auto_recheck
auto_retry: false
attempts: 0
max_attempts: 1
allowed_paths:
  - docs/**
verify:
  - git diff --check
stop_conditions:
  - codex exec failed
  - verify failed
checkpoint_file: docs/current-task.md
```
<!-- task:end -->
````

## Tasks

Hiện chưa có task thủ công. Dùng runner V2 với `-AutoPlan` để planner tự tạo task tiếp theo từ dashboard/plan active.
<!-- task:start AUTO-RUNNER-V2-SMOKE -->
```yaml
id: AUTO-RUNNER-V2-SMOKE
lane: docs
status: DONE
finished_at: "2026-05-14T16:49:26+07:00"
result: "DONE"
priority: 1
title: Agent Runner V2 generated smoke task
prompt_file: docs/prompts/AUTO-RUNNER-V2-SMOKE.md
depends_on: []
blocker_type: none
auto_retry: false
attempts: 1
max_attempts: 1
allowed_paths:
  - docs/current-task.md
  - docs/agent-queue.md
verify:
  - git diff --check
stop_conditions:
  - codex exec failed
  - verify failed
checkpoint_file: docs/current-task.md
```
<!-- task:end -->

<!-- task:start FE-OWNER-ADMIN-UI-SMOKE -->
```yaml
id: FE-OWNER-ADMIN-UI-SMOKE
lane: frontend
status: DONE
finished_at: "2026-05-14T17:27:48+07:00"
result: "DONE"
priority: 1
title: Owner Admin UI route smoke through Vite
prompt_file: docs/prompts/FE-OWNER-ADMIN-UI-SMOKE.md
depends_on: []
blocker_type: none
auto_retry: false
attempts: 1
max_attempts: 1
allowed_paths:
  - frontend/**
  - docs/agent-queue.md
  - docs/current-task.frontend.md
  - temp/agent-runner/**
verify:
  - git diff --check
  - cd frontend && npm run typecheck
  - cd frontend && npm run build
  - frontend owner-admin route smoke
stop_conditions:
  - codex exec failed
  - verify failed
  - vite dev server failed
checkpoint_file: docs/current-task.frontend.md
```
<!-- task:end -->
