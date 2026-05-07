# CLAUDE.md - Clinic SaaS / Hospital Management

Claude Code must follow the same project rules as Codex. The repo was copied from another project, so do not trust old `ez-sales-bot` instructions, connection strings, service names, or task plans.

## Read First

1. `AGENTS.md`
2. `clinic_saas_report_vi.md`
3. `docs/current-task.md`
4. Relevant files under `rules/` only when writing real code

## Before Coding

- For code implementation, create or update `temp/plan.md` and wait for owner approval unless the owner explicitly says to proceed.
- For documentation/config cleanup requested directly by the owner, keep changes scoped and report exactly what changed.
- Do not run build/test/start commands unless needed for the task or explicitly requested.
- Do not commit unless explicitly requested.
- Do not use old production database, SSH, Telegram, or `ez-sales-bot` connection details.

## Project Direction

The target product is a multi-tenant Clinic SaaS platform:

- Public Website for patients.
- Clinic Admin Portal for each clinic tenant.
- Owner Super Admin Portal for platform owner operations.
- API Gateway/BFF in front of microservices.
- Clean Architecture inside each backend service.
- Tenant isolation is mandatory in every tenant-owned query and command.

## Target Stack

- Frontend: Vue 3, Vite, TypeScript, Pinia, Vue Router, shared UI components, Figma design tokens.
- Backend: .NET service architecture, Clean Architecture, CQRS/use-case pattern where useful.
- Data: PostgreSQL, MongoDB, Redis.
- Async/realtime: Kafka/Event Bus and future SignalR/WebSocket gateway.

The `.NET 11` target in reports is a project direction, not a verified stable runtime decision. Before implementation, verify the current stable/LTS .NET version and document the final choice.

## Figma

Figma/FigJam are source of truth:

- Architecture Source of Truth:
  `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth`
- Technical Architecture:
  `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture`

Use Figma MCP when available. If MCP tools are not visible, restart Claude Code/Codex after MCP config changes.

## Handoff Rule

If work stops mid-task, update `docs/current-task.md` with:

- status,
- completed work,
- blocked items,
- exact next commands/actions,
- files changed.

This is mandatory so Codex and Claude Code can continue each other's work without losing context.
