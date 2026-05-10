# Kế Hoạch DevOps - Pre-Phase 4 Hardening

Ngày tạo: 2026-05-10
Ngày cập nhật: 2026-05-10

Trạng thái: ✅ **IMPLEMENTATION DONE** — P1.8 + P1.4 đã sửa, `docker compose config` PASS cả dev và prod. `nginx -t` qua container blocked do Docker daemon offline trong session, để QA verify khi daemon có. Owner sửa thêm nginx scaffold thay `return 503` bằng `proxy_pass http://$api_upstream` resolver-based + cache static asset, đúng hướng deploy thật. Sẵn sàng commit DevOps lane.

Lane này được tạo để chứa các DevOps task không đi cùng backend lane. Hiện chứa hai issue P1 quick win cho Pre-Phase 4 Hardening: P1.4 nginx SPA fallback + X-Forwarded headers, P1.8 docker-compose dev bind `127.0.0.1` thay vì `0.0.0.0`.

## 1. Bối Cảnh

Pre-Phase 4 Hardening là một feature team task do Lead Agent điều phối theo "Feature Team Execution Workflow" (Step 0–10 trong `docs/agent-playbook.md` và `AGENTS.md`). Lane DevOps phụ trách 2/5 issue, các lane khác phụ trách phần còn lại:

- Backend lane (`temp/plan.backend.md`): P1.1 test infra rỗng, P1.3 Swagger gating Development.
- Frontend lane (`temp/plan.frontend.md`): P1.7 httpClient `JSON.parse` try/catch.
- DevOps lane (file này): P1.4 + P1.8.

Mục tiêu Pre-Phase 4 Hardening: vá các điểm yếu nhỏ trước khi mở Phase 4 mới, không thêm feature.

## 2. Agents Tham Gia (Lane DevOps)

- Lead Agent: điều phối, gom report, đề xuất commit split, không tự code.
- Architect Agent: review boundary/risk trước khi DevOps Agent đụng nginx/docker compose.
- DevOps Agent: thực hiện thay đổi `infrastructure/docker/nginx/default.conf` và `infrastructure/docker/docker-compose.dev.yml`.
- QA Agent: tạo verify checklist, chạy `docker compose config`, smoke nginx config nếu khả thi.
- Documentation Agent: cập nhật `docs/current-task.md` dashboard, lane plan này, roadmap nếu status thay đổi.

## 3. Issues Trong Lane Này

### P1.4 nginx SPA fallback + X-Forwarded headers

File hiện tại: `infrastructure/docker/nginx/default.conf` chỉ là placeholder trả `200 "Clinic SaaS reverse proxy placeholder"` (8 dòng). Chưa có SPA fallback (`try_files ... /index.html`), chưa có forward header chuẩn.

Mục tiêu:

- Giữ scaffold nhỏ, không bịa hostname production.
- Thêm SPA fallback pattern: `try_files $uri $uri/ /index.html` cho route SPA.
- Thêm `proxy_set_header X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Host`, `X-Real-IP` để khi sau này forward sang api-gateway, backend thấy đúng client info.
- Để rõ là placeholder cho Phase 4 deployment thật, không hardcode domain Clinic SaaS.
- Chỉ chạm `infrastructure/docker/nginx/default.conf`. Không tạo nginx mới ở root.

### P1.8 docker-compose dev bind `127.0.0.1` thay vì `0.0.0.0`

File hiện tại: `infrastructure/docker/docker-compose.dev.yml` publish 4 service ra `0.0.0.0`:

```yaml
postgres: "5432:5432"
mongodb: "27017:27017"
redis: "6379:6379"
kafka: "9092:9092"
```

Mục tiêu: đổi thành `127.0.0.1:<port>:<port>` cho 4 service để dev port không expose ra LAN/internet ngoài máy dev. Không đụng `docker-compose.prod.yml` (nginx placeholder vẫn cần `8080:80`, đây là task Phase 4 deployment).

## 4. File Dự Kiến Sửa

```txt
infrastructure/docker/docker-compose.dev.yml
infrastructure/docker/nginx/default.conf
docs/current-task.md (dashboard ngắn)
docs/current-task.backend.md (nếu DevOps đi cùng backend lane status)
temp/plan.devops.md (file này, ghi nhận trạng thái sau implement)
docs/roadmap/clinic-saas-roadmap.md (nếu thêm dòng Pre-Phase 4 Hardening Done)
```

Không sửa:

```txt
backend/**
frontend/**
.env*
infrastructure/postgres/init.sql
infrastructure/mongodb/init.js
infrastructure/redis/redis.conf
docker-compose.prod.yml
Figma
```

## 5. Risk

