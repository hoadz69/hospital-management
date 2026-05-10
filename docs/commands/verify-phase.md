# Command: Verify Phase

## Feature Team Execution Workflow

Lead Agent gọi QA Agent chạy Step 7 trong "Feature Team Execution Workflow" (chi tiết trong `docs/agent-playbook.md` và `AGENTS.md`):

- Build/typecheck/test theo lane.
- API smoke (mock + real nếu env có).
- UI route smoke + deep-link refresh.
- Edge states: loading, empty, error, 409 conflict, not-found.
- Tenant isolation/security checks nếu chạm backend/data.
- Regression risk lên lane khác.
- Acceptance criteria từng item: pass/fail/blocker.

QA Agent không sửa source FE/BE trừ khi Lead Agent cho phép vá nhỏ trong slice. Mark "Real API smoke: pending wiring" khi env chưa cung cấp.

## Multi-Workstream Lane Override

Khi verify một lane:

- Backend/DevOps verify theo `temp/plan.backend.md` và cập nhật `docs/current-task.backend.md`.
- Frontend verify theo `temp/plan.frontend.md` và cập nhật `docs/current-task.frontend.md`.
- Database/DevOps lane riêng (nếu task lớn): `temp/plan.database.md`, `temp/plan.devops.md`.
- `docs/current-task.md` chỉ cập nhật dashboard ngắn qua Lead Agent.
- `temp/plan.md` chỉ là index, không phải verify plan chi tiết.

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

Verify phase hiện tại, không code thêm.

## Luôn đọc trước

- docs/current-task.md
- docs/roadmap/clinic-saas-roadmap.md
- temp/plan.md nếu có

## Quy trình

1. Chạy `git status --short`.
2. Kiểm tra file thay đổi có nằm trong scope phase không.
3. Kiểm tra có file đáng nghi không.
4. Kiểm tra secret:
   - api_key
   - sk-
   - password
   - secret
   - token
   - connection string thật
5. Chạy verify phù hợp:

Backend:
```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
```

Frontend:
```powershell
cd frontend
npm install
npm run typecheck
npm run build
```

Infrastructure:
```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
docker compose -f infrastructure/docker/docker-compose.prod.yml config
```

6. Report pass/fail.
7. Không sửa code.
8. Không commit.

## Report format

1. File đã kiểm tra
2. Verify command đã chạy
3. Kết quả pass/fail
4. Lỗi nếu có
5. Có secret không
6. Có vượt scope không
7. Có nên commit checkpoint không
