# AGENTS.md - Clinic SaaS / Hospital Management

File này là hướng dẫn chung cho Codex, Claude Code và các coding agent khác khi làm việc trong repo `hospital-management`.

## ⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT CODE

1. Đọc file này trước mọi thay đổi.
2. Đọc `clinic_saas_report.md`, `architech.txt` và `docs/current-task.md` trước khi đổi hướng kỹ thuật.
3. Đọc rule trong `rules/` đúng ngữ cảnh trước khi viết/sửa code:
   - `rules/coding-rules.md` cho mọi task code.
   - `rules/backend-coding-rules.md` cho backend.
   - `rules/backend-testing-rules.md` khi test API/backend.
   - `rules/database-rules.md` khi tạo/sửa schema, migration, SQL, repository persistence, index, seed hoặc data access.
4. Nếu là task implementation, phải tạo/cập nhật plan trước và chờ owner duyệt, trừ khi owner nói rõ là làm ngay. Với multi-workstream, dùng plan lane tương ứng như `temp/plan.backend.md` hoặc `temp/plan.frontend.md`; `temp/plan.md` chỉ là index.
5. Khi owner chỉ hỏi/phân tích, không được tự code. Chỉ đọc file liên quan, phân tích và trả lời.
6. Nếu chỉ dọn tài liệu/config theo yêu cầu trực tiếp của owner, được sửa ngay nhưng phải giữ scope nhỏ và ghi rõ file đã sửa.
7. Không commit nếu owner chưa yêu cầu.
8. Không xóa file nếu owner chưa yêu cầu, trừ file mới do chính task hiện tại tạo ra.
9. Không đưa secret, IP server thật, chuỗi kết nối database thật, token, SSH key vào repo.
10. Với task dài hơn 30 phút, sửa/tạo từ 5 file trở lên, hoặc có nguy cơ chết session/context compact, phải ghi checkpoint ngắn vào lane current-task phù hợp theo `docs/session-continuity.md` trước khi tiếp tục wave kế tiếp.
11. Sau mỗi lần làm xong phải report lại cho owner: đã làm gì, sửa file nào, kiểm tra gì, còn thiếu/bị chặn gì, bước tiếp theo là gì. Không được im lặng sau khi chạy tool hoặc sửa file.

## Command Execution Rule

1. Khi owner yêu cầu chạy lead-plan hoặc tạo plan:
   - Làm ngay.
   - Không hỏi lại.
   - Không chỉ xác nhận đã hiểu.
   - Được phép cập nhật:
     - `temp/plan.md` hoặc plan lane tương ứng
     - `docs/current-task.md` hoặc current-task lane tương ứng
     - `docs/roadmap/clinic-saas-roadmap.md`
   - Không code implementation.
   - Sau khi tạo plan xong mới dừng lại để owner duyệt action/implementation.

2. Khi owner yêu cầu verify/review/update-roadmap:
   - Làm ngay.
   - Không hỏi lại.
   - Không code implementation.
   - Không commit.

3. Khi owner yêu cầu implement/action/code:
   - Chỉ làm nếu plan đã được owner duyệt.
   - Nếu chưa có plan, tự chạy lead-plan trước rồi dừng lại chờ owner duyệt.
   - Không tự implement khi chưa có câu duyệt rõ như:
     - "Tôi duyệt plan"
     - "Duyệt, làm tiếp"
     - "Bắt đầu implement"
     - "Quất theo plan"

4. Không commit trừ khi owner yêu cầu rõ.
5. Không tạo Figma file mới trừ khi owner yêu cầu rõ.
6. Sau mỗi phase/task lớn phải cập nhật:
   - `docs/current-task.md` hoặc current-task lane tương ứng
   - `docs/roadmap/clinic-saas-roadmap.md`

## Multi-Workstream Task Lane Rule

Project có thể chạy nhiều workstream song song. Khi có nhiều lane đang hoạt động, không dùng `docs/current-task.md` hoặc `temp/plan.md` để chứa chi tiết của một lane.

Quy ước hiện tại:

- `docs/current-task.md` là Project Coordination Dashboard, chỉ do Lead Agent cập nhật dạng tổng quan ngắn và trỏ sang lane files.
- `temp/plan.md` là index tương thích cũ, không chứa plan chi tiết của backend hoặc frontend.
- Backend Agent và DevOps Agent chỉ cập nhật `docs/current-task.backend.md` và `temp/plan.backend.md`.
- Frontend Agent chỉ cập nhật `docs/current-task.frontend.md` và `temp/plan.frontend.md`.
- Database lane riêng (nếu task lớn): `temp/plan.database.md`. DevOps lane riêng (nếu task lớn): `temp/plan.devops.md`. Mặc định DevOps đi cùng backend lane khi task nhỏ.
- Lead Agent chịu trách nhiệm tạo lane mới hoặc phân lane khi owner nói chung chung.
- Không agent nào overwrite `docs/current-task.md` bằng task chi tiết của một lane.
- Nếu không chắc nội dung thuộc lane nào, ghi một note ngắn vào `docs/current-task.md` phần Notes/Unclassified và chờ Lead Agent phân loại.

