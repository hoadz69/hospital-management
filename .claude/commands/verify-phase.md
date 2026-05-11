# Command: Verify Phase

## Nhiệm vụ

Verify phase hiện tại, không code thêm.

Nếu owner gọi ngắn `Lead Agent: verify <task>`, đây là action trigger thật, không acknowledge-only. Lead Agent phải chạy `git status --branch --short`, `git diff --stat`, tự phân lane, tự chọn QA/Documentation và agents liên quan theo scope, đọc lane plan/current-task/handoff, rồi chạy checklist verify phù hợp. Owner không cần paste "Agents tham gia".

## Luôn đọc trước

- docs/current-task.md
- docs/roadmap/clinic-saas-roadmap.md
- temp/plan.md nếu có

## Quy trình

1. Chạy `git status --short` hoặc `git status --branch --short` nếu owner yêu cầu branch.
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
9. Không stage/push; không stage screenshot/log/generated artifacts.
10. Không xóa source/docs/plan dirty nếu chưa rõ chủ sở hữu.
11. Fast Mode verify mặc định không gọi Figma/screenshot/full team nếu owner không yêu cầu visual QA; PASS chỉ ghi PASS, FAIL chỉ paste lỗi liên quan.
12. Visual QA budget chỉ chụp tối đa 1 desktop chính + 1 mobile chính, thêm tối đa 2 ảnh nếu thật sự quan trọng; nhiều route thì tạo contact sheet/collage.

## Report format

1. File đã kiểm tra
2. Verify command đã chạy
3. Kết quả pass/fail
4. Lỗi nếu có
5. Có secret không
6. Có vượt scope không
7. Có nên commit checkpoint không
8. Dirty/untracked còn lại, artifact/log không commit, và xác nhận không commit/push/stage
