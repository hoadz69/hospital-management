---
name: deploy
description: Prepare Clinic SaaS deployment steps after server details are provided.
user_invocable: true
allowed-tools: Bash, Read, Glob
---

Do not deploy yet. The owner has not provided current server, registry, domain, SSH, or database details.

Before any deploy:

1. Confirm target environment: dev, staging, or production.
2. Confirm services to deploy.
3. Confirm image registry and tag.
4. Confirm server path, SSH method, network, domain, SSL flow.
5. Check git status and ask before commit/push.
6. Use Clinic SaaS service names only.

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

Request:

```txt
$ARGUMENTS
```

If server information is missing, create a deployment checklist instead of running commands.
