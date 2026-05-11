# Command: Review Changes

## Nhiệm vụ

Review toàn bộ changes hiện tại trước khi owner commit hoặc chuyển phase.

Nếu owner gọi ngắn `Lead Agent: chia commit <task>`, đây là action trigger thật, không acknowledge-only. Lead Agent phải chạy `git status --branch --short`, `git diff --stat`, review/scope/secret/artifact check, tự chọn Documentation/QA nếu cần, rồi đề xuất commit split theo lane. Chỉ stage/commit khi owner yêu cầu rõ; nếu owner chỉ hỏi "chia commit" thì không tự stage.

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
   - File nào tuyệt đối không stage: `.claude/settings.local.json`, screenshot/log/generated artifacts, file ngoài scope.
   - Dirty/untracked còn lại và xác nhận không push.

## Luật

- Không code thêm.
- Không commit.
- Không stage/push nếu owner chưa yêu cầu rõ.
- Không stage/commit artifact/log/screenshot/generated files hoặc `.claude/settings.local.json`.
- Không xóa source/docs/plan dirty nếu chưa rõ chủ sở hữu.
- Không tự sửa file nếu chưa được yêu cầu.