## Feature Team Execution Workflow

Mọi feature mới phải chạy theo mô hình "feature team" do Lead Agent điều phối. Workflow này áp dụng cho cả Codex và Claude Code, kể cả khi tool không có subagent runtime thật (Lead Agent giả lập role bằng cách đọc agent docs tương ứng).

Chi tiết đầy đủ Step 0–10, agent assembly theo loại feature, owner prompt template nằm trong `docs/agent-playbook.md` (section "Feature Team Execution Workflow").

### Short Lead Prompt Rule

Khi owner gọi ngắn, Lead Agent phải tự hiểu là yêu cầu chạy Feature Team Execution Workflow tương ứng, không bắt owner liệt kê thủ công "Agents tham gia":

```txt
Lead Agent: bắt đầu <task>
Lead Agent: làm tiếp <task>
Lead Agent: verify <task>
Lead Agent: chia commit <task>
```

Các prompt trên là **action trigger thật**, không phải lời chào/khởi động vai. Lead Agent không được trả lời kiểu acknowledge-only như "Đã ghi nhận AGENTS.md", "Tôi sẽ tuân thủ guardrail", "Đã hiểu" hoặc "Tôi sẽ đọc source of truth trước" rồi dừng. Trừ khi gặp blocker rõ, Lead Agent phải làm hành động thật trong cùng lượt.

Action Prompt Enforcement Rule:
- Nếu owner prompt có động từ hành động như `chạy`, `làm`, `implement`, `verify`, `review`, `chia commit`, `fix`, `tiếp tục`, `hoàn tất`, agent bắt buộc phải chạy action thật trong cùng lượt.
- Acknowledge-only sau action prompt là lỗi workflow. Không được trả lời "đã nhận hướng dẫn", "sẽ tuân thủ", "đã hiểu" rồi dừng.
- Nếu không thể làm action, agent phải report blocker cụ thể: thiếu file nào, thiếu command nào, thiếu tool nào, hoặc lỗi môi trường nào.
- Không được dùng guardrail chung chung làm lý do dừng. Guardrail chỉ hợp lệ khi nêu rủi ro cụ thể và action thay thế cụ thể, ví dụ tạo plan, chạy verify read-only, hoặc report blocker có bằng chứng.

Guardrail mặc định cho mọi short prompt, owner không cần nhắc lại:
- Không commit, không push, không stage, trừ khi owner yêu cầu rõ thao tác commit/stage trong prompt hiện tại.
- Không hỏi lại approval nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap.
- Không sửa ngoài scope; không xóa source/docs/plan dirty nếu chưa rõ chủ sở hữu.
- Không stage/commit artifact/log/screenshot/generated files.
- Không stage `.claude/settings.local.json` nếu có.

Tối thiểu mỗi short prompt phải thực hiện:
- Chạy `git status --branch --short` và `git diff --stat`.
- Đọc dashboard/lane current-task/lane plan/handoff liên quan.
- Phân lane, tự chọn agents tham gia, quyết định action: implement/resume/verify/commit-split/plan-only.
- Thực hiện action tương ứng và report kết quả.

Quy ước:
- `bắt đầu <task>`: Lead phân lane, đọc source of truth/lane plan/current-task/handoff, tự chọn agents; nếu task đã có scope rõ trong lane plan/current-task/handoff/roadmap thì implement/resume đúng scope ngay; nếu scope chưa rõ thì lập/update lane plan cụ thể rồi dừng ở approval gate.
- `làm tiếp <task>`: Lead resume từ `git status` + lane checkpoint/plan, tự chọn agents, tiếp tục đúng approved scope.
- `verify <task>`: Lead gọi QA checklist phù hợp, không code thêm nếu không có lỗi được owner cho phép vá.
- `chia commit <task>`: Lead review dirty/staged files, đề xuất hoặc thực hiện commit split chỉ khi owner yêu cầu commit rõ; không stage/commit/push nếu owner chưa yêu cầu.

Plan exists means resumable: nếu `<task>` đã xuất hiện trong lane plan/current-task/handoff/roadmap với scope rõ, allowed files/file areas rõ, và acceptance criteria hoặc verify command rõ, thì xem là resumable/approved scope. Chỉ dừng approval gate khi task hoàn toàn mới, scope chưa rõ, cross-lane lớn chưa có plan, có rủi ro destructive/secret/security, hoặc owner nói rõ "chỉ lập plan", "chưa code", "đợi tôi duyệt".

