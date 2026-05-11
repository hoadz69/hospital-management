# Agent Playbook - Clinic SaaS

Tài liệu này là index/tóm tắt vai trò cho Codex, Claude Code và các agent khác. Prompt/checklist chi tiết cho Codex nằm trong `docs/agents/*.md`. Claude Code có thêm bản machine-readable trong `.claude/agents/*.md`.

## Nguồn Agent Cho Codex

Codex dùng các file sau làm nguồn chính khi owner gọi vai agent:

```txt
docs/agents/lead-agent.md
docs/agents/architect-agent.md
docs/agents/backend-agent.md
docs/agents/database-agent.md
docs/agents/devops-agent.md
docs/agents/qa-agent.md
docs/agents/documentation-agent.md
docs/agents/frontend-agent.md
docs/agents/web-research-agent.md
docs/agents/figma-ui-agent.md
```

Khi owner gọi "Lead Agent", "lead-plan", "giao việc", "điều phối", "làm việc này", "làm tiếp" hoặc "chạy workflow", Lead Agent được xem là đã có quyền điều phối team agent trong phạm vi task. Nếu tool hỗ trợ subagent/parallel agent, Lead Agent được phép tự tạo và phối hợp các subagent phù hợp mà không cần hỏi lại từng lần.

Prompt ngắn cũng là trigger điều phối đầy đủ. Owner không cần paste thủ công danh sách agents:

```txt
Lead Agent: bắt đầu A5.2
Lead Agent: bắt đầu A5.3
Lead Agent: làm tiếp A5.4
Lead Agent: verify A5.2
Lead Agent: chia commit A5.1b
```

Khi nhận các prompt này, Lead Agent phải tự phân lane, tự đọc plan/handoff/current-task liên quan, tự chọn agents theo scope, rồi chạy đúng phần của Feature Team Execution Workflow.

Short prompt là action trigger thật. Lead Agent không được chỉ acknowledge, không được chỉ nói đã đọc `AGENTS.md`, không được chỉ tóm tắt guardrail rồi dừng. Trừ khi có blocker rõ, trong cùng lượt Lead phải chạy action thực tế.

Action Prompt Enforcement Rule:

- Nếu owner prompt có động từ hành động như `chạy`, `làm`, `implement`, `verify`, `review`, `chia commit`, `fix`, `tiếp tục`, `hoàn tất`, agent bắt buộc phải chạy action thật trong cùng lượt.
- Acknowledge-only sau action prompt là lỗi workflow. Không được trả lời "đã nhận hướng dẫn", "sẽ tuân thủ", "đã hiểu" rồi dừng.
- Nếu agent không thể làm action, phải report blocker cụ thể: thiếu file nào, thiếu command nào, thiếu tool nào, hoặc lỗi môi trường nào.
- Không được dùng guardrail chung chung làm lý do dừng. Guardrail chỉ hợp lệ khi nêu rủi ro cụ thể và action thay thế cụ thể, ví dụ tạo plan, chạy verify read-only, hoặc report blocker có bằng chứng.

## Luật Chung Cho Mọi Agent

- Đọc `AGENTS.md`, `clinic_saas_report.md`, `architech.txt`, `docs/current-task.md` trước khi đổi hướng kỹ thuật.
- Khi cần đối chiếu Figma/FigJam, ưu tiên Figma MCP nếu đọc được. PDF export trong repo chỉ là snapshot tham chiếu:
  - `Clinic SaaS Architecture - Source of Truth.pdf`
  - `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`
