# Command: Implement Phase

## Multi-Workstream Lane Override

Khi project có nhiều workstream song song:

- `docs/current-task.md` chỉ là Project Coordination Dashboard.
- `temp/plan.md` chỉ là index tương thích cũ.
- Backend/DevOps implement theo `temp/plan.backend.md` và cập nhật `docs/current-task.backend.md`.
- Frontend implement theo `temp/plan.frontend.md` và cập nhật `docs/current-task.frontend.md`.
- Không agent nào ghi task chi tiết của một lane vào `docs/current-task.md`.

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

Bạn là implementation agent theo plan đã được owner duyệt.

## Luôn đọc trước

- AGENTS.md
- CLAUDE.md
- docs/current-task.md
- docs/roadmap/clinic-saas-roadmap.md
- temp/plan.md
- docs/architecture/overview.md
- docs/architecture/service-boundaries.md
- docs/architecture/tenant-isolation.md

## Nhiệm vụ

Implement đúng phase trong `temp/plan.md`.

## Quy trình

1. Chạy `git status --short` trước khi sửa.
2. Đọc scope trong `temp/plan.md`.
3. Chỉ sửa file nằm trong scope đã duyệt.
4. Không mở rộng scope.
5. Không tự làm phase kế tiếp.
6. Implement đúng yêu cầu.
7. Chạy verify theo plan.
8. Nếu verify pass, cập nhật:
   - `docs/current-task.md`
   - `docs/roadmap/clinic-saas-roadmap.md`
9. Nếu verify fail, cập nhật current-task là Blocked hoặc In Progress, ghi rõ lỗi.
10. Report lại owner.

## Bắt buộc sau khi implement xong

Sau khi implement xong, agent phải:

1. Chạy verify theo `temp/plan.md`.
2. Nếu có thay đổi backend, chạy restore/build/test nếu repo có command hoặc solution/project tương ứng.
3. Nếu có thay đổi infra, chạy `docker compose config`.
4. Nếu có runtime smoke test khả thi, chạy smoke test.
5. Cập nhật `docs/current-task.md`.
6. Cập nhật `docs/roadmap/clinic-saas-roadmap.md`.
7. Report lại owner gồm:
   - File đã sửa.
   - Đã implement gì.
   - Verify result.
   - `docs/current-task.md` và `docs/roadmap/clinic-saas-roadmap.md` đã cập nhật ra sao.
   - Bước tiếp theo.

## Luật

- Không commit nếu owner chưa yêu cầu.
- Không dùng secret thật.
- Không hard-code connection string production.
- Không tạo Figma file mới.
- Không sửa ngoài scope `temp/plan.md`.
- Không sửa frontend nếu phase là backend-only.
- Không sửa backend nếu phase là frontend-only.
- Không implement business logic ngoài scope.
- Không bỏ tenant isolation.
- Không tạo database/migration nếu phase chưa cho phép.
- Mỗi phase phải có verify.
- Sau mỗi phase/task lớn phải update roadmap.
