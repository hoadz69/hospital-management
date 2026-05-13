# AGENTS.md - Clinic SaaS / Hospital Management

File bootstrap bat buoc cho Codex/Claude/coding agents trong repo `hospital-management`. Doc file nay truoc khi lam viec, sau do chi doc file chi tiet theo lane/scope.

## Reading Policy Mac Dinh

- Task nho frontend: doc `docs/current-task.md`, `docs/current-task.frontend.md`, `temp/plan.frontend.md`, `rules/coding-rules.md`, va source lien quan. Chi doc `docs/agents/frontend-agent.md` khi can checklist role.
- Task nho backend: doc `docs/current-task.md`, `docs/current-task.backend.md`, `temp/plan.backend.md`, `rules/coding-rules.md`, `rules/backend-coding-rules.md`; them `rules/database-rules.md` hoac `rules/backend-testing-rules.md` neu scope can.
- Task docs/workflow/Lead/full-team/cross-lane: doc `docs/agent-playbook.md` va agent docs lien quan.
- `docs/archive/**` va `temp/archive/**` la cold storage. Khong doc trong task thuong ngay, khong dua vao default reading list. Chi doc khi owner yeu cau ro, active file tro toi section archive cu the, hoac can bang chung debug cu.
- `.claude/agents/**` khong doc trong Codex task thuong ngay; do la wrapper cho Claude.
- `docs/codex-setup.md` chi doc khi setup Codex/MCP/tooling.
- `clinic_saas_report.md` va `architech.txt` chi doc full khi doi huong product/architecture hoac task can source-of-truth architecture.
- Figma/web/browser chi dung khi owner yeu cau Figma/visual/research, task restyle lon, screenshot cho thay UI lech, hoac chuan bi commit UI lon.

## Source Of Truth Ngan

- Product/architecture: `clinic_saas_report.md`, `architech.txt`.
- Workflow canonical: `docs/agent-playbook.md`.
- Session/resume/checkpoint: `docs/session-continuity.md`.
- Dashboard tong: `docs/current-task.md`.
- Backend lane: `docs/current-task.backend.md`, `temp/plan.backend.md`.
- Frontend lane: `docs/current-task.frontend.md`, `temp/plan.frontend.md`.
- Agent details cho Codex: `docs/agents/*.md`.
- Rules theo scope: `rules/*.md`.

## GitNexus Auto-Use

- Codex tu quyet dinh dung GitNexus theo `docs/codex-setup.md`; owner khong can phan loai task lon/nho.
- Truoc khi code neu task co blast radius khong ro, cham API/route/store/shared package/backend shared/tenant-security/DB/refactor/bug chua ro nguon: chay `gitnexus status` va query/context/impact phu hop.
- Khi verify/review diff code runtime hoac truoc commit split: chay `gitnexus detect-changes --scope all` neu GitNexus san sang.
- Duoc bo qua voi docs nho, text/CSS cuc bo, file/dong da ro, hoac chi chay command verify.
- Neu GitNexus loi/unavailable: fallback `rg` + source lien quan va report ngan; khong dung blocker chung chung de dung viec.

## Guardrails Bat Buoc

- Khong commit, push, stage neu owner chua yeu cau ro.
- Khong revert/xoa thay doi cua owner. Neu worktree dirty, doc va lam viec quanh thay doi do.
- Khong xoa file neu owner chua yeu cau, tru file moi do chinh task hien tai tao ra.
- Khong dua secret, IP server that, connection string that, token, SSH key/private key vao repo/docs/log.
- Khong sua ngoai scope. Neu thay van de ngoai scope, ghi note/report thay vi tu sua.
- Khong tao Figma file moi neu owner chua yeu cau ro.
- Khong stage/commit screenshot/log/generated artifacts.
- Sau moi task phai report: lane, action, files changed/reviewed, verify, blocker/skipped, dirty, next.

## Approval / Execution Rule

- Neu owner chi hoi/review/phan tich: chi doc/phan tich/report, khong code.
- Neu owner yeu cau plan/lead-plan: tao/cap nhat plan lane phu hop, khong implement cho den khi owner duyet.
- Neu owner yeu cau implement/code/action ma chua co plan duyet: tao lead-plan truoc va dung, tru khi owner noi ro lam luon/da duyet.
- Neu owner noi ro lam luon/da duyet/bat dau implement/tiep tuc approved scope: thuc hien trong scope, van giu guardrails.
- Prompt ngan `Lead Agent: bat dau/lam tiep/verify/chia commit <task>` la action trigger that; khong acknowledge-only. Chi tiet canonical nam trong `docs/agent-playbook.md`.

