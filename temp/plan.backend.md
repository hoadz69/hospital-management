# Kế Hoạch Backend/DevOps - Phase 2 API Runtime Smoke Gate

Ngày cập nhật: 2026-05-10

Trạng thái: ✅ Phase 2 API Runtime Smoke Gate PASS đủ 5 case trên server `116.118.47.78` sau hai vòng fix (Dapper type handler + reorder positional record `TenantListRow`). Lane sẵn sàng cho Lead Agent đồng bộ dashboard + roadmap để chuyển Phase 2 sang Done.

Chế độ thực hiện: Backend/DevOps lane riêng. Không sửa frontend, không sửa Figma, không commit, không push, không ghi secret/IP/private key vào repo.

## 1. Tóm Tắt Task

Phase 2 Tenant MVP Backend đã có local commit:

```txt
71923f8 feat: implement tenant mvp backend slice
```

Static/build/config đã pass. DB runtime smoke local từng bị blocked vì Docker daemon local chưa chạy. Owner đã cho dùng server riêng để bootstrap Docker/PostgreSQL.

Bootstrap server đã hoàn tất:

```txt
SSH: pass
OS: Ubuntu 22.04.5 LTS
CPU/RAM/disk: 4 cores, 5.8 GiB RAM, root disk 64G
Docker Engine: 29.4.3
Docker Compose plugin: v5.1.3
Docker info: pass
Deploy directory: /opt/clinic-saas
Docker network: clinic-saas-network
PostgreSQL container: clinic-saas-postgres, postgres:16-alpine, Up
PostgreSQL publish port: none
Schema Phase 2: apply pass
Tables: platform.tenants, platform.tenant_profiles, platform.tenant_domains, platform.tenant_modules
```

Phase 2 vẫn chưa Done vì chưa chạy API smoke qua `tenant-service`/`api-gateway`.

## 2. Mục Tiêu Hiện Tại

Chạy API runtime smoke cho Phase 2 bằng PostgreSQL server đã bootstrap:

```txt
tenant-service -> PostgreSQL container clinic-saas-postgres
api-gateway -> tenant-service
smoke endpoints -> pass
```

## 3. Scope Được Làm

Backend/DevOps có thể làm trong phạm vi:

```txt
backend/services/tenant-service
backend/services/api-gateway
backend/shared nếu cần cho runtime config/smoke
infrastructure/docker nếu cần chạy services đúng network
docs/current-task.backend.md
temp/plan.backend.md
docs/deployment/server-bootstrap.md nếu kết quả deploy/smoke thay đổi
docs/roadmap/clinic-saas-roadmap.md nếu status phase thay đổi
```

Không sửa:

```txt
frontend/
Figma
Clinic Admin/Public Website UI
feature ngoài Tenant MVP API smoke
```

## 4. Out Of Scope

- Không commit nếu owner chưa yêu cầu.
- Không push.
- Không ghi private key vào repo.
- Không ghi secret thật vào file tracked.
- Không ghi IP server thật vào repo.
- Không mở PostgreSQL public ra internet.
- Không expose port `5432` ra `0.0.0.0`.
- Không dùng MySQL.
- Không cài package ngoài scope runtime smoke.
- Không sửa frontend.
- Không tạo Figma file.
- Không chuyển Phase 2 sang Done nếu API smoke chưa pass thật.

## 5. Plan Chạy Smoke

### 5.1 Chuẩn Bị Runtime

1. Xác nhận PostgreSQL container còn chạy:

```bash
docker ps --filter name=clinic-saas-postgres
docker exec clinic-saas-postgres pg_isready -U clinic_dev -d clinic_saas_dev
```

2. Xác nhận schema còn đủ:

```bash
docker exec clinic-saas-postgres psql -U clinic_dev -d clinic_saas_dev -Atc "select schemaname || '.' || tablename from pg_tables where schemaname='platform' order by tablename;"
```

3. Chọn cách chạy services:

```txt
Ưu tiên A: chạy tenant-service và api-gateway trong cùng Docker network clinic-saas-network.
Dự phòng B: chạy service từ máy dev qua SSH tunnel nếu runtime container hóa chưa sẵn.
```