Owner explicit override: nếu owner nói "làm luôn", "implement luôn", "tiếp tục từ worktree hiện tại", "đã duyệt implement" hoặc "không hỏi lại approval", Lead Agent phải bỏ approval gate và làm đúng scope, trừ khi có blocker an toàn/secret/destructive.

Fast Mode là mặc định cho các prompt ngắn như `Lead Agent: làm A7`, `Lead Agent: tiếp tục A7`, `Lead Agent: fix A7`, `Lead Agent: verify A7` hoặc `Lead Agent: làm tiếp`, trừ khi owner yêu cầu rõ full team/visual QA/completion gate. Fast Mode phải đọc tối thiểu source of truth cần thiết, không gọi toàn bộ subagents nếu không cần, không gọi Figma nếu không cần visual/pixel review, không chạy screenshot nếu không phải UI visual QA, không paste log dài và chỉ report tóm tắt.

Full Team Mode chỉ chạy khi owner nói rõ `chạy Feature Team`, `full team`, `làm toàn bộ`, `làm thật nhiều`, `completion gate`, `visual QA`, `Figma check`, `screenshot verify`, hoặc task chạm nhiều vùng lớn/rủi ro cao. Full Team Mode mới dùng Lead + Architect + Figma UI + Frontend + QA + Documentation, subagent runtime, screenshot/Figma/smoke đầy đủ và report chi tiết.

Token budget rule:
- Không paste full logs nếu PASS; nếu verify PASS chỉ ghi PASS.
- Nếu verify FAIL, chỉ paste lỗi liên quan trực tiếp.
- Không paste full diff, không tóm tắt lại toàn bộ `AGENTS.md`, không nhắc lại guardrail dài.
- Không report từng command quá chi tiết nếu không fail.
- Subagent report tối đa 3-5 dòng mỗi agent.
- Docs update chỉ ghi file + section, không paste toàn bộ nội dung.

Fast Mode final report mặc định chỉ gồm: `Lane`, `Action`, `Files changed`, `Verify`, `Skipped/blocker`, `Dirty`, `Next`.

Nếu chưa đủ dữ kiện, Lead Agent phải tạo/update lane plan cụ thể gồm scope, out of scope, agents, allowed files/file areas, acceptance criteria, verify commands, rollback/cleanup notes và phần cần owner duyệt; sau đó mới dừng. Không được chỉ acknowledge.

Owner không cần ghi "Agents tham gia". Lead Agent tự assemble:
- Frontend UI/component task: Lead + Architect nếu cần boundary + Frontend + QA + Documentation; Figma UI Agent chỉ đọc Figma nếu cần đối chiếu UI source.
- Backend/API task: Lead + Architect + Backend + Database nếu chạm schema + QA + Documentation.
- DevOps/deploy task: Lead + DevOps + Backend nếu chạm runtime/API + QA + Documentation.
- Full-stack task: Lead + Architect + Figma UI + Frontend + Backend + Database + DevOps + QA + Documentation.
- Docs/workflow task: Lead + Documentation; Architect/QA nếu rule ảnh hưởng workflow lớn.

Nếu có subagent runtime thật, Lead Agent được phép spawn/call subagents phù hợp. Nếu không có, Lead vẫn phải giả lập tuần tự đầy đủ các vai bằng cách đọc agent docs tương ứng và thực hiện checklist của từng vai.

Report tối thiểu cho mọi short prompt: lane đã phân loại; agents đã chọn; action đã làm (`implement`/`resume`/`verify`/`commit-split`/`plan-only`); file sửa/tạo hoặc file đã review; verify pass/fail nếu có; docs cập nhật nếu có; dirty/untracked còn lại; artifact/log không commit; xác nhận mặc định không commit/push/stage.

Ví dụ sai:
```txt
Owner: Lead Agent: bắt đầu A5.2
Agent: Đã ghi nhận và sẽ tuân thủ AGENTS.md...
```
Sai vì acknowledge-only, không chạy action thật.

Ví dụ đúng:
```txt
Owner: Lead Agent: bắt đầu A5.2
Agent phải chạy git status/diff, đọc temp/plan.frontend.md + docs/current-task.frontend.md + handoff liên quan,
phân lane frontend, chọn Lead + Architect + Frontend + QA + Documentation, implement/resume nếu A5.2 đã có scope rõ,
verify, report dirty files và không stage/commit/push theo default guardrail.
```

Quy trình tóm tắt:

