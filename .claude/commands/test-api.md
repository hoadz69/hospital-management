---
name: test-api
description: Test Clinic SaaS API flows after services exist.
effort: high
user_invocable: true
allowed-tools: Read, Bash, Glob, Grep
---

The repo currently has no real Clinic SaaS backend services. Do not use old copied JWT tokens, ports, or `EzSalesBot` paths.

When services exist, test:

1. Health endpoint for API Gateway/BFF.
2. Tenant resolution by domain/subdomain/header/JWT.
3. Auth and role permissions.
4. Owner Super Admin cross-tenant flow.
5. Clinic Admin tenant-scoped flow.
6. Booking flow.
7. Template apply mode.
8. Domain verification flow.

Request:

```txt
$ARGUMENTS
```

If services do not exist yet, report that API testing is not applicable and update `docs/current-task.md` if needed.
