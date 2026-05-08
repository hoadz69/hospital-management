# Command: Update Roadmap

## Nhiệm vụ

Cập nhật roadmap và current task sau phase/task lớn.

## Luôn cập nhật

- docs/current-task.md
- docs/roadmap/clinic-saas-roadmap.md

## Status legend

- ✅ Done
- 🟡 In Progress
- 🔜 Next
- ❌ Blocked
- ⏸ Paused

## Quy trình

1. Đọc `docs/current-task.md`.
2. Đọc `docs/roadmap/clinic-saas-roadmap.md`.
3. Xác định phase vừa xong.
4. Nếu verify pass, đánh phase vừa xong là `✅ Done`.
5. Nếu task đang làm, đánh là `🟡 In Progress`.
6. Nếu task tiếp theo đã xác định, đánh là `🔜 Next`.
7. Nếu bị lỗi/chặn, đánh là `❌ Blocked`.
8. Cập nhật phần `Current Status`.
9. Cập nhật phần `Current Next Actions`.
10. Không sửa code.
11. Không commit.

## Luật

- Không được để report nói Done nhưng roadmap vẫn In Progress.
- Không được để current-task khác với roadmap.
- Không tự thêm phase mới nếu chưa có lý do rõ.
- Nếu có thêm phase mới, phải ghi rõ lý do.