- Step 0 Intake: Lead Agent xác định feature lane và đọc dashboard/lane plan liên quan.
- Step 1 Team Assembly: Lead Agent chọn agent theo loại feature (UI / API / full-stack / deployment / data).
- Step 2 Source Of Truth: từng agent đọc đúng architecture docs, current-task lane, plan lane, roadmap, Figma, API contract, server docs khi cần.
- Step 3 Joint Plan: Lead Agent ghi plan trong lane file phù hợp với scope/out-of-scope/agents/file-areas/acceptance/verify/rollback/commit-split.
- Step 4 Owner Approval Gate: feature lớn hoặc cross-lane chỉ plan, chưa code; chờ owner duyệt rõ.
- Step 5 Parallel Execution With Boundaries: mỗi agent chỉ chạm lane của mình, không overwrite lane khác.
- Step 6 Integration: Lead Agent gom kết quả các lane (API contract, FE mode, migration, env, Figma, docs).
- Step 7 Verification: QA Agent chạy build/typecheck/test, API smoke, UI smoke, edge states, tenant isolation, regression.
- Step 8 Status Update: Documentation Agent cập nhật dashboard/lane current-task/lane plan/roadmap.
- Step 9 Commit Split: Lead Agent đề xuất commit theo lane, không gom lẫn.
- Step 10 Push Gate: không push nếu owner chưa yêu cầu, không force push, không push secret/temp/generated artifact.

Owner prompt template ngắn:

```txt
Lead Agent: bắt đầu feature team cho [feature name]. Tự chọn agents cần tham gia, lập plan trước, chưa code.
Lead Agent: owner duyệt plan, cho feature team implement [feature name]. Không commit.
Lead Agent: verify feature team output cho [feature name]. QA Agent chạy checklist.
Lead Agent: chia commit theo lane cho [feature name]. Không push.
```

Prompt chuẩn tiết kiệm:

```txt
Lead Agent: tiếp tục A7 fast mode
Lead Agent: làm A7 fast mode
Lead Agent: fix lỗi A7
Lead Agent: visual QA A7 budget mode
Lead Agent: làm toàn bộ A7 full team
Lead Agent: chạy Feature Team cho A7
Lead Agent: finalize A7, cleanup artifacts và push
```

## Language Rule

- Mọi câu trả lời cho owner, plan, report, handoff, roadmap update và tài liệu hướng dẫn agent phải viết bằng tiếng Việt.
- Chỉ dùng tiếng Anh khi đó là tên code, tên file, API endpoint, command, log/error gốc, keyword kỹ thuật chuẩn, hoặc nội dung trích nguyên văn cần giữ nguyên.
- Nếu tạo/cập nhật `temp/plan.md`, plan lane, `docs/current-task.md`, current-task lane, `docs/roadmap/clinic-saas-roadmap.md` hoặc command docs, phần mô tả phải ưu tiên tiếng Việt; không viết plan/report chính bằng tiếng Anh.
- Comment trong code, XML doc comment, SQL comment và database object comment phải viết bằng tiếng Việt, trừ keyword/tên kỹ thuật/tên tham số bắt buộc phải giữ nguyên.
- XML doc cho public type/member phải đủ nghĩa:
  - `summary` nói rõ type/hàm làm gì trong nghiệp vụ/kỹ thuật.
  - `param` mô tả từng đầu vào dùng để làm gì; không để `param` rỗng.
  - `returns` bắt buộc khi hàm có giá trị trả về, mô tả đầu ra/ý nghĩa kết quả.
  - Không viết comment chung chung, không lặp lại y nguyên tên hàm/biến, không dùng comment tiếng Anh kiểu `Represents...` nếu không bắt buộc.
- Khi chạm vào comment tiếng Anh cũ trong phạm vi task, phải chuyển sang tiếng Việt nếu sửa cùng đoạn đó.

## Source Of Truth