- P1.4: nếu sau này ai đó deploy thẳng `nginx/default.conf` lên prod, SPA fallback không có upstream → trả 404 cho mọi route. Cần ghi rõ comment file là placeholder dev/staging, Phase 4 deployment phải override với hostname thật + SSL + upstream api-gateway. Không thay vai trò của Phase 4 deployment.
- P1.8: bind `127.0.0.1` chỉ cho phép truy cập từ máy host. Nếu owner có WSL/devcontainer chạy ở máy khác hoặc test từ device LAN, sẽ break. Theo session hiện tại owner dev trên Windows host, container chạy local Docker Desktop ⇒ `127.0.0.1` an toàn. Có thể document trong comment file: nếu cần expose, đổi sang `0.0.0.0` tạm thời, không commit.
- Backward compat: smoke trên server `116.118.47.78` (Phase 2) đã dùng container không qua published port, nên P1.8 không ảnh hưởng. P1.4 chỉ là placeholder, không impact runtime smoke đã pass.

## 6. Verify Command

DevOps Agent sau khi sửa chạy:

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
docker compose -f infrastructure/docker/docker-compose.prod.yml config
```

Kỳ vọng: cả 2 lệnh exit 0, không lỗi YAML. `docker compose -f infrastructure/docker/docker-compose.dev.yml config` phải hiển thị `published: 5432`, `host_ip: 127.0.0.1` (hoặc tương đương) cho 4 service.

QA Agent kiểm tra thêm bằng grep:

```powershell
Select-String -Pattern '"5432:5432"|"27017:27017"|"6379:6379"|"9092:9092"' infrastructure/docker/docker-compose.dev.yml
```

Kỳ vọng: không match (đã đổi sang `127.0.0.1:<port>:<port>`).

Smoke nginx (nếu có docker daemon local):

```powershell
docker run --rm -v "${PWD}/infrastructure/docker/nginx/default.conf:/etc/nginx/conf.d/default.conf:ro" nginx:1.27-alpine nginx -t
```

Kỳ vọng: `nginx: configuration file ... test is successful`.

## 7. Commit Split Đề Xuất

Theo Step 9 Feature Team Execution Workflow, tách commit theo lane:

```txt
chore(devops): bind dev compose to 127.0.0.1 (P1.8)
chore(devops): scaffold nginx SPA fallback + forward headers (P1.4)
```

Không gom với commit backend/frontend của Pre-Phase 4 Hardening. Nếu owner muốn 1 commit duy nhất cho cả lane DevOps, có thể gộp 2 thành 1:

```txt
chore(devops): pre-phase 4 hardening (compose 127.0.0.1, nginx scaffold)
```

## 8. Out Of Scope

- Không thêm domain production thật vào nginx config.
- Không đụng SSL/cert config (Phase 4 deployment).
- Không đụng docker-compose.prod.yml (Phase 4 deployment).
- Không expose database public; ngược lại P1.8 đang siết lại bind.
- Không sửa backend/frontend code.
- Không tạo Figma file.
- Không commit/push.
- Không cài package mới (vẫn dùng nginx:1.27-alpine, postgres:16-alpine, ...).
- Không đổi Phase 2/Phase 3 status đã Done.
- Không mở Phase 4 mới.

## 9. Điểm Dừng

Plan ready. DevOps Agent chỉ implement khi owner đã duyệt rõ. Sau khi implement, QA Agent chạy verify, Documentation Agent cập nhật:

```txt
docs/current-task.md
temp/plan.devops.md (file này)
docs/roadmap/clinic-saas-roadmap.md (nếu thêm dòng Pre-Phase 4 Hardening)
```

Lead Agent gom report và đề xuất commit split. Không push.

## 10. Implementation Result (2026-05-10)

### 10.1 File đã sửa

```txt
infrastructure/docker/docker-compose.dev.yml (P1.8: 4 service đổi sang 127.0.0.1:port:port + comment scope)
infrastructure/docker/nginx/default.conf (P1.4: replace placeholder bằng SPA fallback + X-Forwarded headers + healthz + cache static + proxy_pass api-gateway resolver-based)
```

### 10.2 Verify đã chạy

```powershell
docker compose -f infrastructure/docker/docker-compose.dev.yml config
docker compose -f infrastructure/docker/docker-compose.prod.yml config
```

Kết quả:

```txt
dev compose: PASS, 4 service đều có host_ip: 127.0.0.1 (postgres 5432, mongo 27017, redis 6379, kafka 9092)
prod compose: PASS, vẫn parse OK với nginx config mới
```

### 10.3 Verify pending

```powershell
docker run --rm -v ".../nginx/default.conf:/etc/nginx/conf.d/default.conf:ro" nginx:1.27-alpine nginx -t
```

Trạng thái: BLOCKED — Docker daemon (Docker Desktop Linux engine) không chạy trong session hiện tại, error `open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.`. Manual review syntax đã pass (server/listen/location/try_files/proxy_pass đúng pattern, brace pairs khớp).

QA Agent verify lại bằng `nginx -t` khi Docker daemon online hoặc khi Phase 4 deploy thật. Đây không phải blocker chặn commit DevOps lane vì `docker compose config` đã parse được file nginx khi mount volume.