### 5.2 Chạy `tenant-service`

- Trỏ connection string tới PostgreSQL host `clinic-saas-postgres` nếu chạy cùng Docker network.
- Không ghi connection string thật vào file tracked.
- Chỉ dùng env var hoặc secret tạm trong shell/runtime.
- Verify:

```txt
GET /health
GET /openapi/v1.json
```

### 5.3 Chạy `api-gateway`

- Trỏ TenantService base URL tới `tenant-service`.
- Verify:

```txt
GET /health
GET /openapi/v1.json
```

### 5.4 API Smoke Bắt Buộc

Chạy qua gateway nếu khả thi, đồng thời có thể kiểm tra trực tiếp tenant-service khi cần debug:

```txt
POST /api/tenants
GET /api/tenants
GET /api/tenants/{id}
PATCH /api/tenants/{id}/status
POST duplicate slug/domain => 409
```

## 6. Success Criteria

Phase 2 chỉ được chuyển Done khi:

- `tenant-service` chạy được với PostgreSQL server đã bootstrap.
- `api-gateway` gọi được `tenant-service`.
- `POST /api/tenants` tạo tenant và ghi DB thành công.
- `GET /api/tenants` trả danh sách có tenant vừa tạo.
- `GET /api/tenants/{id}` trả đúng tenant.
- `PATCH /api/tenants/{id}/status` cập nhật status đúng.
- Duplicate slug/domain trả `409`.
- Không expose PostgreSQL public.
- Docs lane backend và roadmap được cập nhật đúng kết quả thật.

## 7. Verify Plan

Local hoặc remote tùy cách chạy:

```powershell
git status --branch --short
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build
docker compose -f infrastructure/docker/docker-compose.dev.yml config
```

Runtime smoke:

```bash
curl -i <gateway-or-tenant-service>/health
curl -i <gateway-or-tenant-service>/openapi/v1.json
curl -i -X POST <gateway>/api/tenants ...
curl -i <gateway>/api/tenants
curl -i <gateway>/api/tenants/{id}
curl -i -X PATCH <gateway>/api/tenants/{id}/status ...
curl -i -X POST <gateway>/api/tenants ...duplicate...
```

Port check:

```bash
docker port clinic-saas-postgres
ss -tulpen
ufw status verbose
```

Kỳ vọng:

```txt
Không có PostgreSQL listen public 0.0.0.0:5432.
```

## 8. Rủi Ro Và Cách Xử Lý

- Nếu SSH fail: dừng và report lỗi xác thực/kết nối.
- Nếu container PostgreSQL không còn chạy: inspect trạng thái, không drop volume ngoài scope.
- Nếu migration đã apply: verify tables, không drop schema.
- Nếu service không đọc được env: sửa config runtime tối thiểu trong backend/devops scope, không ghi secret.
- Nếu gateway forward lỗi: kiểm tra TenantService base URL và network DNS.
- Nếu API trả 500: kiểm tra log, map lỗi rõ, không expose stack trace ngoài Development.
- Nếu duplicate không trả 409: giữ Phase 2 In Progress và ghi blocker.

## 9. Điểm Dừng

Dừng sau khi API smoke pass hoặc khi có blocker rõ. Cập nhật:

```txt
docs/current-task.backend.md
temp/plan.backend.md
docs/roadmap/clinic-saas-roadmap.md
```

Nếu API smoke pass, Lead Agent mới được cập nhật dashboard và roadmap để chuyển Phase 2 Done.

## 10. Cập Nhật 2026-05-10 - Root Cause GET/PATCH 500

### 10.1 Triệu chứng smoke

```txt
POST /api/tenants -> 201 (pass)
duplicate POST /api/tenants -> 409 (pass)
GET /api/tenants -> 500 (fail)
GET /api/tenants/{id} -> 500 (fail)
PATCH /api/tenants/{id}/status -> 500 (fail)
```

### 10.2 Root cause

