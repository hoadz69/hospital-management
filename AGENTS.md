# AGENTS.md - Clinic SaaS / Hospital Management

File này là hướng dẫn chung cho Codex, Claude Code và các coding agent khác khi làm việc trong repo `hospital-management`.

## ⚠️ BẮT BUỘC ĐỌC TRƯỚC KHI VIẾT CODE

1. Đọc file này trước mọi thay đổi.
2. Đọc `clinic_saas_report.md`, `architech.txt` và `docs/current-task.md` trước khi đổi hướng kỹ thuật.
3. Nếu là task implementation, phải tạo/cập nhật `temp/plan.md` trước và chờ owner duyệt, trừ khi owner nói rõ là làm ngay.
4. Khi owner chỉ hỏi/phân tích, không được tự code. Chỉ đọc file liên quan, phân tích và trả lời.
5. Nếu chỉ dọn tài liệu/config theo yêu cầu trực tiếp của owner, được sửa ngay nhưng phải giữ scope nhỏ và ghi rõ file đã sửa.
6. Không commit nếu owner chưa yêu cầu.
7. Không xóa file nếu owner chưa yêu cầu, trừ file mới do chính task hiện tại tạo ra.
8. Không đưa secret, IP server thật, chuỗi kết nối database thật, token, SSH key vào repo.
9. Sau mỗi lần làm xong phải report lại cho owner: đã làm gì, sửa file nào, kiểm tra gì, còn thiếu/bị chặn gì, bước tiếp theo là gì. Không được im lặng sau khi chạy tool hoặc sửa file.

## Source Of Truth

- Báo cáo sản phẩm/kiến trúc chính: `clinic_saas_report.md`
- Ghi chú kiến trúc kỹ thuật: `architech.txt`
- Handoff task hiện tại: `docs/current-task.md`
- Playbook agent: `docs/agent-playbook.md`
- Setup riêng cho Codex: `docs/codex-setup.md`
- Hướng dẫn chống mất session: `docs/session-continuity.md`
- Ghi chú cho owner khi project đã có code: `docs/owner-code-notes.md`
- UI source of truth: Figma/FigJam do owner cung cấp
- PDF export đã đọc từ FigJam:
  - `Clinic SaaS Architecture - Source of Truth.pdf`
  - `Clinic SaaS Technical Architecture - Microservices Clean Architecture.pdf`
- Figma Design UI từ tài liệu cũ:
  - `https://www.figma.com/design/Mz2oB5doTl6alla1KwTUWL`
- System architecture source of truth:
  - `https://www.figma.com/board/Cw0evT4maoKnQX5G23tJpT/Clinic-SaaS-Architecture---Source-of-Truth`
  - `https://www.figma.com/board/Fwpls2wzNxzGdpDuGGYSxi/Clinic-SaaS-Technical-Architecture---Microservices-Clean-Architecture`

## Nhận Diện Dự Án Hiện Tại

- Tên dự án: Clinic SaaS / Hospital Management Platform.
- Repo hiện là scaffold cho nền tảng SaaS quản lý phòng khám/bệnh viện.
- Backend/frontend chưa phải implementation hoàn chỉnh; khi gặp tài liệu/script chưa khớp Clinic SaaS thì phải cập nhật theo hướng Clinic SaaS, không chạy theo giả định cũ.
- Thông tin server/database thật chưa được owner chốt. Chỉ dùng placeholder cho đến khi owner cung cấp.

## Quy Trình Làm Việc

1. Đọc source of truth.
2. Kiểm tra `git status` trước khi sửa để tránh ghi đè thay đổi của người khác.
3. Với task code, lập kế hoạch trong `temp/plan.md` và chờ duyệt.
4. Sửa đúng phạm vi task, không reorganize repo rộng nếu chưa được duyệt.
5. Sau khi sửa, chạy kiểm tra phù hợp với mức độ thay đổi.
6. Cuối mỗi lượt làm việc, trả lời owner bằng báo cáo ngắn gồm:
   - đã hoàn thành gì,
   - file đã sửa/tạo,
   - đã kiểm tra bằng lệnh gì hoặc vì sao chưa kiểm tra,
   - việc còn thiếu/bị chặn,
   - bước tiếp theo đề xuất.
