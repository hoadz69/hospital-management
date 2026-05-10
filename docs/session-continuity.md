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

## Crash Recovery & Checkpoint Protocol

Mục tiêu: nếu Codex/Claude bị compact, hết context hoặc chết session giữa lúc đang sửa file, session mới vẫn resume được từ worktree mà không cần nhớ lại đoạn chat cũ.

### Khi Nào Phải Ghi Checkpoint

Agent phải ghi checkpoint ngắn vào lane current-task phù hợp khi gặp một trong các điều kiện sau:

- Task dự kiến kéo dài hơn 30 phút.
- Task sửa/tạo từ 5 file trở lên.
- Vừa hoàn tất một wave nhỏ trong task lớn, ví dụ layout/sidebar/topbar hoặc API adapter riêng.
- Trước khi chạy verify/build/test dài hoặc trước khi chuyển sang bước rủi ro.
- Khi phát hiện session có thể bị mất context, tool lỗi, MCP quota, hoặc cần dừng giữa chừng.

Lane ghi checkpoint:

- Frontend: `docs/current-task.frontend.md`
- Backend/DevOps: `docs/current-task.backend.md`
- DevOps riêng: `temp/plan.devops.md` nếu dashboard đã chỉ rõ lane này
- Database riêng: `temp/plan.database.md` nếu có lane riêng
- Cross-lane/Lead: `docs/current-task.md` chỉ ghi dashboard ngắn và trỏ sang lane file, không nhét chi tiết lane vào dashboard.

### Mẫu Checkpoint Bắt Buộc

```txt
## In-progress Checkpoint - YYYY-MM-DD HH:mm

Scope đang làm:
- ...

Đã hoàn thành:
- ...

File đã sửa/tạo:
- ...

Chưa verify / còn thiếu:
- ...

Lệnh đã chạy:
- ...

Lệnh cần chạy tiếp:
- ...

Bước resume tiếp theo:
1. Chạy git status --short.
2. Chạy git diff --stat.
3. Đọc diff các file trong scope.
4. Tiếp tục từ ...

Guardrail:
- Không revert thay đổi chưa rõ chủ sở hữu.
- Không commit/push nếu owner chưa yêu cầu.
```

Checkpoint không thay thế report cuối cùng và không được dùng để đánh dấu Done. Nếu verify chưa chạy thì ghi rõ "chưa verify".

### Quy Trình Resume Sau Khi Session Chết

Session mới phải ưu tiên trạng thái repo thật:

```powershell
git status --short
git diff --stat
git diff --check
```

Sau đó đọc lane checkpoint gần nhất và diff file trong scope:

```powershell
git diff -- frontend/apps/owner-admin frontend/packages/ui
```

Nguyên tắc resume:

- Không revert thay đổi đang dở nếu chưa rõ là của ai.
- Không tự code tiếp chỉ dựa vào đoạn chat cũ; phải đối chiếu `git diff` và lane checkpoint.
- Nếu diff có file ngoài scope, báo owner và bỏ qua hoặc hỏi nếu nó chặn task.
- Nếu checkpoint nói đã verify nhưng repo hiện tại khác diff, chạy verify lại.
- Nếu không có checkpoint, tạo "recovery summary" từ `git status` + `git diff --stat` trước khi làm tiếp.

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
5. Ghi checkpoint giữa chừng nếu task dài/sửa nhiều file theo "Crash Recovery & Checkpoint Protocol".
6. Cập nhật `docs/current-task.md` hoặc lane current-task phù hợp.
7. Report lại cho owner: đã làm gì, file nào, kiểm tra gì, còn thiếu/bị chặn gì, bước tiếp theo.

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
Nếu task dài hoặc sửa trên 5 file, ghi checkpoint ngắn vào lane current-task sau từng wave nhỏ.
Sau khi làm xong cập nhật docs/current-task.md hoặc lane current-task phù hợp và report lại file đã sửa, lệnh đã kiểm tra, phần còn thiếu.
Nếu thay đổi làm lệch roadmap thì cập nhật plan.md.
```

Khi resume session chết:

```txt
Session trước chết giữa lúc implement. Đọc AGENTS.md, docs/session-continuity.md, docs/current-task.md và lane current-task liên quan.
Resume từ git status + git diff, không revert thay đổi đang dở.
Tóm tắt diff hiện tại, đối chiếu checkpoint gần nhất, chạy verify phù hợp rồi tiếp tục đúng scope.
```
