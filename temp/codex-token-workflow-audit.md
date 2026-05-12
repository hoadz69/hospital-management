# Codex Token / Workflow Audit - Clinic SaaS

Ngày ghi: 2026-05-12

Lane: Docs/Workflow
Action: review-only audit, no file changes

## Findings

- Top nguyên nhân tốn token: `temp/plan.frontend.md`, `docs/current-task.frontend.md`, `temp/plan.backend.md`, `docs/current-task.backend.md` đang là append-only history, mỗi lượt đọc lane có thể kéo hàng nghìn dòng cũ.
- Rule workflow bị lặp giữa `AGENTS.md`, `docs/agent-playbook.md`, `docs/agents/lead-agent.md`, `.claude/agents/lead-agent.md`, `docs/codex-setup.md`.
- `AGENTS.md` hiện quá nhiều nội dung chi tiết đáng lẽ chỉ link sang playbook/agent docs; agent đọc full mỗi session làm input token tăng nền.
- `clinic_saas_report.md` có thêm phần agent plan ở cuối, không nên đọc cho task nhỏ nếu chỉ cần architecture/product summary.
- `git diff --check`: PASS, chỉ có LF/CRLF warnings. Không build/test. Không sửa file trong lượt audit.

## File Size Table

| File | Lines | Size | Current role | Recommendation |
|---|---:|---:|---|---|
| `temp/plan.frontend.md` | 3632 | 146 KB | FE plan + lịch sử wave | Archive history, giữ active plan 100-200 dòng |
| `docs/current-task.frontend.md` | 1664 | 85.1 KB | FE handoff + history | Giữ dashboard hiện tại, move old wave log sang archive |
| `temp/plan.backend.md` | 1727 | 69.3 KB | BE/DevOps plan + history | Tách active plan và history |
| `docs/current-task.backend.md` | 954 | 41.7 KB | BE handoff + smoke history | Giữ trạng thái hiện tại + blockers, archive log cũ |
| `AGENTS.md` | 436 | 35.3 KB | Global mandatory rules | Rút xuống bootstrap policy, link chi tiết |
| `docs/agent-playbook.md` | 606 | 32.1 KB | Full workflow | Giữ canonical, nhưng không đọc full cho task nhỏ |
| `docs/agents/lead-agent.md` | 340 | 19.3 KB | Lead role | Rút bớt phần đã có ở playbook/AGENTS |
| `clinic_saas_report.md` | 536 | 17.4 KB | Product/architecture SOT | Chỉ đọc khi đổi hướng architecture/product |
| `rules/backend-coding-rules.md` | 317 | 14.6 KB | Backend rules | Chỉ đọc backend code task |
| `docs/agents/figma-ui-agent.md` | 423 | 13.2 KB | Figma UI workflow | Chỉ đọc Figma/visual task |
| `.claude/agents/lead-agent.md` | 164 | 12.2 KB | Claude mirror | Không cần Codex đọc thường xuyên |
| `rules/database-rules.md` | 258 | 10.2 KB | DB rules | Chỉ đọc DB/schema/data access task |
| `agents/figma-ui-agent.md` | 209 | 9.6 KB | Root quick agent doc | Có thể link về `docs/agents/...` |
| `docs/codex-setup.md` | 169 | 9.1 KB | Setup/reference | Không đọc mỗi task |
| `temp/plan.devops.md` | 183 | 8.5 KB | DevOps plan | Chỉ đọc DevOps lane |

## Duplication Table

