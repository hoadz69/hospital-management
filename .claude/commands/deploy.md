---
name: deploy
description: Chuẩn bị deployment Clinic SaaS sau khi owner cung cấp server details.
user_invocable: true
allowed-tools: Bash, Read, Glob
---

Không deploy nếu owner chưa cung cấp server, registry, domain, SSH và database details hiện tại.

Trước mọi deploy:

1. Confirm target environment: dev, staging, or production.
2. Confirm services to deploy.
3. Confirm image registry and tag.
4. Confirm server path, SSH method, network, domain, SSL flow.
5. Check git status and ask before commit/push.
6. Use Clinic SaaS service names only.
7. Không commit/push nếu owner chưa yêu cầu.

Expected future services:

- api-gateway
- owner-admin
- clinic-admin
- public-web
- identity-service
- tenant-service
- website-cms-service
- template-service
- domain-service
- booking-service
- catalog-service
- customer-service
- billing-service
- report-service
- notification-service
- realtime-gateway

Yêu cầu:

```txt
$ARGUMENTS
```

Nếu thiếu server information, tạo deployment checklist thay vì chạy lệnh.