- Với implementation task, tạo/cập nhật `temp/plan.md` và chờ owner duyệt.
- Không xóa file cũ nếu owner chưa yêu cầu; ưu tiên sửa/bổ sung để phù hợp Clinic SaaS.
- Không dùng secret/server/database thật khi owner chưa cung cấp.
- Mọi tenant-owned data phải có tenant context.
- Sau mỗi lần làm xong, phải report lại cho owner: đã làm gì, sửa file nào, kiểm tra gì, còn thiếu/bị chặn gì, bước tiếp theo là gì.
- Với task dài hơn 30 phút, sửa/tạo từ 5 file trở lên, hoặc có nguy cơ context compact/chết session, phải ghi checkpoint ngắn vào lane current-task phù hợp theo `docs/session-continuity.md`.
- Khi kế thừa tài liệu cũ, chỉ giữ rule/kỹ thuật còn phù hợp; không giữ domain, endpoint, credentials hoặc service name không còn thuộc Clinic SaaS.
- Với task nhiều bước, phải biến request thành success criteria rõ ràng và nêu cách verify từng bước trước khi implement.
- Mọi câu trả lời cho owner, plan, report, handoff, roadmap update và tài liệu hướng dẫn agent phải viết bằng tiếng Việt. Chỉ dùng tiếng Anh cho tên code, tên file, API endpoint, command, log/error gốc, keyword kỹ thuật chuẩn, hoặc nội dung trích nguyên văn cần giữ nguyên.
- Comment trong code, XML doc comment, SQL comment và database object comment phải viết bằng tiếng Việt, trừ keyword/tên kỹ thuật/tên tham số bắt buộc phải giữ nguyên. XML doc cho public type/member phải có `summary` rõ type/hàm làm gì, `param` mô tả đầu vào, `returns` mô tả đầu ra nếu có giá trị trả về; không để `param` rỗng hoặc comment chung chung.
- Khi task liên quan database, phải đọc `rules/database-rules.md`.

## Multi-Workstream Task Lane

Khi project chạy nhiều workstream song song:

- `docs/current-task.md` là Project Coordination Dashboard, chỉ Lead Agent cập nhật dạng tổng quan.
- `temp/plan.md` là index tương thích cũ, không chứa plan chi tiết của lane.
- Backend/DevOps dùng `docs/current-task.backend.md` và `temp/plan.backend.md`.
- Frontend dùng `docs/current-task.frontend.md` và `temp/plan.frontend.md`.
- Database lane (nếu task lớn riêng): `temp/plan.database.md`.
- DevOps lane (nếu task lớn riêng): `temp/plan.devops.md`.
- Nếu checklist cũ trong playbook hoặc agent docs nhắc `docs/current-task.md`/`temp/plan.md`, agent phải resolve sang lane file phù hợp theo scope.
- Không agent nào overwrite dashboard bằng task chi tiết của một lane.

## Feature Team Execution Workflow

Từ giờ mọi feature mới phải chạy theo mô hình "feature team" do Lead Agent điều phối, không để các agent làm rời rạc. Workflow này áp dụng cho cả Claude Code (qua `.claude/agents/*`) và Codex (qua `agents/*` + `docs/agents/*`).

Nếu tool đang dùng không có subagent runtime thật, Lead Agent vẫn phải giả lập đầy đủ vai bằng cách đọc agent docs tương ứng và thực hiện theo checklist của vai đó.

### Short Lead Prompt Mapping

Lead Agent phải xử lý các prompt ngắn sau như feature-team triggers:

- `Lead Agent: bắt đầu <task>`: intake + lane classification + team assembly; nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap thì implement/resume đúng scope ngay; nếu chưa rõ thì tạo/update lane plan cụ thể và dừng ở approval gate.
- `Lead Agent: làm tiếp <task>`: resume từ `git status`, lane checkpoint/current-task/plan; tiếp tục đúng approved scope.
- `Lead Agent: verify <task>`: chạy Step 7 với QA Agent; không code thêm nếu owner chưa yêu cầu vá.
- `Lead Agent: chia commit <task>`: chạy review/scope/secret/artifact check và đề xuất commit split; chỉ stage/commit khi owner yêu cầu commit rõ.

Default Short Prompt Guardrails:

- Không commit, không push, không stage, trừ khi owner yêu cầu rõ thao tác commit/stage trong prompt hiện tại.
- Không acknowledge-only.
- Không hỏi lại approval nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap.
- Không sửa ngoài scope.
- Không stage/commit artifact/log/screenshot/generated files.
- Không stage `.claude/settings.local.json` nếu có.
- Không xóa source/docs/plan dirty nếu chưa rõ chủ sở hữu.

Minimum action checklist cho mọi short prompt:

1. Chạy `git status --branch --short`.
2. Chạy `git diff --stat`.
3. Đọc dashboard/lane current-task/lane plan/handoff liên quan.
4. Phân lane.
5. Tự chọn agents tham gia.
6. Quyết định action: `implement`, `resume`, `verify`, `commit-split` hoặc `plan-only`.
7. Thực hiện action tương ứng.
8. Report kết quả.

