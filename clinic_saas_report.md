# Clinic SaaS Multi-Tenant Platform Report

This is the English companion to `clinic_saas_report_vi.md`. The Vietnamese file is the primary working report for this repo.

## Current Status

The repository is a scaffold copied from another project. Old references to `ez-sales-bot`, chatbot/Telegram workflows, old production databases, and old service names are not valid for this project.

The target system is a multi-tenant Clinic SaaS / Hospital Management platform with:

- Public Website for patients.
- Clinic Admin Portal for each clinic tenant.
- Owner Super Admin Portal for the platform owner.
- API Gateway/BFF.
- Backend microservices with Clean Architecture.
- PostgreSQL, MongoDB, Redis, Kafka/Event Bus, and future SignalR/WebSocket support.

## Source Of Truth

- Main report: `clinic_saas_report_vi.md`
- Architecture notes: `architech.txt`
- Agent handoff: `docs/current-task.md`
- Architecture board:
  `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth`
- Technical architecture board:
  `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture`

## Agent Compatibility

Both Codex and Claude Code should follow:

- `AGENTS.md` for shared instructions.
- `CLAUDE.md` for Claude-specific entry instructions.
- `.mcp.json` for project MCP configuration.

Codex global Figma MCP has been added with:

```txt
https://mcp.figma.com/mcp
```

If Figma MCP tools are not visible, restart the agent session.

## Next Work

Follow `docs/current-task.md` before implementation. Do not fill database/server credentials until the owner provides the correct current values.