Schema PostgreSQL Phase 2 dùng `timestamptz` cho mọi cột `*_at_utc`. Từ Npgsql 6.0 trở lên, provider mặc định trả `timestamptz` về `System.DateTime` với `Kind=Utc`, không còn về `System.DateTimeOffset` như trước. Tất cả row record trong `DapperTenantRepository` (`TenantRootRow`, `TenantListRow`, `TenantDomainRow`, `TenantModuleRow`) đang khai báo property dạng `DateTimeOffset` / `DateTimeOffset?`. Khi Dapper map kết quả SELECT, `InvalidCastException` ném ra → ASP.NET Core trả 500.

POST tạo tenant pass vì `CreateTenantHandler` map response từ aggregate in-memory ngay sau insert, không re-read DB. Đường read/update mới chạm code path lỗi:

```txt
GET list -> DapperTenantRepository.ListAsync -> TenantListRow
GET id -> DapperTenantRepository.GetByIdAsync -> LoadTenantAsync -> TenantRootRow + TenantDomainRow + TenantModuleRow
PATCH status -> DapperTenantRepository.UpdateStatusAsync -> LoadTenantAsync -> cùng các row record trên
```

Tài liệu tham chiếu:

```txt
https://www.npgsql.org/doc/types/datetime.html
https://www.npgsql.org/doc/release-notes/6.0.html
```

### 10.3 Minimal fix đã apply

Đăng ký Dapper `SqlMapper.TypeHandler<DateTimeOffset>` để bridge `DateTime` UTC ↔ `DateTimeOffset` UTC. Không thay schema, không sửa domain entity, không sửa public contracts, không thêm dependency.

File mới:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DateTimeOffsetTypeHandler.cs
```

File modified:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs
```

`DependencyInjection.AddTenantServiceInfrastructure(...)` gọi `RegisterDapperTypeHandlersOnce()` đúng một lần per process bằng `Interlocked.Exchange` guard. Comment/XML doc tiếng Việt theo `rules/backend-coding-rules.md`.

### 10.4 Verify local

```txt
& 'C:\Program Files\dotnet\dotnet.exe' restore backend/ClinicSaaS.Backend.sln: pass
& 'C:\Program Files\dotnet\dotnet.exe' build backend/ClinicSaaS.Backend.sln --no-restore: pass, 0 warnings, 0 errors
& 'C:\Program Files\dotnet\dotnet.exe' test backend/ClinicSaaS.Backend.sln --no-build: exit code 0
```

Test runner không pick up test case nào vì project `tests/TenantService.Tests` hiện là placeholder không reference xUnit/NUnit/MSTest. Đây là gap có sẵn trước Phase 2, không thuộc scope smoke gate.

### 10.5 DevOps action cần làm tiếp

```txt
1. Trên server, rebuild Docker image cho tenant-service từ source mới nhất (chứa DateTimeOffsetTypeHandler).
2. Restart container clinic-saas-tenant-service trong Docker network clinic-saas-network.
3. api-gateway không cần đổi (pure pass-through, không sửa code).
4. Smoke lại 5 case Phase 2 qua api-gateway:
   - POST /api/tenants -> 201
   - duplicate slug/domain -> 409
   - GET /api/tenants -> 200 + JSON list
   - GET /api/tenants/{id} -> 200 + JSON detail
   - PATCH /api/tenants/{id}/status body {"status":"Active"} -> 200 + JSON detail
5. Nếu có endpoint vẫn 500, lấy log:
   docker logs clinic-saas-tenant-service --tail=200
   Tìm chuỗi "Unable to cast" hoặc stack trace InvalidCastException.
6. Không expose 5432 ra 0.0.0.0. Không ghi connection string thật vào file tracked.
```

DevOps lane chỉ có thể chạy khi owner cung cấp lại biến shell `DEPLOY_HOST`/`DEPLOY_USER`/`SSH_KEY_PATH` trong phiên hiện tại. Lead Agent không tự dùng SSH/server thật khi owner chưa cung cấp ở phiên này.

### 10.6 Done condition Phase 2