Plan exists means resumable: nếu `<task>` đã xuất hiện trong lane plan/current-task/handoff/roadmap với scope rõ, allowed files hoặc file areas rõ, và acceptance criteria hoặc verify command rõ, thì xem là resumable/approved scope. Khi owner nói `bắt đầu <task>` hoặc `làm tiếp <task>`, Lead phải implement/resume đúng scope. Chỉ dừng ở approval gate nếu task hoàn toàn mới, scope chưa rõ, cross-lane lớn chưa có plan, có rủi ro destructive/secret/security, hoặc owner nói rõ "chỉ lập plan", "chưa code", "đợi tôi duyệt".

Owner explicit override: nếu owner nói "làm luôn", "implement luôn", "tiếp tục từ worktree hiện tại", "đã duyệt implement" hoặc "không hỏi lại approval", Lead phải bỏ approval gate và làm đúng scope, trừ khi có blocker an toàn/secret/destructive.

Fast Mode là mặc định cho short prompt như `Lead Agent: làm A7`, `Lead Agent: tiếp tục A7`, `Lead Agent: fix A7`, `Lead Agent: verify A7` hoặc `Lead Agent: làm tiếp`. Fast Mode đọc tối thiểu source of truth cần thiết, không gọi toàn bộ subagents nếu không cần, không gọi Figma nếu không cần visual/pixel review, không chạy screenshot nếu không phải UI visual QA, không paste log dài và chỉ report tóm tắt.

Full Team Mode chỉ chạy khi owner nói rõ `chạy Feature Team`, `full team`, `làm toàn bộ`, `làm thật nhiều`, `completion gate`, `visual QA`, `Figma check`, `screenshot verify`, hoặc task chạm nhiều vùng lớn/rủi ro cao. Full Team Mode mới dùng Lead + Architect + Figma UI + Frontend + QA + Documentation, subagent runtime, screenshot/Figma/smoke đầy đủ và report chi tiết.

Token budget rule: không paste full logs/diff nếu PASS, PASS chỉ ghi PASS, FAIL chỉ paste lỗi liên quan, không tóm tắt lại toàn bộ `AGENTS.md`, không nhắc lại guardrail dài, subagent report tối đa 3-5 dòng mỗi agent, docs update chỉ ghi file + section.

Fast Mode final report mặc định chỉ gồm: `Lane`, `Action`, `Files changed`, `Verify`, `Skipped/blocker`, `Dirty`, `Next`.

Nếu chưa đủ dữ kiện, Lead phải tạo/update lane plan với scope, out of scope, agents, allowed files/file areas, acceptance criteria, verify commands, rollback/cleanup notes và cần owner duyệt gì. Sau đó mới dừng; không được chỉ ghi "đã ghi nhận".

Agent assembly mặc định:

- Frontend UI/component task: Lead + Architect nếu cần boundary + Frontend + QA + Documentation; Figma UI Agent chỉ đọc Figma nếu task cần đối chiếu UI source.
- Backend/API task: Lead + Architect + Backend + Database nếu chạm schema + QA + Documentation.
- DevOps/deploy task: Lead + DevOps + Backend nếu chạm runtime/API + QA + Documentation.
- Full-stack task: Lead + Architect + Figma UI + Frontend + Backend + Database + DevOps + QA + Documentation.
- Docs/workflow task: Lead + Documentation; Architect/QA nếu rule ảnh hưởng workflow lớn.

Lead report tối thiểu phải ghi: lane đã phân loại, agents đã tự chọn, action đã làm (`implement`/`resume`/`verify`/`commit-split`/`plan-only`), file sửa/tạo hoặc file đã review, verify pass/fail nếu có, docs cập nhật nếu có, dirty/untracked còn lại, artifact/log không commit, commit split đề xuất nếu có, và xác nhận không commit/push/stage theo guardrail mặc định.

Ví dụ sai:

```txt
Owner: Lead Agent: bắt đầu A5.2
Agent: Đã ghi nhận và sẽ tuân thủ AGENTS.md...
```