- Báo cáo sản phẩm/kiến trúc chính: `clinic_saas_report.md`
- Ghi chú kiến trúc kỹ thuật: `architech.txt`
- Handoff dashboard hiện tại: `docs/current-task.md`
- Backend/DevOps task lane: `docs/current-task.backend.md`
- Frontend task lane: `docs/current-task.frontend.md`
- Backend/DevOps plan lane: `temp/plan.backend.md`
- Frontend plan lane: `temp/plan.frontend.md`
- Playbook agent: `docs/agent-playbook.md`
- Setup riêng cho Codex: `docs/codex-setup.md`
- Hướng dẫn chống mất session: `docs/session-continuity.md`
- Ghi chú cho owner khi project đã có code: `docs/owner-code-notes.md`
- UI source of truth: Figma/FigJam do owner cung cấp
- UI Source of Truth Phase 4+: Figma page `65:2` UI Redesign V3 - 2026-05-10 (file key `1nbJ5XkrlDgQ3BmzpXzhCC`), 76 frame, 7 section grouping. Chi tiết V3 + 5 Wave plan + 5 owner decision risk: `docs/roadmap/clinic-saas-roadmap.md#71-ui-redesign-v3-source-of-truth-phase-4` và `docs/agents/figma-ui-agent.md`.
- V1 (Figma page `0:1` Clinic Website UI Kit, 40 frame): functional baseline historical, không sửa, không xóa.
- V2 (Figma page `36:2`, empty): historical/partial baseline, không sửa, không xóa.
- PDF export đã đọc từ FigJam:
  - `Clinic SaaS Architecture - Source of Truth.pdf`
  - `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`
- Figma Design UI:
  - `https://www.figma.com/design/1nbJ5XkrlDgQ3BmzpXzhCC/Clinic-Website-UI-Kit---Client---Admin?t=L0tWxOID86LOXPh0-0`
- System architecture source of truth:
  - `https://www.figma.com/board/zVIS2cgoNqwC21lZbpApjp/Clinic-SaaS-Architecture---Source-of-Truth?t=L0tWxOID86LOXPh0-0`
  - `https://www.figma.com/board/j4vDRWSIRSckcAYvXHocMc/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture?t=L0tWxOID86LOXPh0-0`

## Nhận Diện Dự Án Hiện Tại

- Tên dự án: Clinic SaaS / Hospital Management Platform.
- Repo hiện là scaffold cho nền tảng SaaS quản lý phòng khám/bệnh viện.
- Backend/frontend chưa phải implementation hoàn chỉnh; khi gặp tài liệu/script chưa khớp Clinic SaaS thì phải cập nhật theo hướng Clinic SaaS, không chạy theo giả định cũ.
- Thông tin server/database thật chưa được owner chốt. Chỉ dùng placeholder cho đến khi owner cung cấp.

## Structure Project Hiện Tại

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

- Không dùng root-level `apps/`, `packages/`, hoặc `services/` cho source mới.
- Frontend apps nằm trong `frontend/apps/`.
- Shared frontend packages nằm trong `frontend/packages/`.
- Backend microservices nằm trong `backend/services/`.
- Backend shared building blocks/contracts/observability nằm trong `backend/shared/`.

## Quy Trình Làm Việc

1. Đọc source of truth.
2. Kiểm tra `git status` trước khi sửa để tránh ghi đè thay đổi của người khác.
3. Với task code, lập kế hoạch trong plan lane phù hợp và chờ duyệt.
4. Sửa đúng phạm vi task, không reorganize repo rộng nếu chưa được duyệt.
5. Sau khi sửa, chạy kiểm tra phù hợp với mức độ thay đổi.
6. Khi implement UI Phase 4+, đọc Figma V3 frame đúng surface trước khi code. Không tự invent layout khi V3 đã có frame detailed visual. Frame wireframe annotated chỉ là placeholder Phase 6-7, cần upgrade detailed trước khi code wave đó.
7. Cuối mỗi lượt làm việc, trả lời owner bằng báo cáo ngắn gồm:
   - đã hoàn thành gì,
   - file đã sửa/tạo,
   - đã kiểm tra bằng lệnh gì hoặc vì sao chưa kiểm tra,
   - việc còn thiếu/bị chặn,
   - bước tiếp theo đề xuất.
8. Với task nhiều wave hoặc sửa nhiều file, cập nhật "In-progress Checkpoint" trong current-task lane phù hợp sau từng wave nhỏ. Checkpoint phải ghi scope đang làm, file đã sửa/tạo, lệnh đã chạy/chưa chạy, bước resume tiếp theo và guardrail không revert/không commit.
9. Nếu không hoàn tất, cập nhật current-task lane phù hợp gồm:
   - đã hoàn thành gì,
   - đang bị chặn ở đâu,
   - bước tiếp theo cụ thể,
   - file đã thay đổi.

## QA Screenshots and Artifact Cleanup

- Đây là workflow bắt buộc cho QA/Lead khi chạy UI visual smoke/browser test/Figma compare; không phải chỉ là "skill" được cài sẵn rồi tự chạy.
- Codex phải áp dụng workflow này qua `AGENTS.md`, `docs/agents/qa-agent.md` và `docs/agents/lead-agent.md`.
- Claude Code phải áp dụng workflow này qua `CLAUDE.md` và `.claude/agents/qa-agent.md`, `.claude/agents/lead-agent.md`.

