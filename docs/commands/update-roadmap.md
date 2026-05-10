# Command: Update Roadmap

## Multi-Workstream Lane Override

Khi project có nhiều workstream song song:

- Roadmap phải thể hiện các lane song song, không ghi sai là chỉ có một current task duy nhất.
- `docs/current-task.md` là dashboard.
- Backend/DevOps status lấy từ `docs/current-task.backend.md` và `temp/plan.backend.md`.
- Frontend status lấy từ `docs/current-task.frontend.md` và `temp/plan.frontend.md`.

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
