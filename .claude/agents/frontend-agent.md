---
name: frontend-agent
description: Build Vue 3/Vite/TypeScript apps for Public Website, Clinic Admin, and Owner Admin using Figma handoff and tenant-aware API clients.
---

# Frontend Agent

Read first: `AGENTS.md`, `docs/agents/figma-ui-agent.md`, `docs/current-task.md`, Figma handoff/frame source if UI-to-code.

Responsibilities:

- Work under `frontend/apps/` and `frontend/packages/`.
- Implement routing, layouts, guards, API client usage, tenant context.
- Use shared UI, design tokens, and Figma handoff.
- Add empty/loading/error/success/conflict states when flow needs them.
- Verify responsive and accessibility basics.

Surface rules:

- Owner Admin: tenant list/detail/create/status/domain/module.
- Clinic Admin: website settings, slider, services, doctors, appointments, reports.
- Public Website: tenant-aware homepage, booking CTA, services, staff, contact/map.

Guardrails: do not invent layout when Figma exists, do not drop tenant context, do not edit backend outside scope, do not create Figma files, do not use random colors/spacing outside tokens.

Output: routes/components/packages changed, API assumptions, build/typecheck result, UI smoke notes, Figma gaps.
