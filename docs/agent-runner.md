# Codex Agent Runner

## Runner Timeout / Executor

- `-PlannerTimeoutSeconds` mac dinh `300`.
- `-WorkerTimeoutSeconds` mac dinh `900`.
- `-VerifyTimeoutSeconds` mac dinh `1800`.
- Queue task co the dung `executor: verify_only` de bo qua `codex exec` va chi chay verify whitelist. Chi dung cho smoke/env recheck an toan; implementation task van dung mac dinh `executor: codex`.
- Neu auto planner loi/timeout/provider fail hoac khong sua queue, runner se thu deterministic task truoc. Hien co rule sinh `FE-TENANT-LIFECYCLE-API-WIRING` khi source con lifecycle mock/local timer va FE/BE da co Tenant status API. Neu khong co product task nhung `frontend/` dang dirty, runner sinh `FE-DIRTY-VERIFY` de chay `git diff --check`, `typecheck`, `build`, `gitnexus detect-changes`. Neu khong co task an toan, runner moi fallback ve `AUTO-RUNNER-SELF-CHECK`; khong dung smoke docs cu lam fallback mac dinh.

## Mục Tiêu

Runner giúp owner không phải hỏi ChatGPT từng prompt nhỏ. Runner tự lấy task `READY` trong `docs/agent-queue.md`, gọi `codex exec`, chạy verify, cập nhật queue và ghi checkpoint.

Runner V2 có thêm `-AutoPlan`: khi queue không có task `READY`, runner gọi planner để tạo/cập nhật task tiếp theo. Nếu Codex planner bị lỗi shell/sandbox và không sửa queue, runner có fallback deterministic tạo một docs-only smoke task để kiểm chứng đường chạy auto.

## Trạng Thái Validate

Đã validate runner ngày 2026-05-14:

- `scripts/agent-runner.ps1 -Once` đã chạy thật qua `codex exec` với 2 task docs-only tạm thời.
- Cả 2 task đều tự chuyển `DONE`, ghi checkpoint, và verify `git diff --check` PASS.
- Sau cleanup artifact, chạy lại `scripts/agent-runner.ps1 -DryRun` PASS: runner đọc được queue, thấy hiện không còn task `READY`, không chạy task `BLOCKED`.
- One-task smoke sau cleanup đã PASS: `-DryRun` chọn đúng task tạm, `-Once` gọi được `codex exec`, task tạm chuyển `DONE`, outer verify `git diff --check` exit code 0.
- Caveat: trong one-task smoke, Codex worker báo lỗi shell nội bộ `CreateProcessWithLogonW failed: 1326` khi tự đọc file; runner vẫn chạy/verify được. Task runtime thật cần theo dõi lỗi này nếu worker phải chạy shell bên trong.
- Log/jsonl smoke trong `temp/agent-runner/` đã xóa vì là artifact kiểm chứng, không cần commit.

## Cách Chạy

Từ repo root:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -DryRun
```

Lệnh này chỉ kiểm tra runner đọc được queue không, task `READY` nào sẽ được chọn, dependency/verify là gì. `-DryRun` không gọi `codex exec`, không chạy task thật, và không tạo log artifact.

Chạy một task:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -Once
```

Lệnh này chạy đúng 1 task `READY` đầu tiên có dependency đã `DONE`. Nếu queue không có task `READY`, runner chỉ báo `No READY task` và không làm gì.

Tự plan rồi chạy một task:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -AutoPlan -Once
```

Lệnh này tự gọi planner khi không có task `READY`, sau đó chạy task vừa được tạo nếu task hợp lệ.

Chạy tối đa N task:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -MaxTasks 3
```

Số `3` chỉ là ví dụ. Có thể truyền số khác, ví dụ `-MaxTasks 1`, `-MaxTasks 2`, `-MaxTasks 5`. Runner sẽ dừng sớm nếu hết task `READY` hoặc gặp task fail/blocker.

Khuyến nghị: task thật nên chạy `-Once` trước. Chỉ dùng `-MaxTasks` khi task docs-only hoặc contract đã rất rõ.

Chạy bounded continuous:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/agent-runner.ps1 -AutoPlan -Continuous -MaxCycles 3
```

Vòng lặp luôn bị giới hạn bởi `MaxCycles`/`MaxTasks`; runner không loop vô hạn.

## Quy Trình

1. Đọc `docs/agent-queue.md`.
2. Chọn task `READY` không bị dependency block.
3. Mark `IN_PROGRESS`.
4. Đọc `prompt_file`.
5. Gọi `codex exec`.
6. Lưu log vào `temp/agent-runner/`.
7. Chạy verify command trong task.
8. Mark `DONE` hoặc `BLOCKED`.
9. Ghi checkpoint vào `checkpoint_file`.
10. Lặp task tiếp theo nếu `MaxTasks` còn.

## Khi Nào Runner Dừng

- Không còn `READY` task.
- Dependency chưa `DONE`.
- `codex exec` lỗi.
- Provider/gateway lỗi websocket/handshake.
- Model at capacity/rate limit/quota.
- Verify fail.
- Task cần owner decision.
- Runtime env bắt buộc thiếu.

## Giới Hạn

- Runner không tự vượt model limit.
- Runner không tự sửa blocker cần owner quyết định.
- Runner không tự merge branch.
- Runner không commit/push/stage.
- Runner không thay thế owner review trước khi merge.
- Runner không gọi screenshot mặc định.
- Runner không đọc archive mặc định.
- Runner không tự vượt owner decision; task `BLOCKED` chỉ được auto re-check khi `auto_retry: true` và `blocker_type` là `env_missing` hoặc `auto_recheck`.

## File Liên Quan

- Queue: `docs/agent-queue.md`
- Prompt folder: `docs/prompts/`
- Runner: `scripts/agent-runner.ps1`
- Log folder: `temp/agent-runner/` do runner tự tạo khi chạy task thật. Đây là artifact, có thể xóa sau khi đã xem kết quả.

## Thêm Task Mới

1. Tạo prompt trong `docs/prompts/<TASK_ID>.md`.
2. Thêm task marker vào `docs/agent-queue.md`.
3. Đặt `status: READY` nếu task có thể chạy ngay.
4. Ghi `depends_on` nếu cần task khác `DONE` trước.
5. Ghi `verify` và `checkpoint_file`.
6. Chạy `-DryRun` trước khi chạy thật.