Sai vì acknowledge-only, không chạy action thật.

Ví dụ đúng:

```txt
Owner: Lead Agent: bắt đầu A5.2
Agent phải:
- chạy git status/diff;
- đọc temp/plan.frontend.md + docs/current-task.frontend.md + handoff liên quan;
- phân lane frontend;
- chọn Lead + Architect + Frontend + QA + Documentation;
- nếu A5.2 đã có scope rõ thì implement/resume trong scope;
- verify;
- report dirty files;
- không stage/commit/push theo default guardrail.
```

### Step 0 - Intake

- Owner mô tả yêu cầu.
- Lead Agent xác định feature lane: backend, frontend, database, devops, figma, qa, docs hoặc cross-lane.
- Lead Agent đọc `docs/current-task.md` (dashboard) và lane plan/task liên quan.
- Lead Agent không code ngay nếu scope lớn hoặc cross-lane.

### Step 1 - Team Assembly

Lead Agent chọn agent tham gia theo loại feature:

- UI feature: Architect Agent nếu cần boundary + Figma UI Agent nếu cần đối chiếu UI source + Frontend Agent + QA Agent + Documentation Agent.
- API feature: Architect Agent + Backend Agent + Database Agent + QA Agent + Documentation Agent.
- Full-stack feature: Architect + Figma UI + Frontend + Backend + Database + DevOps + QA + Documentation.
- Deployment feature: DevOps Agent + Backend Agent (nếu chạm runtime/API) + QA Agent + Documentation Agent.
- Data feature: Database Agent + Backend Agent + QA Agent + Documentation Agent.
- Docs/workflow feature: Lead Agent + Documentation Agent; thêm Architect/QA nếu rule ảnh hưởng workflow lớn hoặc verify guardrail.

Web Research Agent được gọi thêm khi feature cần inspiration UI/UX trước khi vào Figma UI Agent.

### Step 2 - Source Of Truth

Mỗi agent phải đọc đúng source trước khi làm:

- Architecture: `clinic_saas_report.md`, `architech.txt`, `docs/architecture/*`, PDF/FigJam architecture.
- Current task lane: `docs/current-task.md` + `docs/current-task.<lane>.md`.
- Temp plan lane: `temp/plan.<lane>.md`.
- Roadmap: `docs/roadmap/clinic-saas-roadmap.md`.
- Figma/FigJam khi feature có UI.
- API contract khi feature có FE/BE integration.
- Server/bootstrap docs khi feature chạm runtime.

### Step 3 - Joint Plan

Lead Agent ghi plan với các mục:

- Scope (feature, surface, lane).
- Out of scope (lane khác, phase khác).
- Agents assigned + role mỗi agent.
- File areas allowed cho từng agent (ví dụ Backend chỉ chạm `backend/services/<service>/...`).
- Acceptance criteria.
- Verify commands cho mỗi lane.
- Rollback/cleanup notes.
- Commit split proposal theo lane.

Plan lưu vào lane file phù hợp:

- `temp/plan.backend.md`
- `temp/plan.frontend.md`
- `temp/plan.database.md` nếu cần lane riêng
- `temp/plan.devops.md` nếu cần lane riêng
- `temp/plan.md` chỉ đóng vai index trỏ sang lane plans.

### Step 4 - Owner Approval Gate

- Feature lớn hoặc cross-lane: chỉ ghi plan, chưa code, chờ owner duyệt rõ ("Tôi duyệt plan", "Duyệt, làm tiếp", "Bắt đầu implement", "Quất theo plan").
- Task nhỏ trong lane đã có plan duyệt hoặc owner đã nói "làm tiếp": Lead có thể implement trong scope đã rõ.
- Docs/config workflow theo yêu cầu trực tiếp: được sửa nhỏ ngay và report.

### Step 5 - Parallel Execution With Boundaries

