# Auto Planner - Agent Runner V2

Lead Agent: cập nhật `docs/agent-queue.md` để runner có task tiếp theo hợp lệ.

## Mode

- Planner lane.
- Chỉ tạo/cập nhật queue và prompt task.
- Không commit, push, stage.
- Không sửa `backend/**`, `frontend/**`, database migration, Figma hoặc screenshot.
- Không đánh dấu task runtime/API/DB là `DONE`.

## Read

- `AGENTS.md`
- `docs/current-task.md`
- `docs/current-task.backend.md`
- `docs/current-task.frontend.md`
- `temp/plan.backend.md`
- `temp/plan.frontend.md`
- `docs/agent-queue.md`
- `docs/prompts/README.md`
- `docs/prompts/TEMPLATE.backend.md`
- `docs/prompts/TEMPLATE.frontend.md`
- `docs/prompts/TEMPLATE.fullstack.md`
- `docs/prompts/TEMPLATE.docs.md`

## Scope

1. Đọc trạng thái active hiện tại.
2. Nếu có task tiếp theo đã đủ scope, tạo task marker trong `docs/agent-queue.md` với `status: READY`.
3. Nếu task cần owner/env/contract, tạo task `BLOCKED` và ghi `blocker_type`.
4. Tạo prompt riêng `docs/prompts/<TASK-ID>.md` cho task thật nếu task không dùng template.
5. Nếu không có task product an toàn, tạo task docs/workflow smoke nhỏ để verify runner V2, không chạm source runtime.

## Queue Schema

Mỗi task dùng marker:

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

## Planning Rules

- Không tạo task `READY` nếu thiếu contract, tenant/security context, runtime env bắt buộc, hoặc owner decision.
- Runtime smoke task phải có verify thật cho smoke bắt buộc; chỉ check env/build/test chưa đủ để `DONE`.
- `blocker_type: owner_required` nghĩa là runner không được tự mở.
- `blocker_type: env_missing` hoặc `auto_recheck` chỉ được dùng với `auto_retry: true` khi re-check an toàn.
- Nếu queue có task sai trạng thái hoặc dependency mất, sửa queue về `BLOCKED` hoặc xóa task nếu chỉ là artifact smoke đã lỗi.
- Queue không phải backlog dài hạn; chỉ giữ 1-3 task active gần nhất.

## Safe Smoke Fallback

Nếu không có product task đủ an toàn, tạo task:

- `id: AUTO-RUNNER-V2-SMOKE`
- `lane: docs`
- `status: READY`
- `priority: 1`
- `prompt_file: docs/prompts/AUTO-RUNNER-V2-SMOKE.md`
- `allowed_paths`: `docs/current-task.md`, `docs/agent-queue.md`
- `verify`: `git diff --check`

Prompt smoke chỉ được thêm checkpoint ngắn vào `docs/current-task.md`, báo runner V2 planner/worker path đã được kiểm tra, không sửa source runtime.

## Stop If

- Cần quyết định owner.
- Không rõ task nào an toàn.
- Queue schema không thể giữ hợp lệ.
- Git conflict hoặc file không đọc được.

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