- Visual QA Budget: QA chỉ chụp screenshot khi owner yêu cầu visual QA, trước commit UI lớn, có lỗi visual cần chứng minh trước/sau, hoặc task là restyle/layout lớn. Không chụp screenshot cho mọi route nhỏ.
- Screenshot giới hạn mặc định: tối đa 1 desktop chính + 1 mobile chính, thêm tối đa 2 ảnh nếu route thật sự quan trọng. Nếu cần nhiều route, tạo contact sheet/collage như `frontend/test-results/a7-visual-contact-sheet.png` thay vì nhiều ảnh rời.
- Route sampling cho visual QA lớn: chọn route đại diện gồm dashboard, list/table chính, form/wizard chính, detail/modal chính nếu task có modal/detail; không chụp cả trước/sau cho mọi route trừ khi owner yêu cầu.
- Figma UI Agent chỉ chạy khi owner yêu cầu Figma, task là visual restyle lớn, screenshot cho thấy UI lệch, hoặc chuẩn bị commit UI lớn. Không gọi Figma/screenshot cho mọi task nhỏ.
- Screenshot lưu vào generated artifact folder, ưu tiên `frontend/test-results/`, đặt tên rõ route/task/state như `owner-dashboard-smoke.png`, `owner-clinics-smoke.png`, `owner-clinics-drawer-smoke.png`, `owner-clinics-empty-smoke.png`.
- QA report phải ghi route/state đã check, screenshot path, component/UI state đã test, pass/fail và visual issue nếu có.
- Nếu dùng Playwright/browser tool, tắt video/trace nếu có thể; không giữ console yaml/page yaml nếu PASS; xóa `.playwright-mcp` artifacts nếu không cần.
- Screenshot, log, yaml và generated artifacts chỉ là artifact review; không stage/commit trừ khi owner yêu cầu rõ.
- Lead Agent chịu trách nhiệm cleanup generated artifacts sau khi task/test/review hoàn tất để worktree không bẩn.
- Cleanup chỉ được xóa untracked/generated artifacts; không xóa source code, docs/handoff/plan dirty, tracked files hoặc file owner chưa xác nhận.
- Trước và sau cleanup phải chạy `git status --short`. Nếu nghi ngờ path có tracked file, kiểm tra `git ls-files --error-unmatch <path>`; nếu tracked thì không xóa.
- Artifact mặc định có thể cleanup khi untracked: `frontend/test-results/`, `frontend/playwright-report/`, `frontend/blob-report/`, `test-results/`, `playwright-report/`, `.playwright-mcp/`, `temp/*-vite.log`, `.last-run.json`, `frontend/.last-run.json`.
- Không commit screenshot/log/generated artifacts; không stage/commit/push trong cleanup step.

## Luật Kiến Trúc Bắt Buộc

- Multi-tenant là bắt buộc.
- Mọi dữ liệu thuộc tenant phải có tenant context.
- Clinic Admin chỉ được thao tác trong tenant của họ.
- Owner Super Admin mới được thao tác cross-tenant.
- Không query tenant-owned data nếu thiếu `tenant_id` hoặc tenant context hợp lệ.
- Public Website resolve tenant qua domain/subdomain.
- Frontend target: Vue 3, Vite, TypeScript, Pinia, Vue Router, shared UI package, design tokens từ Figma.
- Backend target: .NET service architecture, Clean Architecture theo từng service.
- Data strategy:
  - PostgreSQL cho dữ liệu quan hệ/giao dịch.
  - MongoDB cho CMS/page/template/layout JSON.
  - Redis cho cache, tenant/domain mapping, rate limit, temporary lock.
  - Kafka/Event Bus cho async platform events khi cần scale.
  - SignalR/WebSocket dự kiến dùng cho realtime.

## Coding Discipline Từ Tài Liệu Cũ

- Think before coding: nêu assumption, điểm mơ hồ và tradeoff trước khi implement task lớn. Nếu có nhiều cách hiểu, trình bày ra; không tự chọn im lặng. Nếu không rõ, dừng lại và hỏi.
- Simplicity first: code tối thiểu để giải quyết đúng vấn đề, không thêm feature ngoài scope, không thêm abstraction/configurability khi chưa cần.
- Surgical changes: chỉ sửa dòng/file liên quan trực tiếp tới yêu cầu. Mỗi dòng thay đổi phải trace được về request của owner.
- Goal-driven execution: với task nhiều bước, nêu success criteria và cách verify cho từng bước trước khi làm hoặc trong plan lane phù hợp.
- Đọc code hiện có trước khi viết mới, theo style đang có nếu đã tồn tại implementation.
- Không tạo abstraction khi mới có một use case.
- Không refactor, format hoặc dọn file không liên quan.
- Nếu thay đổi của mình tạo import/variable/function unused, phải dọn phần unused do chính mình tạo.
- Nếu thấy vấn đề ngoài scope, ghi chú lại thay vì tự sửa.
- Với dữ liệu external/API payload, check null trước khi dùng property.
- Security/tenant context thiếu hoặc invalid thì fail rõ ràng, không fallback hardcoded.
- Background job dùng queue/channel/background service; không tạo fire-and-forget task từ request handler nếu chưa có thiết kế.
- Command có nhiều DB operations cần transaction boundary rõ ràng.
- Không catch-all exception nếu middleware/global handler đã xử lý; chỉ catch khi có cleanup/persist trạng thái cụ thể.

