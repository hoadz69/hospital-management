# Server Bootstrap - Clinic SaaS Phase 2 Smoke

Tài liệu này mô tả cách bootstrap server do owner cung cấp để chạy Docker/PostgreSQL cho Phase 2 DB runtime smoke.

Không lưu IP server thật, private key, token hoặc secret production trong file này.

## Server Test Runtime Rule

Server test/dev smoke do owner cung cấp qua `DEPLOY_HOST`, `DEPLOY_USER`, `SSH_KEY_PATH` trong shell hoặc file local đã ignore như `deploy.local.ps1` là runtime chính cho backend/DB/API smoke khi local Windows thiếu Docker/.NET hoặc Docker daemon không chạy. Agent dùng SSH/SCP để deploy/check code trong phạm vi project, không ghi host/user thật, private key, token, secret hoặc connection string thật vào repo/docs/log.

- PostgreSQL chạy trong Docker network/server nội bộ, không publish `5432` public và không mở DB ra internet.
- Tenant Service/API Gateway chạy trên server test; FE real API smoke trỏ Vite proxy tới API Gateway thật hoặc SSH tunnel tới gateway server.
- Stub chỉ dùng fallback cuối cùng để verify contract path; không dùng stub để đánh dấu E2E Done nếu server test có thể chạy API thật.

## Mục Tiêu

```txt
Server
→ Docker Engine + Docker Compose plugin
→ Docker network clinic-saas-network
→ PostgreSQL dev/smoke container
→ Phase 2 Tenant MVP schema
```

## Quy Tắc Bảo Mật

- Không expose PostgreSQL public.
- Không publish `5432` ra `0.0.0.0`.
- Ưu tiên truy cập PostgreSQL bằng Docker network, `docker exec`, hoặc SSH tunnel.
- Chỉ dùng dev placeholder cho DB smoke.
- Không dùng MySQL.
- Không đưa private key hoặc secret vào repo.

## Biến Cục Bộ Khi Chạy Từ Máy Dev

Các biến này chỉ đặt trong shell local hoặc `deploy.local.ps1` đã được gitignore, không ghi giá trị thật vào file tracked:

```powershell
$env:DEPLOY_HOST='<owner-provided-host>'
$env:DEPLOY_USER='<owner-provided-user>'
$env:SSH_KEY_PATH='<local-private-key-path>'
```

Nếu dùng file local, tạo `deploy.local.ps1` ở repo root trên từng máy rồi nạp trước khi smoke:

```powershell
. .\deploy.local.ps1
```

## Kiểm Tra Server

```bash
cat /etc/os-release
nproc
free -h
df -h /
```

## Docker Verify

```bash
docker --version
docker compose version
docker info
```

## PostgreSQL Smoke Container

Container dự kiến:

```txt
name: clinic-saas-postgres
network: clinic-saas-network
volume: clinic-saas-postgres-data
database: clinic_saas_dev
user: clinic_dev
port: không publish public
```

Ví dụ chạy container theo hướng không publish public port:

```bash
docker network create clinic-saas-network
docker volume create clinic-saas-postgres-data
docker run -d \
  --name clinic-saas-postgres \
  --network clinic-saas-network \
  -e POSTGRES_DB=clinic_saas_dev \
  -e POSTGRES_USER=clinic_dev \
  -e POSTGRES_PASSWORD='<dev-placeholder-password>' \
  -v clinic-saas-postgres-data:/var/lib/postgresql/data \
  postgres:16-alpine
```

## Apply Schema

Upload hoặc stream migration:

```txt
backend/services/tenant-service/src/TenantService.Infrastructure/Migrations/0001_create_tenant_mvp.sql
```

Apply bằng `psql` trong container:

```bash
docker exec -i clinic-saas-postgres psql -U clinic_dev -d clinic_saas_dev < 0001_create_tenant_mvp.sql
```

## Verify Schema

```bash
docker exec clinic-saas-postgres pg_isready -U clinic_dev -d clinic_saas_dev
docker exec clinic-saas-postgres psql -U clinic_dev -d clinic_saas_dev -c "\dt platform.*"
```

Các bảng bắt buộc:

```txt
platform.tenants
platform.tenant_profiles
platform.tenant_domains
platform.tenant_modules
```

## Verify Port

```bash
docker port clinic-saas-postgres
ss -tulpen
```

Kỳ vọng:

```txt
docker port clinic-saas-postgres không trả public 0.0.0.0:5432.
PostgreSQL không listen public trên host.
```

## Bước Tiếp Theo

Sau khi PostgreSQL smoke container sẵn sàng, chạy `tenant-service` và `api-gateway` trong cùng Docker network hoặc dùng SSH tunnel tùy hướng deploy tiếp theo owner duyệt.

## Kết Quả Bootstrap 2026-05-09

Server:

```txt
OS: Ubuntu 22.04.5 LTS
CPU: 4 cores
RAM: 5.8 GiB
Disk root: 64G total, 48G available tại thời điểm verify
```

Docker:

```txt
Docker Engine: 29.4.3
Docker Compose plugin: v5.1.3
docker info: pass
CgroupDriver: systemd
StorageDriver: overlayfs
```

PostgreSQL smoke:

```txt
container: clinic-saas-postgres
image: postgres:16-alpine
status: Up
network: clinic-saas-network
volume: clinic-saas-postgres-data
port publish: none
pg_isready: accepting connections
```

Schema:

```txt
platform.tenant_domains
platform.tenant_modules
platform.tenant_profiles
platform.tenants
```

Port exposure:

```txt
docker port clinic-saas-postgres: empty
PostgreSQL 5432 chỉ là container internal port, không publish ra host.
UFW không mở 5432/tcp.
```

Host ports đang listen/mở theo UFW tại thời điểm verify:

```txt
22/tcp
80/tcp
443/tcp
5678/tcp
20128/tcp
127.0.0.1:18789
127.0.0.1:5679
```