Phase 2 chỉ chuyển Done sau khi cả 5 smoke trên pass thật trên server đã rebuild. Trước khi pass đủ, dashboard và roadmap phải giữ Phase 2 ở trạng thái `🟡 In Progress`.

## 11. Cập Nhật 2026-05-10 - Smoke Pass Đủ

### 11.1 Kết quả vòng 1 deploy

Sau khi deploy fix vòng 1 (Dapper type handler) lên server bằng pattern publish + scp + swap dir + restart:

```txt
POST /api/tenants -> 201 (pass)
duplicate -> 409 (pass)
GET /api/tenants/{id} -> 200 (pass)
PATCH /api/tenants/{id}/status -> 200 (pass)
GET /api/tenants -> 500 (fail tiếp, lý do khác)
```

Log container chỉ còn `InvalidOperationException` cho `TenantListRow` materialization, không còn `InvalidCastException` trên các path khác.

### 11.2 Root cause vòng 2

Dapper 2.1 với positional record materialize theo position type, không lookup type handler khi resolve constructor. `TenantListRow` có `ClinicName` ở position 6 trong ctor nhưng SQL SELECT trong `ListAsync` đặt `clinic_name` cuối cùng (position 11) vì JOIN `tenant_profiles`. Type lệch tại position 6 và 11.

`TenantRootRow`, `ClinicProfileRow`, `TenantDomainRow`, `TenantModuleRow` không gặp vì ctor order đã khớp SELECT.

### 11.3 Fix vòng 2

Reorder ctor `TenantListRow` đúng thứ tự cột SELECT (`ClinicName` cuối). Mapping `ToTenantSummary(TenantListRow row)` truy cập field theo NAME nên reorder không phá logic.

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantRepository.cs (MODIFIED)
```

Thêm comment giải thích invariant order.

### 11.4 Verify Local v2

```txt
dotnet build backend/ClinicSaaS.Backend.sln --nologo: pass, 0 warnings, 0 errors
dotnet test backend/ClinicSaaS.Backend.sln --no-build --nologo: không pickup test case (placeholder)
```

### 11.5 Deploy v2 và Smoke Final

```txt
dotnet publish ... -o temp/publish/tenant-service: pass
scp -r ... root@116.118.47.78:/opt/clinic-saas/runtime-smoke/tenant-service-new/: pass
docker stop clinic-saas-tenant-service-smoke: pass
mv tenant-service tenant-service.bak.20260509T191905Z: pass
mv tenant-service-new tenant-service: pass
docker start clinic-saas-tenant-service-smoke: pass
curl http://127.0.0.1:5006/health: 200 Healthy
curl http://127.0.0.1:5005/health: 200 Healthy
docker logs --since=2m: chỉ còn warning libgssapi_krb5.so.2 cannot open (harmless GSSAPI)
```

Smoke 5 case qua `api-gateway` (`127.0.0.1:5005`) với payload `demo-clinic-smoke-003`:

```txt
POST /api/tenants -> 201
duplicate POST -> 409
GET /api/tenants -> 200 (response items array trả tenant list với clinicName, createdAtUtc, updatedAtUtc đúng)
GET /api/tenants/{id} -> 200 (response detail với profile, domains, modules)
PATCH /api/tenants/{id}/status body {"status":"Active"} -> 200 (status chuyển từ Draft sang Active)
```

Kết luận: Phase 2 đủ điều kiện chuyển Done.

### 11.6 File backend đã sửa trong session này

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DateTimeOffsetTypeHandler.cs (NEW, vòng 1)
backend/services/tenant-service/src/TenantService.Infrastructure/DependencyInjection.cs (MODIFIED, vòng 1)
backend/services/tenant-service/src/TenantService.Infrastructure/Persistence/DapperTenantRepository.cs (MODIFIED, vòng 2 - reorder TenantListRow)
```

File helper local (không bắt buộc commit):

```txt
temp/smoke.sh (script smoke 5 case dùng curl + python3 trên server)
temp/publish/tenant-service/ (publish output, ngoài git)
```

Owner chưa yêu cầu commit/push.