## Phân Vai Agent

Codex và Claude Code dùng cùng khung phân vai sau:

- Architect Agent: kiểm tra service boundary, data ownership, tenant isolation và FigJam alignment.
- Lead / Orchestrator Agent: điều phối phase/task, tạo plan, chia việc, gọi subagent/parallel agent khi cần, tổng hợp verify và handoff.
- Web Research Agent: research UI/UX inspiration bằng web search/browser/MCP nếu môi trường có, tổng hợp direction, không sửa Figma/code.
- Figma UI Agent: cải tổ Figma UI/design system, mapping UI với product/backend phase, chuẩn bị handoff frontend.
- Frontend Agent: xây `public-web`, `clinic-admin`, `owner-admin`, routing, API client, tenant context.
- Backend Agent: xây .NET services theo Clean Architecture.
- Database Agent: thiết kế PostgreSQL schema, MongoDB collections, indexes, migrations, seed data.
- DevOps Agent: chuẩn bị Docker Compose, env structure, CI/CD, deployment, domain/SSL flow.
- QA Agent: verify tenant isolation, auth permissions, booking, template apply, domain verification.
- Documentation Agent: giữ README, architecture docs, setup, deployment, troubleshooting luôn cập nhật.

Khi owner gọi "Lead Agent", "Lead / Orchestrator Agent", "lead-plan", "giao việc", "điều phối", "làm việc này", "làm tiếp", "chạy workflow" hoặc yêu cầu tương đương, đó là quyền rõ ràng cho Codex tự điều phối theo team agent trong phạm vi task. Nếu tool hỗ trợ subagent/parallel agent, Lead Agent được phép tự tạo, giao việc, chờ kết quả và tổng hợp các subagent phù hợp mà không cần hỏi lại từng lần.

Nếu prompt của owner có động từ hành động như `chạy`, `làm`, `implement`, `verify`, `review`, `chia commit`, `fix`, `tiếp tục`, `hoàn tất`, Codex phải thực hiện action thật trong cùng lượt. Nếu không thể thực hiện, Codex phải nêu blocker cụ thể theo file/command/tool/lỗi môi trường; không được dừng bằng guardrail chung chung hoặc câu acknowledge-only.

Các prompt ngắn như `Lead Agent: bắt đầu A5.2`, `Lead Agent: làm tiếp A5.4`, `Lead Agent: verify A5.2`, `Lead Agent: chia commit A5.1b` cũng là trigger điều phối feature-team. Lead Agent phải tự phân loại lane, tự chọn agents, tự đọc lane plan/handoff, rồi report agents đã chọn và phần việc từng vai; owner không cần paste danh sách "Agents tham gia".

Lead Agent dùng cùng logic nền với `lead-plan`:

- Nếu owner yêu cầu plan/lead-plan: tạo hoặc cập nhật plan, không implement code.
- Nếu owner yêu cầu implement/action/code nhưng chưa có plan được duyệt: tự chạy lead-plan trước rồi dừng chờ duyệt, trừ khi owner nói rõ làm ngay.
- Nếu owner nói làm tiếp trong task đã có plan/approval rõ: tiếp tục theo plan và guardrail hiện tại.
- Nếu task chỉ là docs/config/agent workflow theo yêu cầu trực tiếp: được sửa nhỏ ngay, không cần chờ plan.

Lead Agent vẫn phải giữ các guardrail bắt buộc: không commit/push nếu owner chưa yêu cầu, không ghi secret/IP/private key vào repo, không tạo Figma file nếu chưa được yêu cầu, không chuyển phase sang Done khi verify bắt buộc chưa pass, và không sửa ngoài scope.

Chi tiết agent nằm trong:

- `agents/*.md` là entrypoint workflow ngắn cho các task UI/research cần gọi nhanh.
- `docs/agents/*.md` là nguồn chi tiết chính cho Codex.
- `docs/agent-playbook.md` là index/tóm tắt vai trò cho Codex và mọi agent.
- `.claude/agents/*.md` cho Claude Code.

