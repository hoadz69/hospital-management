# Current Task - Clinic SaaS Repo Cleanup

Last updated: 2026-05-07

## Goal

Normalize the copied repo so both Codex and Claude Code understand this is now a Clinic SaaS / Hospital Management platform, not the old `ez-sales-bot` project.

## Completed

- Added Figma MCP to Codex global config:
  - `figma`
  - `https://mcp.figma.com/mcp`
  - OAuth login completed.
- Rewrote `AGENTS.md` for shared Codex/Claude instructions.
- Rewrote `CLAUDE.md` for Claude Code entry instructions.
- Replaced project `.mcp.json` with Figma-only MCP config.
- Rewrote `clinic_saas_report_vi.md` as the main Vietnamese source-of-truth report.
- Rewrote `clinic_saas_report.md` as the English companion report.
- Rewrote `architech.txt` as a clean architecture index.

## Blocked / Not Done

- The current Codex session does not expose Figma MCP tools directly, so the two FigJam boards were not extracted into structured text yet.
- Browser fetch for the Figma board URLs failed in this session.
- Real server/database credentials are intentionally not configured. Owner said they will provide server information later.
- `src/docker-compose.yml`, `docker-compose.prod.yml`, `plan.md`, `todo-refactor.md`, and some files under `.claude/commands` still likely contain old copied project assumptions and should be cleaned in the next pass.

## Next Recommended Steps

1. Restart Codex/Claude Code so Figma MCP is loaded.
2. Use Figma MCP to extract the two architecture boards:
   - `Cw0evT4maoKnQX5G23tJpT`
   - `Fwpls2wzNxzGdpDuGGYSxi`
3. Update `clinic_saas_report_vi.md` with exact board contents.
4. Clean old compose and task files:
   - `src/docker-compose.yml`
   - `docker-compose.prod.yml`
   - `plan.md`
   - `todo-refactor.md`
   - `.claude/commands/*.md`
5. When the owner provides server info, add proper MCP/database/env documentation without committing secrets.

## Files Changed In This Pass

- `AGENTS.md`
- `CLAUDE.md`
- `.mcp.json`
- `clinic_saas_report_vi.md`
- `clinic_saas_report.md`
- `architech.txt`
- `docs/current-task.md`
