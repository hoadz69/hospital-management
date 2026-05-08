# Codex Command Templates - Clinic SaaS

File này dùng để tạo bộ command template trong repo, để từ sau không phải dán prompt dài nữa.

---

## 1. Cách dùng nhanh

Copy nguyên prompt dưới đây vào Codex **một lần duy nhất**:

```txt
Tạo bộ command template để từ giờ tôi không phải dán prompt dài nữa.

Tạo các file:

docs/commands/lead-plan.md
docs/commands/implement-phase.md
docs/commands/verify-phase.md
docs/commands/update-roadmap.md
docs/commands/review-changes.md

Dùng đúng nội dung trong file codex_command_templates.md tôi cung cấp.

Yêu cầu:
- Không sửa code.
- Không tạo Figma file mới.
- Không commit.
- Sau khi tạo xong, report file đã tạo và cách dùng ngắn gọn.
```

Sau khi Codex tạo xong, từ sau chỉ cần nói ngắn:

```txt
Chạy docs/commands/lead-plan.md cho Phase 2 - Tenant MVP Backend.
```

Sau khi duyệt plan:

```txt
Tôi duyệt plan. Chạy docs/commands/implement-phase.md theo temp/plan.md.
```

Khi cần verify:

```txt
Chạy docs/commands/verify-phase.md.
```

Khi cần cập nhật roadmap:

```txt
Chạy docs/commands/update-roadmap.md.
```

Khi cần review trước commit:

```txt
Chạy docs/commands/review-changes.md.
```

---

# File: docs/commands/lead-plan.md

```md
# Command: Lead Plan

Bạn là Lead / Orchestrator Agent của project Clinic SaaS Multi Tenant.

## Luôn đọc trước

- AGENTS.md
- CLAUDE.md
- clinic_saas_report.md
- architech.txt
- docs/current-task.md
- docs/roadmap/clinic-saas-roadmap.md
- docs/architecture/overview.md
- docs/architecture/service-boundaries.md
- docs/architecture/tenant-isolation.md
- temp/plan.md nếu có

## Nhiệm vụ

Tạo plan cho phase/task owner yêu cầu.

## Quy trình

1. Tóm tắt phase/task.
2. Xác định phase hiện tại trong roadmap.
3. Xác định phase/task tiếp theo.
4. Chia việc cho agent liên quan:
   - Architect Agent
   - Backend Agent
   - Frontend Agent
   - Database Agent
   - DevOps Agent
   - QA Agent
   - Documentation Agent
5. Ghi rõ scope.
6. Ghi rõ out-of-scope.
7. Ghi success criteria.
8. Ghi verify plan.
9. Ghi rủi ro/điểm cần owner duyệt.
10. Cập nhật `docs/current-task.md`.
11. Cập nhật `docs/roadmap/clinic-saas-roadmap.md`.
12. Ghi plan vào `temp/plan.md`.
13. Dừng lại để owner duyệt.

## Luật

- Chưa code.
- Không commit.
- Không tạo Figma file mới.
- Không tự làm nhiều phase một lúc.
- Không đổi architecture nếu chưa ghi rõ trong plan.
- Không bỏ tenant isolation.
- Nếu task ảnh hưởng roadmap, phải cập nhật roadmap status.
- Nếu task ảnh hưởng current task, phải cập nhật current-task.
```

---

# File: docs/commands/implement-phase.md

```md
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
```

---

# File: docs/commands/verify-phase.md

```md
# Command: Verify Phase

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
```

---

# File: docs/commands/update-roadmap.md

```md
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
```

---

# File: docs/commands/review-changes.md

```md
# Command: Review Changes

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
```

---

## 2. Prompt dùng sau khi tạo command

### Tạo plan Phase 2

```txt
Chạy docs/commands/lead-plan.md cho Phase 2 - Tenant MVP Backend.
```

### Duyệt và implement Phase 2

```txt
Tôi duyệt plan. Chạy docs/commands/implement-phase.md theo temp/plan.md.
```

### Verify

```txt
Chạy docs/commands/verify-phase.md.
```

### Update roadmap

```txt
Chạy docs/commands/update-roadmap.md.
```

### Review trước commit

```txt
Chạy docs/commands/review-changes.md.
```