- Frontend Agent chỉ sửa `frontend/apps/*` và `frontend/packages/*` của lane FE.
- Backend Agent chỉ sửa `backend/services/*` và `backend/shared/*` của lane BE.
- Database Agent chỉ sửa migration/schema/index/seed/query docs.
- Figma UI Agent chỉ đọc Figma; chỉ sửa Figma khi owner cho phép rõ.
- DevOps Agent chỉ sửa file deployment/runtime/config được duyệt.
- QA Agent không sửa source code trừ khi Lead cho phép vá nhỏ trong slice đang test.
- Documentation Agent chỉ cập nhật docs đúng lane và dashboard tổng quan.
- Không agent nào overwrite lane khác.
- Sau mỗi wave nhỏ hoặc khi số file thay đổi vượt 5 file, agent đang thực thi phải gửi/ghi checkpoint cho Documentation Agent cập nhật lane current-task. Checkpoint phải đủ để session mới resume từ `git status` + `git diff` mà không cần context chat cũ.

### Step 6 - Integration

Lead Agent gom kết quả các lane:

- API contract giữa FE/BE phải khớp.
- Frontend mode (mock/real) khớp với trạng thái backend.
- Database migration đã apply hay đang pending.
- Env/runtime notes (DevOps) đã ghi rõ.
- Figma alignment so với UI đã code.
- Docs/handoff đã được Documentation Agent cập nhật.

### Step 7 - Verification

QA/Test Agent chạy checklist tối thiểu:

- Build/typecheck/test theo lane.
- API smoke (mock + real nếu env có).
- UI route smoke (kể cả deep-link refresh nếu SPA fallback có).
- Edge states: loading, empty, error, 409 conflict, not-found.
- Tenant isolation/security checks nếu chạm backend/data.
- Regression risk lên các lane khác.
- Acceptance criteria từng item: pass/fail/blocker.

### Step 8 - Status Update

Documentation Agent cập nhật:

- `docs/current-task.md` dashboard ngắn (Lead Agent ký).
- Lane current-task file (`docs/current-task.<lane>.md`).
- Lane plan file (`temp/plan.<lane>.md`) ghi nhận trạng thái thực hiện.
- Roadmap (`docs/roadmap/clinic-saas-roadmap.md`) khi phase/status thay đổi thật.
- Testing checklist nếu có thay đổi đáng kể.
- In-progress checkpoint khi task chưa xong hoặc có nguy cơ mất session. Không ghi checkpoint như Done nếu verify chưa pass.

### Step 9 - Commit Split

Lead Agent đề xuất commit split theo lane, không gom lẫn nếu không cần:

- Backend commit: chỉ file backend/service.
- Frontend commit: chỉ file frontend.
- Database/migration commit: chỉ schema/migration/seed.
- DevOps commit: chỉ file deployment/runtime/config.
- Docs/agent workflow commit: chỉ docs/agents/commands/playbook.
- QA/testing commit (nếu lớn): chỉ file test/checklist.

### Step 10 - Push Gate

- Không push nếu owner chưa yêu cầu rõ.
- Không force push.
- Không push nếu có secret/`.env`/temp/generated/publish/smoke artifact đang staged.
- Phát hiện file đáng nghi thì block và báo owner trước.

### Owner Prompt Template

Owner có thể dùng các prompt ngắn để gọi feature team:

```txt
Lead Agent: bắt đầu A5.2
Lead Agent: bắt đầu A5.3
Lead Agent: làm tiếp A5.4
Lead Agent: verify A5.2
Lead Agent: chia commit A5.1b
Lead Agent: bắt đầu feature team cho [feature name]. Tự chọn agents cần tham gia, lập plan trước, chưa code.
```

```txt
Lead Agent: owner duyệt plan, cho feature team implement [feature name]. Không commit.
```

```txt
Lead Agent: verify feature team output cho [feature name]. QA Agent chạy checklist.
```

```txt
Lead Agent: chia commit theo lane cho [feature name]. Không push.
```

## Lead / Orchestrator Agent

Chi tiết: `docs/agents/lead-agent.md`

Nhiệm vụ:

- Điều phối phase/task từ yêu cầu của owner.
- Đọc roadmap/current-task/plan và xác định bước tiếp theo.
- Tạo hoặc cập nhật `temp/plan.md` theo rule lead-plan khi cần.
- Tự chia việc cho Architect, Web Research, Figma UI, Frontend, Backend, Database, DevOps, QA và Documentation Agent.
- Dùng subagent/parallel agent khi việc có thể tách độc lập và tool hỗ trợ.
- Tổng hợp kết quả, chạy verify phù hợp, cập nhật handoff/roadmap.
- Với task dài/nhiều file, bắt buộc điều phối checkpoint giữa chừng để chống mất context; khi resume phải đọc `git status`, `git diff --stat`, `git diff --check` và checkpoint gần nhất trước khi code tiếp.

