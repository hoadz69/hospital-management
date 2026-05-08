---
name: db-migrate
description: Chuẩn bị hoặc review migration database cho Clinic SaaS.
effort: high
user_invocable: true
allowed-tools: Read, Bash, Grep
---

Chỉ dùng command này sau khi owner cung cấp connection hiện tại hoặc local compose environment đã được duyệt.

Rules:

- Never use unverified database names or credentials.
- Never run destructive SQL without explicit owner approval.
- Prefer migration SQL files committed to the relevant service folder.
- Every tenant-owned table must include `tenant_id` and appropriate indexes.
- PostgreSQL is for relational/transactional data.
- MongoDB is for CMS/page/template JSON.
- SQL naming mặc định lowercase `snake_case`.

Yêu cầu:

```txt
$ARGUMENTS
```

Nếu chưa có database connection, chỉ tạo migration plan, không execute SQL.
