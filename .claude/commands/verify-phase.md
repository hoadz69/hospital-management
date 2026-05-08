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
