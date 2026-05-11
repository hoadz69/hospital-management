# Command: Implement Phase

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

Nếu owner gọi ngắn `Lead Agent: bắt đầu <task>` hoặc `Lead Agent: làm tiếp <task>`, đây là action trigger thật. Không acknowledge-only. Trước khi implement phải chạy `git status --branch --short`, `git diff --stat`, tự chạy Feature Team Execution Workflow: phân lane, đọc lane current-task/plan/handoff, tự chọn agents theo scope. Nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap với allowed files/file areas và acceptance/verify rõ, xem là resumable/approved scope và implement/resume ngay. Owner không cần paste "Agents tham gia".

## Quy trình

1. Chạy `git status --short` trước khi sửa.
2. Đọc scope trong `temp/plan.md` hoặc lane plan tương ứng (`temp/plan.frontend.md`, `temp/plan.backend.md`, `temp/plan.devops.md`, `temp/plan.database.md`).
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

## Luật

- Không commit nếu owner chưa yêu cầu.
- Không stage nếu owner chưa yêu cầu.
- Không push.
- Không stage `.claude/settings.local.json`, screenshot/log/generated artifacts.
- Không xóa source/docs/plan dirty nếu chưa rõ chủ sở hữu.
- Không dùng secret thật.
- Không hard-code connection string production.
- Không tạo Figma file mới.
- Không sửa frontend nếu phase là backend-only.
- Không sửa backend nếu phase là frontend-only.
- Không implement business logic ngoài scope.
- Không bỏ tenant isolation.
- Không tạo database/migration nếu phase chưa cho phép.
- Mỗi phase phải có verify.
- Sau mỗi phase/task lớn phải update roadmap.