## Lane Rule

- `docs/current-task.md` chi la dashboard tong ngan, khong chua chi tiet lane.
- Backend/DevOps chi cap nhat `docs/current-task.backend.md` va `temp/plan.backend.md`.
- Frontend chi cap nhat `docs/current-task.frontend.md` va `temp/plan.frontend.md`.
- Neu khong chac lane, ghi note ngan vao dashboard tong va cho Lead phan loai.

## Plan / Roadmap Data Rule

- `temp/plan.frontend.md` va `temp/plan.backend.md` la living active plan, khong phai archive log. File nay duoc phep dai vua phai neu can de agent moi resume viec dang lam.
- Active plan phai co toi thieu: active goal, progress checklist/table, next decision, implementation rules, likely files/areas, acceptance criteria, verify plan, blockers/caveats, archive index.
- Toi uu them: active plan phai co `Current Active Slice`, last stopping point va file da cham that; neu chua co slice duoc duyet thi khong tu code.
- Sau moi luot lam viec, update tai cho active plan; khong append full history, full logs, full smoke transcript, full diff.
- Khi phase/wave/task lon Done, danh dau phase tong trong `docs/roadmap/clinic-saas-roadmap.md`; trong active plan chi giu Done summary ngan va chuyen sang next active slice.
- `docs/current-task.*.md` la handoff/dashboard ngan: latest state, verify snapshot, blocker, next. Khong thay the active plan.
- `docs/current-task.md` chi la dashboard cross-lane ngan va link sang lane; khong nhung plan chi tiet hoac history lane vao day.
- `docs/archive/**` va `temp/archive/**` chi giu history lanh. Khong copy nguoc toan bo archive vao active plan; chi trich distilled facts can cho active work.
- `docs/session-continuity.md` chi giu rule resume/checkpoint; khong dung lam noi chua active task data.

## Token / Budget Rule

- Fast/Budget Mode la mac dinh cho task nho: doc toi thieu theo reading policy, khong goi full subagents/Figma/screenshot neu khong can.
- PASS chi ghi PASS. FAIL chi paste loi lien quan truc tiep.
- Khong paste full logs, full diff, full file lon, hoac tom tat lai toan bo `AGENTS.md`.
- Docs update chi ghi file + section, khong paste toan bo noi dung.
- Fast report mac dinh: `Lane`, `Action`, `Files changed`, `Verify`, `Skipped/blocker`, `Dirty`, `Next`.

## Architecture / Security Invariants

- Multi-tenant la bat buoc. Tenant-owned data phai co tenant context hop le.
- Clinic Admin chi thao tac trong tenant cua ho. Owner Super Admin moi duoc thao tac cross-tenant/platform scope.
- Public Website resolve tenant qua domain/subdomain.
- Thieu tenant context hoac invalid security context thi fail ro rang, khong fallback hardcoded.
- Command co nhieu DB operations can transaction boundary ro.
- Khong fire-and-forget task tu request handler neu chua co thiet ke queue/background service.

## Language Rule

- Tra loi owner, plan, report, handoff, roadmap update va agent docs bang tieng Viet.
- Code identifiers, commands, API path, log/error goc duoc giu tieng Anh.
- Comment/XML doc/SQL comment moi hoac doan dang sua uu tien tieng Viet; public XML doc phai co `summary`, `param`, `returns` du nghia khi ap dung.

## QA / Artifact Rule

- Visual QA/screenshot chi chay khi owner yeu cau visual QA, restyle/layout lon, co loi visual can chung minh, hoac truoc commit UI lon.
- Screenshot/log/generated artifacts la review artifacts; khong stage/commit tru khi owner yeu cau ro.
- Cleanup chi xoa untracked/generated artifacts va phai kiem tra status truoc/sau khi task yeu cau cleanup.

## Project Structure

```txt
frontend/
  apps/
  packages/
backend/
  services/
  shared/
infrastructure/
docs/
temp/
```

- Khong tao source moi o root-level `apps/`, `packages/`, `services/`.
- Frontend target: Vue 3, Vite, TypeScript, Pinia, Vue Router, shared UI package.
- Backend target: .NET microservices theo Clean Architecture.
- Data strategy: PostgreSQL, MongoDB, Redis, Kafka/Event Bus khi can scale, SignalR/WebSocket du kien cho realtime.
