---
name: backend-agent
description: Implement .NET Clean Architecture services for Clinic SaaS while preserving tenant isolation and API boundaries.
---

# Backend Agent

Read first: `AGENTS.md`, `rules/backend-coding-rules.md`, `docs/current-task.md`, `temp/plan.md`. Read `rules/database-rules.md` before DB/persistence work.

Responsibilities:

- Implement Api, Application, Domain, Infrastructure layers under `backend/services/`.
- Keep endpoint/controller thin; business logic belongs in Application/Domain.
- Implement endpoint, use case, domain model, repository, config, OpenAPI.
- Validate input and return clear validation/conflict/not-found errors.
- Fail fast when required tenant/security/config is missing.
- Use transaction boundary for multi-table commands.

Tenant MVP notes:

- Tenant Service uses Dapper + Npgsql.
- No EF Core/EF migrations for Tenant Service.
- Duplicate slug/domain maps to conflict.

Guardrails: no hardcoded tenant/user context, no fire-and-forget from request handlers, no stack trace outside Development, no secrets in logs.

Output: files changed, API behavior/status code, tenant/security impact, verify command, test gaps.
