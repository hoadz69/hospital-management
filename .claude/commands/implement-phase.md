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

## Luật

- Không commit nếu owner chưa yêu cầu.
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
