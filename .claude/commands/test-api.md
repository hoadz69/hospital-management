---
name: test-api
description: Test API flows của Clinic SaaS sau khi services tồn tại.
effort: high
user_invocable: true
allowed-tools: Read, Bash, Glob, Grep
---

Nếu repo chưa có backend services thật, không tự chế token/port/path. Hãy báo API testing chưa áp dụng.

Khi services tồn tại, test:

1. Health endpoint for API Gateway/BFF.
2. Tenant resolution by domain/subdomain/header/JWT.
3. Auth and role permissions.
4. Owner Super Admin cross-tenant flow.
5. Clinic Admin tenant-scoped flow.
6. Booking flow.
7. Template apply mode.
8. Domain verification flow.
9. Security context missing/invalid.
10. Forbidden cross-tenant access.

Yêu cầu:

```txt
$ARGUMENTS
```

Nếu services chưa tồn tại, report rằng API testing chưa áp dụng và cập nhật `docs/current-task.md` nếu cần.
