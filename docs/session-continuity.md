# Session Continuity Guide

File này dùng để chống mất ngữ cảnh khi session Codex/Claude bị xóa hoặc hết hạn.

## File Cần Đọc Khi Bắt Đầu Session Mới

1. `AGENTS.md`
2. `CLAUDE.md` nếu dùng Claude Code
3. `clinic_saas_report.md`
4. `architech.txt`
5. `docs/current-task.md`
6. `docs/agent-playbook.md`
7. `rules/*.md` nếu chuẩn bị viết code

## Phân Biệt Các File Plan / Handoff

- `plan.md`: roadmap/kế hoạch tổng của repo. Cập nhật khi hướng triển khai hoặc milestone thay đổi.
- `temp/plan.md`: plan cụ thể cho task sắp code. Với implementation task, phải tạo/cập nhật file này trước và chờ owner duyệt.
- `docs/current-task.md`: handoff hiện tại. Cập nhật sau mỗi lượt làm việc hoặc khi dừng giữa chừng.
- `rules/*.md`: rule kỹ thuật. Chỉ cập nhật khi có convention mới, bug pattern mới, hoặc quyết định kiến trúc mới được owner chốt.

## Khi Owner Yêu Cầu Code Thật

Trước khi code:

1. Đọc source of truth và rules.
2. Tạo/cập nhật `temp/plan.md`.
3. Plan phải có:
   - scope,
   - assumptions,
   - file dự kiến sửa/tạo,
   - success criteria,
   - verification steps,
   - tenant isolation impact,
   - Figma/FigJam reference nếu liên quan UI/architecture.
4. Chờ owner duyệt.

Sau khi owner duyệt:

1. Implement đúng approved scope.
2. Không refactor ngoài scope.
3. Dọn unused code do chính thay đổi tạo ra.
4. Verify theo plan.
5. Cập nhật `docs/current-task.md`.
6. Report lại cho owner: đã làm gì, file nào, kiểm tra gì, còn thiếu/bị chặn gì, bước tiếp theo.

## Khi Nào Cập Nhật `plan.md`

Cập nhật `plan.md` nếu:

- roadmap thay đổi,
- thêm/bớt milestone,
- chuyển phase,
- owner đổi ưu tiên,
- task implementation làm thay đổi hướng triển khai tổng.

Không cập nhật `plan.md` cho thay đổi nhỏ không ảnh hưởng roadmap.

## Khi Nào Cập Nhật `rules/*.md`

Không tự sửa `rules/*.md` liên tục trong lúc code.

Chỉ cập nhật rules khi:

- phát hiện bug pattern có thể lặp lại,
- owner chốt convention mới,
- có quyết định kiến trúc mới,
- tool/test/deploy workflow thay đổi,
- rule cũ sai hoặc thiếu so với thực tế project.

Nếu không chắc, đề xuất rule mới trong report trước, chờ owner duyệt rồi mới sửa.

## Prompt Gợi Ý Cho Session Mới

```txt
Đọc AGENTS.md, clinic_saas_report.md, docs/current-task.md, docs/session-continuity.md và rules/*.md.
Tạo/cập nhật temp/plan.md cho task này trước, chưa code.
Nếu thấy plan.md hoặc docs/current-task.md cần cập nhật thì ghi rõ lý do.
Nếu phát hiện rule code mới cần chuẩn hóa thì đề xuất trước, chưa tự sửa rules/*.md.
```

Khi duyệt code:

```txt
Tôi duyệt temp/plan.md. Bắt đầu implement đúng approved scope.
Sau khi làm xong cập nhật docs/current-task.md và report lại file đã sửa, lệnh đã kiểm tra, phần còn thiếu.
Nếu thay đổi làm lệch roadmap thì cập nhật plan.md.
```