| Rule/Content | Duplicated in | Recommendation |
|---|---|---|
| Short Lead Prompt / action trigger | `AGENTS.md`, `docs/agent-playbook.md`, `docs/agents/lead-agent.md`, `.claude/agents/lead-agent.md`, `docs/codex-setup.md` | Giữ canonical ở `docs/agent-playbook.md`; `AGENTS.md` chỉ tóm tắt 5-8 dòng |
| Fast/Budget Mode + token budget | 5 file trên | Giữ ở `AGENTS.md` dạng mandatory ngắn; chi tiết ở playbook |
| Không commit/stage/push | Rất nhiều agent/rule/plan/current-task | Giữ global ở `AGENTS.md`; agent docs chỉ nhắc 1 dòng |
| Server Test Runtime Rule | `AGENTS.md`, `docs/agent-playbook.md`, `docs/session-continuity.md`, `docs/codex-setup.md`, lane docs | Giữ global ngắn + backend lane link chi tiết |
| Figma chỉ khi cần | `AGENTS.md`, playbook, lead-agent, figma-agent, frontend-agent | Giữ trong `AGENTS.md` + `docs/agents/figma-ui-agent.md`; các nơi khác link |
| Current-task/plan lane policy | `AGENTS.md`, playbook, lead-agent, session-continuity | Giữ canonical trong `docs/session-continuity.md`; AGENTS chỉ rule ngắn |

## Recommended Cleanup Plan

### 1. Quick wins an toàn

- Archive history cũ của `temp/plan.frontend.md`, `docs/current-task.frontend.md`, `temp/plan.backend.md`, `docs/current-task.backend.md`.
- Giữ mỗi lane file phần “Current state / Next / Verify / Blocker / Active plan” ngắn.
- Rút `AGENTS.md` còn bootstrap guardrails + reading policy + link canonical.
- Thêm “Reading Policy” rõ: task nhỏ không đọc full playbook/report/Figma.

### 2. Medium changes cần cẩn thận

- Deduplicate `docs/agent-playbook.md` và `docs/agents/lead-agent.md`; chọn playbook làm canonical cho Step 0-10.
- Root `agents/*.md` và `.claude/agents/*.md` chuyển thành thin wrappers/link summaries.
- Tách `clinic_saas_report.md` phần agent plan ra doc agent/playbook hoặc bỏ nếu outdated.

### 3. Không nên sửa

- Không xóa guardrail tenant/security/secret/commit.
- Không bỏ lane current-task/plan hoàn toàn; chỉ giảm lịch sử.
- Không archive verify evidence mới nhất nếu đang là blocker active.

## Proposed New Reading Policy

- Task nhỏ frontend: đọc `AGENTS.md` bản rút gọn, `docs/current-task.md`, `docs/current-task.frontend.md`, `temp/plan.frontend.md`, `rules/coding-rules.md`; chỉ đọc `docs/agents/frontend-agent.md` khi cần role checklist.
- Task nhỏ backend: đọc `AGENTS.md`, dashboard, `docs/current-task.backend.md`, `temp/plan.backend.md`, `rules/coding-rules.md`, `rules/backend-coding-rules.md`; thêm DB/testing rules theo scope.
- Task docs/workflow: đọc `AGENTS.md`, `docs/agent-playbook.md`, `docs/session-continuity.md`, file docs liên quan.
- Chỉ đọc full `AGENTS.md`/`agent-playbook`/`clinic_saas_report` khi đổi workflow, feature team lớn, architecture/product direction, hoặc session onboarding.
- Chỉ đọc Figma/web/browser khi owner yêu cầu visual/Figma/research, task restyle lớn, hoặc chuẩn bị commit UI lớn.

## Next Prompt

```txt
Lead Agent: implement cleanup docs/token budget theo audit vừa rồi. Chỉ sửa docs/config workflow, không sửa backend/frontend/infrastructure code, không build/test.

Scope:
- Rút gọn AGENTS.md thành bootstrap guardrails + reading policy + link canonical.
- Archive/rút gọn history trong docs/current-task.frontend.md, docs/current-task.backend.md, temp/plan.frontend.md, temp/plan.backend.md; giữ active state, blockers, next steps, verify commands.
- Deduplicate rule lặp giữa AGENTS.md, docs/agent-playbook.md, docs/agents/lead-agent.md, docs/codex-setup.md bằng link về canonical.
- Không xóa guardrail tenant/security/secret/commit/stage/push.
- Không commit/stage/push.
- Sau khi sửa chạy git diff --check và report files changed + dirty.
```