7. Nếu không hoàn tất, cập nhật `docs/current-task.md` gồm:
   - đã hoàn thành gì,
   - đang bị chặn ở đâu,
   - bước tiếp theo cụ thể,
   - file đã thay đổi.

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
- Goal-driven execution: với task nhiều bước, nêu success criteria và cách verify cho từng bước trước khi làm hoặc trong `temp/plan.md`.
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
- Figma UI Agent: chuyển Figma UI thành Vue components và design tokens.
- Frontend Agent: xây `public-web`, `clinic-admin`, `owner-admin`, routing, API client, tenant context.
- Backend Agent: xây .NET services theo Clean Architecture.
- Database Agent: thiết kế PostgreSQL schema, MongoDB collections, indexes, migrations, seed data.
- DevOps Agent: chuẩn bị Docker Compose, env structure, CI/CD, deployment, domain/SSL flow.
- QA Agent: verify tenant isolation, auth permissions, booking, template apply, domain verification.
- Documentation Agent: giữ README, architecture docs, setup, deployment, troubleshooting luôn cập nhật.

Nếu tool hỗ trợ subagent, chỉ dùng khi owner yêu cầu rõ việc chia agent/parallel agent. Nếu không, dùng phân vai này như checklist tư duy.

Chi tiết agent nằm trong:

- `docs/agent-playbook.md` cho Codex và mọi agent.
- `.claude/agents/*.md` cho Claude Code.

## Codex Support

Codex không dùng trực tiếp format `.claude/agents/*.md`. Cấu trúc chuẩn cho Codex trong repo này là:

- `AGENTS.md`: luật bắt buộc và project instructions, Codex phải đọc đầu tiên.
- `docs/agent-playbook.md`: mô tả vai trò Architect/Figma UI/Frontend/Backend/Database/DevOps/QA/Documentation để Codex dùng như checklist hoặc prompt nền.
- `docs/codex-setup.md`: cách cấu hình MCP, Figma, GitNexus và quy trình chạy Codex.
- `rules/*.md`: luật code/test theo từng ngữ cảnh.
- `docs/current-task.md`: handoff hiện tại.

Nếu cần tách việc theo agent trong Codex, chỉ dùng khi owner yêu cầu rõ. Khi đó dùng vai trò trong `docs/agent-playbook.md`; không tự tạo workflow song song ngoài yêu cầu.

## Figma / MCP

- Codex global Figma MCP đã cấu hình remote URL: `https://mcp.figma.com/mcp`.
- `.mcp.json` trong repo dùng cho Claude-style project MCP config.
- Nếu Figma MCP tools không hiện trong phiên làm việc, restart Codex/Claude Code để MCP config được load lại.
- Nếu không đọc được Figma trong phiên hiện tại, phải ghi rõ giới hạn vào `docs/current-task.md` và không tự bịa nội dung board.
- Ngày 2026-05-08 đã chạy `codex mcp add figma --url https://mcp.figma.com/mcp` và OAuth thành công. Process Codex mới gọi được MCP nhưng Figma trả lỗi quota Starter plan, nên chưa trích xuất được board.
- Hai PDF export trong repo đã được đọc và dùng làm nguồn thay thế cho đến khi MCP quota có lại.

## GitNexus

Nếu GitNexus tools có trong phiên agent:

- Chạy `npx gitnexus analyze` sau khi repo structure đã ổn định.
- Chạy impact analysis trước khi sửa code symbol thật.
- Chạy change detection trước khi commit.

Nếu GitNexus tools không có, ghi rõ giới hạn và tiếp tục review theo file-level cho task tài liệu/config.