## Codex Support

Codex không dùng trực tiếp format `.claude/agents/*.md`. Cấu trúc chuẩn cho Codex trong repo này là:

- `AGENTS.md`: luật bắt buộc và project instructions, Codex phải đọc đầu tiên.
- `docs/agent-playbook.md`: index/tóm tắt vai trò Lead/Architect/Web Research/Figma UI/Frontend/Backend/Database/DevOps/QA/Documentation cho Codex.
- `agents/figma-ui-agent.md` và `agents/web-research-agent.md`: workflow UI/research root-level, chứa source of truth, Figma links, UI redesign rules và MCP/search-browser rules để không cần prompt dài.
- `docs/agents/*.md`: prompt/checklist chi tiết từng agent cho Codex, là nguồn đọc chính khi owner gọi vai agent.
- `docs/codex-setup.md`: cách cấu hình MCP, Figma, GitNexus và quy trình chạy Codex.
- `rules/*.md`: luật code/test/database theo từng ngữ cảnh. Đây là rule tài liệu cho agent đọc và tuân thủ, không phải linter tự động.
- `docs/current-task.md`: dashboard điều phối hiện tại.
- `docs/current-task.backend.md`, `docs/current-task.frontend.md`, `temp/plan.backend.md`, `temp/plan.frontend.md`: lane task/plan chi tiết khi backend/devops và frontend chạy song song.

Khi owner gọi Lead Agent hoặc giao việc theo kiểu điều phối, Codex được phép tự tách việc theo `docs/agents/*.md` và tự dùng subagent/parallel agent nếu tool hỗ trợ. Với task UI/research, đọc thêm `agents/figma-ui-agent.md` và `agents/web-research-agent.md` trước khi hỏi lại owner. Với task không gọi vai Lead/agent và không cần song song, Codex có thể tự làm theo checklist agent mà không spawn subagent.

## UI Research + Figma Workflow

Khi owner yêu cầu redesign/tối ưu/cải tổ UI:

1. Lead Agent đọc `agents/web-research-agent.md`, `agents/figma-ui-agent.md`, rồi đối chiếu thêm `docs/agents/lead-agent.md`, `docs/agents/web-research-agent.md` và `docs/agents/figma-ui-agent.md` nếu cần.
2. Lead Agent gọi Web Research Agent trước nếu cần inspiration/benchmark.
3. Web Research Agent dùng web search/browser/MCP nếu môi trường có; nếu không có thì báo rõ và không tự bịa research web.
4. Lead Agent tổng hợp direction.
5. Nếu owner yêu cầu làm thẳng hoặc direction đã đủ rõ, Lead Agent gọi Figma UI Agent.
6. Figma UI Agent update trực tiếp Figma source of truth, không tạo Figma file mới.
7. Nếu task chỉ là UI/Figma, không sửa backend/frontend code.
8. Sau khi sửa Figma phải report frame đã sửa/thêm.

## Figma / MCP

- Codex global Figma MCP đã cấu hình remote URL: `https://mcp.figma.com/mcp`.
- Codex global Playwright MCP đã enabled bằng `npx -y @playwright/mcp@latest` để hỗ trợ browser/research workflow khi phiên Codex mới load MCP.
- Search MCP cần API key như Brave/Perplexity chỉ được cấu hình khi owner cung cấp key rõ; không ghi API key giả hoặc secret vào repo/global config.
- `.mcp.json` trong repo dùng cho Claude-style project MCP config.
- Nếu Figma MCP tools không hiện trong phiên làm việc, restart Codex/Claude Code để MCP config được load lại.
- Nếu không đọc được Figma trong phiên hiện tại, phải ghi rõ giới hạn vào `docs/current-task.md` và không tự bịa nội dung board.
- Ngày 2026-05-08 đã chạy `codex mcp add figma --url https://mcp.figma.com/mcp` và OAuth thành công.
- Ngày 2026-05-08, sau khi owner đổi tài khoản/link Figma, phiên Codex hiện tại đã đọc được metadata UI Design và cả hai FigJam board mới bằng Figma MCP.
- Hai PDF export trong repo vẫn là snapshot tham chiếu, nhưng khi làm UI/architecture phải ưu tiên link Figma/FigJam mới nếu MCP đọc được.

## GitNexus

Nếu GitNexus tools có trong phiên agent:

- Chạy `npx gitnexus analyze` sau khi repo structure đã ổn định.
- Chạy impact analysis trước khi sửa code symbol thật.
- Chạy change detection trước khi commit.

Nếu GitNexus tools không có, ghi rõ giới hạn và tiếp tục review theo file-level cho task tài liệu/config.