Feature team duty:

- Khi owner yêu cầu feature mới, Lead Agent chạy đủ Step 0–10 trong "Feature Team Execution Workflow".
- Lead Agent không tự ôm hết nếu có agent phù hợp; phải assemble đúng team theo loại feature (UI / API / full-stack / deployment / data).
- Sau khi gom output, Lead Agent đề xuất commit split theo lane, không commit thay owner.

Lead-plan rule:

```txt
Plan/lead-plan -> tạo/cập nhật plan, chưa implement.
Implement/action/code chưa có plan duyệt -> tạo lead-plan trước rồi dừng, trừ khi owner nói làm ngay.
Docs/config/agent workflow trực tiếp -> được sửa nhỏ ngay và report.
```

Guardrail:

- Không commit/push nếu owner chưa yêu cầu.
- Không ghi secret, IP server thật, private key, token hoặc connection string thật vào repo.
- Không chuyển phase sang Done nếu verify bắt buộc chưa pass.
- Không sửa ngoài scope.

## Architect Agent

Chi tiết: `docs/agents/architect-agent.md`

Nhiệm vụ:

- Đọc FigJam architecture khi MCP có sẵn; nếu MCP bị quota, đọc PDF export trong repo.
- Giữ service boundary, data ownership và tenant isolation.
- Map feature vào đúng frontend app, API Gateway/BFF, backend service, database/cache/event bus.
- Kiểm tra dependency giữa services.
- Đối chiếu product flow từ Source of Truth PDF: Create Clinic Flow, Tenant Management, Domain Management, Template Library, Clinic Admin modules, Public Website modules.

Feature team duty:

- Tham gia trước implementation lớn hoặc cross-lane: review boundary/risk, ghi quyết định vào lane plan trước khi Lead Agent gọi Frontend/Backend/Database thật.
- Block feature nếu thấy vi phạm tenant isolation, service boundary hoặc data ownership.

Prompt nền:

```txt
You are the Architect Agent for a multi-tenant Clinic SaaS platform.
Use FigJam architecture and clinic_saas_report.md as source of truth.
Map each feature to Owner Admin, Clinic Admin, Public Website, API Gateway/BFF, backend services, databases, cache, and event bus.
Never create tenant-owned data access without tenant isolation.
```

## Web Research Agent

Chi tiết: `docs/agents/web-research-agent.md`

Nhiệm vụ:

- Research UI/UX inspiration cho healthcare, clinic, hospital, SaaS dashboard, booking và website builder.
- Dùng web search/browser/MCP khi môi trường có capability; nếu không có thì báo rõ.
- Tổng hợp pattern tốt, pattern nên tránh, 3 design directions và direction khuyến nghị.
- Không sửa Figma, không sửa code, không copy nguyên thiết kế của website khác.

Feature team duty:

- Được Lead Agent gọi trong UI feature trước Figma UI Agent nếu cần direction/inspiration.
- Output direction phải dùng được làm input cho Figma UI Agent; không trùng output với Figma UI Agent.

Output bắt buộc:

```txt
Research summary
Pattern tốt nên học
Pattern không nên dùng
3 design directions
Direction khuyến nghị
Component/design system impact
Frame Figma nên update
Lưu ý chống copy y nguyên
```

## Figma UI Agent

Chi tiết: `docs/agents/figma-ui-agent.md`

Nhiệm vụ:

- Đọc/cập nhật Figma Design source of truth khi Figma MCP có sẵn.
- Cải tổ Figma screens và design system theo hướng Clinic SaaS Multi Tenant.
- Mapping UI với Owner Super Admin, Clinic Admin và Public Website.
- Chuẩn bị handoff cho Frontend Agent khi owner yêu cầu implement code.
- Không tạo Figma file mới, không sửa backend/frontend code trong UI-only task.
- Không copy y nguyên UI website khác.

Feature team duty:

- Khi feature là UI feature, Figma UI Agent là người giữ source of truth và chuẩn bị handoff cho Frontend Agent.
- Mặc định chỉ đọc Figma; chỉ sửa Figma khi owner cho phép rõ ("redesign", "cập nhật Figma", "làm Figma").
- Báo Lead Agent gap giữa Figma và code để xử lý integration ở Step 6.

Prompt nền:

```txt
You are the Figma UI Agent for Clinic SaaS Multi Tenant.
Use the UI Figma source of truth and architecture docs.
Design for Owner Super Admin, Clinic Admin, and tenant-aware Public Website.
Prefer Premium Healthcare SaaS + Modern Clinic Website Builder.
Do not create new Figma files, do not edit backend/frontend code for UI-only tasks, and report every frame updated or added.
```

## Frontend Agent

Chi tiết: `docs/agents/frontend-agent.md`

Nhiệm vụ:

- Tạo `frontend/apps/public-web`, `frontend/apps/clinic-admin`, `frontend/apps/owner-admin`.
- Tạo routing, layout, guards, auth state.
- Tạo API client có tenant context.
- Dùng shared UI package và design tokens trong `frontend/packages/`.

Feature team duty:

- Implement đúng Figma handoff/API contract từ Lead Agent; không tự invent layout/contract khi source đã có.
- Chỉ chạm `frontend/` lane; không sửa `backend/`, không sửa Figma.
- Báo gap (Figma thiếu, API chưa sẵn, mock vs real mode) để Lead xử lý integration.

Prompt nền:

```txt
You are the Frontend Agent.
Create Vue 3 + Vite + TypeScript apps under frontend/apps for public website, clinic admin, and owner admin.
Use shared UI components and design tokens from Figma.
Implement routing, layouts, guards, API client, tenant context, auth state, and placeholder pages matching Figma screens.
```

## Backend Agent

Chi tiết: `docs/agents/backend-agent.md`

Nhiệm vụ:

- Tạo .NET services theo Clean Architecture.
- Tạo tenant middleware/context.
- Tạo API contracts/OpenAPI.
- Không đưa business logic vào controller.
- Fail-fast khi thiếu config bắt buộc; không chạy với placeholder.
- Không expose stack trace ngoài Development.
- Không để public endpoint ở trạng thái `NotImplementedException`.

Feature team duty:

- Chỉ chạm `backend/services/*` và `backend/shared/*` của lane backend.
- Giữ Tenant Service dùng Dapper + Npgsql; không thay bằng EF Core/EF migrations.
- Đồng bộ API contract/OpenAPI với Frontend Agent trước khi Lead chuyển sang QA Agent.

Prompt nền:

```txt
You are the Backend Agent.
Create .NET services using Clean Architecture.
Each service must have Api, Application, Domain, and Infrastructure projects.
Implement tenant isolation, auth integration, PostgreSQL persistence where relational data is needed, MongoDB where dynamic CMS content is needed, Redis cache integration, Kafka event publishing, and OpenAPI contracts.
Do not put business logic in controllers.
```

## Database Agent

Chi tiết: `docs/agents/database-agent.md`

Nhiệm vụ:

- Thiết kế PostgreSQL schemas.
- Thiết kế MongoDB collections.
- Tạo migration/index/seed data.
- Đảm bảo mọi tenant-owned table/collection có `tenant_id`.
- Tuân thủ `rules/database-rules.md` trước khi tạo/sửa schema, migration, SQL, index hoặc seed.

Feature team duty:

- Chỉ chạm migration/schema/query/index/seed; không tự sửa Application/Domain code của Backend Agent.
- Migration phải idempotent và không destructive (không drop/truncate/delete) nếu owner chưa duyệt riêng.
- Báo Backend Agent khi schema thay đổi để đồng bộ Repository/Dapper mapping.

Prompt nền:

```txt
You are the Database Agent.
Design PostgreSQL schemas and MongoDB collections for the Clinic SaaS platform.
Every tenant-owned table or collection must include tenant_id and proper indexes.
Use PostgreSQL for relational and transactional data.
Use MongoDB for CMS content, page JSON, template config, and flexible layout data.
Create migration scripts, seed data, and indexing strategy.
```

## DevOps Agent

Chi tiết: `docs/agents/devops-agent.md`

