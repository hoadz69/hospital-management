# AGENTS.md - Clinic SaaS / Hospital Management

File này là hướng dẫn chung cho Codex, Claude Code và các coding agent khác khi làm việc trong repo `hospital-management`.

## Source Of Truth

- Product/architecture report: `clinic_saas_report_vi.md`
- Technical architecture notes: `architech.txt`
- Current handoff task: `docs/current-task.md`
- UI source of truth: Figma Design/board do owner cung cấp
- System architecture source of truth:
  - `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth`
  - `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture`

## Current Project Identity

- Project name: Clinic SaaS / Hospital Management Platform
- Old copied project names such as `ez-sales-bot`, `EzSalesBot`, merchant chatbot, Telegram bot, knowledge upload, and old production database references are not valid source of truth.
- Backend/frontend folders are currently a scaffold shell. Treat old docs or compose files as migration targets unless they have already been rewritten for Clinic SaaS.
- Server/database credentials are not finalized. Do not invent connection details. Use placeholders until the owner provides real values.

## Required Workflow

1. Read this file before doing work.
2. Read `clinic_saas_report_vi.md` and `docs/current-task.md` before changing project direction.
3. For implementation work, create or update a plan in `temp/plan.md` first and wait for owner approval unless the owner explicitly asks you to make documentation/config cleanup immediately.
4. Keep changes scoped. Do not reorganize the repo broadly unless the approved plan says so.
5. Do not commit unless the owner explicitly asks.
6. Do not delete files unless the owner explicitly asks or the file is newly created by your own current change.
7. If you cannot finish, update `docs/current-task.md` with:
   - what was completed,
   - what is blocked,
   - exact next steps,
   - files already changed.

## Architecture Rules

- Multi-tenant is mandatory. Tenant-owned data must always have tenant context.
- Owner Super Admin can operate cross-tenant. Clinic Admin can only operate inside one tenant.
- Frontend target: Vue 3, Vite, TypeScript, shared UI package, design tokens from Figma.
- Backend target: .NET service architecture with Clean Architecture per service.
- Data strategy:
  - PostgreSQL for relational/transactional data.
  - MongoDB for CMS/page/template JSON.
  - Redis for cache, tenant/domain mapping, rate limit, temporary locks.
  - Kafka/Event Bus for async platform events when needed.
  - SignalR/WebSocket support is planned for realtime features.

## Agent Coordination

Codex and Claude Code should both follow the same role split:

- Architect Agent: validates service boundary, data ownership, tenant isolation, and FigJam alignment.
- Figma UI Agent: converts Figma UI into Vue components and shared design tokens.
- Frontend Agent: builds `public-web`, `clinic-admin`, `owner-admin`, routing, API client, tenant context.
- Backend Agent: builds .NET services using Clean Architecture.
- Database Agent: designs PostgreSQL schemas, MongoDB collections, indexes, migrations, seed data.
- DevOps Agent: prepares Docker Compose, env structure, CI/CD, deployment, domain/SSL flow.
- QA Agent: verifies tenant isolation, auth permissions, booking, template apply, domain verification.
- Documentation Agent: keeps README, architecture docs, setup, deployment, and troubleshooting current.

When a tool supports subagents, use them only when the user explicitly asks for delegation/parallel agents. Otherwise, keep the role split as a planning framework.

## MCP Notes

- Codex global Figma MCP is configured with remote URL `https://mcp.figma.com/mcp`.
- `.mcp.json` in this repo is for Claude-style project MCP config.
- Real database/SSH/server MCP entries must not be filled until the owner provides current server details.
- If Figma MCP tools are not visible in a session, restart Codex/Claude Code so the MCP config is reloaded.

## GitNexus

This repo was copied from a project that referenced GitNexus index `ez-sales-bot`. That index is not trusted for this Clinic SaaS shell until re-analyzed for the new project.

If GitNexus tools are available in the active agent session:

- Run `npx gitnexus analyze` after the repo structure is corrected.
- Run impact analysis before editing real code symbols.
- Run change detection before committing.

If GitNexus tools are not available, state that limitation clearly and continue with file-level review for documentation/config tasks.
