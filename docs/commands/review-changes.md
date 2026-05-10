# Command: Review Changes

## Feature Team Execution Workflow

Lead Agent dùng review để hỗ trợ Step 9 Commit Split và Step 10 Push Gate trong "Feature Team Execution Workflow" (chi tiết trong `docs/agent-playbook.md` và `AGENTS.md`):

- Đề xuất commit split theo lane: backend / frontend / database / devops / docs+agent workflow / QA-testing (nếu lớn).
- Không gom commit lẫn lane nếu không cần.
- Push Gate: không push nếu owner chưa yêu cầu, không force push, không push nếu có secret/`.env`/temp/generated/publish/smoke artifact đang staged.
- Phát hiện file đáng nghi thì block và báo owner trước; không tự stage hoặc unstage thay owner.

## Multi-Workstream Lane Override

Khi project có nhiều workstream song song, review phải đọc dashboard `docs/current-task.md` rồi đọc lane liên quan:

- Backend/DevOps: `docs/current-task.backend.md`, `temp/plan.backend.md`.
- Frontend: `docs/current-task.frontend.md`, `temp/plan.frontend.md`.
- Database/DevOps lane riêng (nếu task lớn): `temp/plan.database.md`, `temp/plan.devops.md`.
- `temp/plan.md` chỉ là index, không phải plan chi tiết.

## Command Execution Rule

1. Khi owner yêu cầu chạy lead-plan hoặc tạo plan:
   - Làm ngay.
   - Không hỏi lại.
   - Không chỉ xác nhận đã hiểu.
   - Được phép cập nhật:
     - `temp/plan.md`
     - `docs/current-task.md`
     - `docs/roadmap/clinic-saas-roadmap.md`
   - Không code implementation.
   - Sau khi tạo plan xong mới dừng lại để owner duyệt action/implementation.

2. Khi owner yêu cầu verify/review/update-roadmap:
   - Làm ngay.
   - Không hỏi lại.
   - Không code implementation.
   - Không commit.

3. Khi owner yêu cầu implement/action/code:
   - Chỉ làm nếu plan đã được owner duyệt.
   - Nếu chưa có plan, tự chạy lead-plan trước rồi dừng lại chờ owner duyệt.
   - Không tự implement khi chưa có câu duyệt rõ như:
     - "Tôi duyệt plan"
     - "Duyệt, làm tiếp"
     - "Bắt đầu implement"
     - "Quất theo plan"

4. Không commit trừ khi owner yêu cầu rõ.
5. Không tạo Figma file mới trừ khi owner yêu cầu rõ.
6. Sau mỗi phase/task lớn phải cập nhật:
   - `docs/current-task.md`
   - `docs/roadmap/clinic-saas-roadmap.md`

## Nhiệm vụ

Review toàn bộ changes hiện tại trước khi owner commit hoặc chuyển phase.

## Luôn đọc trước

- docs/current-task.md
- docs/roadmap/clinic-saas-roadmap.md
- temp/plan.md nếu có

## Quy trình

1. Chạy:
```powershell
git status --short
git diff --stat
git diff --name-only
```

2. Nếu có staged files, chạy:
```powershell
git diff --cached --stat
git diff --cached --name-only
```

3. Kiểm tra scope:
   - File thay đổi có đúng phase không?
   - Có sửa ngoài scope không?
   - Có tạo file Figma không?
   - Có sửa frontend/backend ngoài yêu cầu không?

4. Kiểm tra secret:
```powershell
git diff | Select-String -Pattern "sk-|api_key|password|connectionString|ConnectionStrings|secret|token"
git diff --cached | Select-String -Pattern "sk-|api_key|password|connectionString|ConnectionStrings|secret|token"
```

5. Kiểm tra build/verify nếu phase yêu cầu.

6. Report:
   - Changes hợp lệ
   - File đáng nghi
   - Có secret không
   - Có vượt scope không
   - Có nên commit checkpoint không
   - Commit message đề xuất nếu owner muốn commit

## Luật

- Không code thêm.
- Không commit.
- Không tự sửa file nếu chưa được yêu cầu.
