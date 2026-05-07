---
name: db-migrate
description: Prepare or review Clinic SaaS database migration work.
effort: high
user_invocable: true
allowed-tools: Read, Bash, Grep
---

Use this command only after the owner provides current database connection details or a local compose environment is approved.

Rules:

- Never use copied `ez-sales-bot` database names or credentials.
- Never run destructive SQL without explicit owner approval.
- Prefer migration SQL files committed to the relevant service folder.
- Every tenant-owned table must include `tenant_id` and appropriate indexes.
- PostgreSQL is for relational/transactional data.
- MongoDB is for CMS/page/template JSON.

Request:

```txt
$ARGUMENTS
```

If no database connection is configured, produce a migration plan instead of executing SQL.
