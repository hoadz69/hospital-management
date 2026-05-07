# Coding Rules - Clinic SaaS

These rules apply when writing real code for the Clinic SaaS / Hospital Management platform.

## General

- Read `AGENTS.md`, `CLAUDE.md`, `clinic_saas_report_vi.md`, and `docs/current-task.md` first.
- Work from an approved `temp/plan.md` for implementation tasks.
- Keep changes scoped to the approved feature.
- Do not add real secrets, server IPs, database passwords, or tokens to the repo.
- Do not use copied project names or paths from the old codebase.

## Architecture

- Use Clean Architecture per backend service:
  - Api
  - Application
  - Domain
  - Infrastructure
- Keep business logic out of controllers.
- Keep infrastructure dependencies out of Domain.
- Application layer owns use cases, commands, queries, validation, and transaction boundaries.
- Infrastructure layer owns persistence, cache, event bus, external providers.

## Multi-Tenant Rules

- Tenant-owned data access must require tenant context.
- Tenant-owned PostgreSQL tables must include `tenant_id` and indexes.
- Tenant-owned MongoDB documents must include `tenant_id` and indexes.
- Clinic Admin cannot access another tenant.
- Owner Super Admin is the only cross-tenant role.
- Public website tenant should resolve from domain/subdomain.

## Frontend

- Use Vue 3 + Vite + TypeScript.
- Use shared UI components and design tokens.
- Keep UI aligned with Figma.
- Do not invent layouts when Figma source exists.
- Keep tenant context in the API client/request layer.

## Backend

- Use async APIs for I/O.
- Validate external input before domain/application operations.
- Prefer explicit use cases over large generic services.
- Use structured errors and logging.
- Do not catch and swallow exceptions that should rollback a transaction.
- Do not fallback to hardcoded tenant/user/security context.

## Database

- PostgreSQL: relational and transactional data.
- MongoDB: CMS, page JSON, template config, layout data.
- Redis: cache, tenant config, domain mapping, rate limit, temporary locks.
- Kafka/Event Bus: async domain events.

## Verification

- Verify with focused manual/API checks until test structure exists.
- For tenant features, verify both allowed and forbidden tenant access.
- For UI features, verify desktop and mobile layout.
- Update `docs/current-task.md` if work stops before completion.