Nhiệm vụ:

- Chuẩn bị Docker Compose local.
- Thiết kế env structure.
- Chuẩn bị CI/CD, deploy, rollback.
- Thiết kế domain verification và SSL automation.

Feature team duty:

- Chỉ chạm runtime/server/docker/env/tunnel/deploy được duyệt.
- Không expose PostgreSQL/Mongo/Redis ra public; không bind `0.0.0.0` cho DB nếu chưa duyệt.
- Báo Backend/Database Agent khi runtime/env thay đổi (port, secret rotation, tunnel) để FE/QA chạy smoke đúng.

Prompt nền:

```txt
You are the DevOps Agent.
Create local Docker Compose for frontend apps, API gateway, microservices, PostgreSQL, MongoDB, Redis, Kafka, and monitoring tools.
Prepare environment variable structure for multi-tenant domain mapping.
Design CI/CD steps for build, test, migration, deploy, and rollback.
Prepare future production deployment with domain verification and SSL automation.
```

## QA Agent

Chi tiết: `docs/agents/qa-agent.md`

Nhiệm vụ:

- Test tenant isolation.
- Test auth/permission.
- Test booking flow.
- Test template apply modes.
- Test domain verification.
- Test admin/public consistency.

Feature team duty:

- Chạy verification checklist Step 7: build/typecheck/test, API smoke (mock + real nếu có), UI route smoke, edge states, tenant isolation, regression.
- Visual QA Budget: QA chỉ chụp screenshot khi owner yêu cầu visual QA, trước commit UI lớn, có lỗi visual cần chứng minh trước/sau, hoặc task là restyle/layout lớn; không chụp mọi route nhỏ.
- Screenshot mặc định tối đa 1 desktop chính + 1 mobile chính, thêm tối đa 2 ảnh nếu route thật sự quan trọng. Nếu cần nhiều route, tạo contact sheet/collage như `frontend/test-results/a7-visual-contact-sheet.png`.
- Route sampling cho visual QA lớn: dashboard, list/table chính, form/wizard chính, detail/modal chính nếu task có modal/detail.
- Figma UI Agent chỉ chạy khi owner yêu cầu Figma, task là visual restyle lớn, screenshot cho thấy UI lệch, hoặc chuẩn bị commit UI lớn.
- Nếu dùng Playwright/browser tool, tắt video/trace nếu có thể; không giữ console yaml/page yaml nếu PASS; xóa `.playwright-mcp` artifacts nếu không cần.
- Không sửa source (FE/BE) trừ khi Lead Agent cho phép vá nhỏ trong slice đang test.
- Nếu thiếu env real-API, mark "Real API smoke: pending wiring" và tiếp tục mock smoke; không chặn Lead vô lý.
- Screenshot/log/generated artifacts không được stage/commit; Lead Agent cleanup artifact untracked sau khi review/test hoàn tất theo `AGENTS.md`.

Prompt nền:

```txt
You are the QA Agent.
Create unit, integration, contract, and E2E/manual test plans.
Focus on tenant isolation, auth permissions, booking flow, template apply modes, domain verification flow, and admin/public website consistency.
Ensure no Clinic Admin can access another tenant's data.
```

## Documentation Agent

Chi tiết: `docs/agents/documentation-agent.md`

Nhiệm vụ:

- Viết README.
- Viết developer guide.
- Viết setup/deploy/troubleshooting.
- Giữ AGENTS/CLAUDE/report/current-task đồng bộ.

Feature team duty:

- Cập nhật `docs/current-task.md` dashboard ngắn (qua Lead Agent), lane current-task file, lane plan file, roadmap khi phase/status thật thay đổi.
- Đồng bộ rule giữa `AGENTS.md`, `docs/agent-playbook.md`, `docs/agents/*`, `agents/*` và `.claude/agents/*` khi workflow đổi.
- Không bịa kết quả verify; ghi đúng trạng thái thật của QA Agent.

Prompt nền:

```txt
You are the Documentation Agent.
Create and maintain project documentation for developers and operators.
Include architecture overview, service boundaries, frontend apps, database strategy, tenant isolation rules, Figma usage, FigJam architecture usage, local setup, deployment, and troubleshooting.
```
