# Owner Code Notes - Khi Project Đã Có Code Thật

File này dành cho owner đọc lại sau này khi repo đã bắt đầu có code thật.

Mục tiêu: biết khi nào cần cập nhật plan, handoff, rule, và nên bổ sung rule vào file nào.

## 1. Trước Khi Bảo Agent Code

Nên yêu cầu agent đọc:

- `AGENTS.md`
- `clinic_saas_report.md`
- `architech.txt`
- `docs/current-task.md`
- `docs/session-continuity.md`
- `rules/*.md`

Prompt gợi ý:

```txt
Đọc AGENTS.md, clinic_saas_report.md, docs/current-task.md, docs/session-continuity.md và rules/*.md.
Tạo/cập nhật temp/plan.md cho task này trước, chưa code.
Plan phải có scope, assumptions, file dự kiến sửa, success criteria, verification steps.
```

## 2. Khi Nào Cập Nhật File Nào

### `temp/plan.md`

Dùng cho task cụ thể sắp code.

Cập nhật khi:

- chuẩn bị implement feature/bugfix/refactor,
- cần owner duyệt trước khi code,
- task có nhiều bước hoặc ảnh hưởng kiến trúc.

Không dùng `temp/plan.md` làm roadmap dài hạn.

### `plan.md`

Dùng cho roadmap/kế hoạch tổng của repo.

Cập nhật khi:

- đổi phase triển khai,
- đổi thứ tự milestone,
- thêm/bớt module lớn,
- quyết định kiến trúc làm thay đổi hướng đi,
- sau một task code lớn làm roadmap cũ không còn đúng.

Không cần cập nhật `plan.md` cho fix nhỏ.

### `docs/current-task.md`

Dùng cho handoff giữa các session/agent.

Cập nhật sau mỗi lượt làm việc nếu:

- có file đã sửa,
- có blocker,
- chưa hoàn tất task,
- có bước tiếp theo cụ thể,
- session có thể bị mất/hết hạn.
- task dài hơn 30 phút hoặc sửa/tạo từ 5 file trở lên.

Nội dung nên có:

- đã làm gì,
- file đã sửa/tạo,
- kiểm tra đã chạy,
- việc còn thiếu,
- bước tiếp theo.

Với task đang dở, yêu cầu agent ghi thêm checkpoint:

```txt
Ghi In-progress Checkpoint vào lane current-task phù hợp: scope đang làm, file đã sửa/tạo, lệnh đã chạy/chưa chạy, bước resume tiếp theo. Không đánh dấu Done nếu chưa verify.
```

Prompt resume khi session chết:

```txt
Session trước chết giữa lúc implement. Resume từ git status + git diff, không revert.
Đọc docs/session-continuity.md và lane current-task liên quan, tóm tắt diff hiện tại, chạy verify phù hợp rồi tiếp tục đúng scope.
```

## 3. Bổ Sung Rule Vào Đâu

Không nhét tất cả vào một file. Chọn đúng file theo loại rule.

### Rule chung cho mọi agent

Ghi vào:

- `AGENTS.md`
- nếu Claude cũng cần biết: `CLAUDE.md`
- nếu là workflow agent: `docs/agent-playbook.md`

Ví dụ:

- phải report sau mỗi lượt,
- không code khi chưa có plan,
- không xóa file nếu chưa hỏi,
- cách dùng subagent.

### Rule về code style / clean architecture chung

Ghi vào:

- `rules/coding-rules.md`

Ví dụ:

- simplicity first,
- surgical changes,
- goal-driven execution,
- không thêm abstraction không cần thiết,
- mỗi dòng thay đổi phải trace về request.

### Rule backend

Ghi vào:

- `rules/backend-coding-rules.md`

Ví dụ:

- Clean Architecture layer boundary,
- validation pipeline,
- error contract,
- repository pattern,
- transaction boundary,
- tenant isolation,
- config fail-fast,
- logging/correlation id,
- không expose stack trace.

### Rule test API/backend

Ghi vào:

- `rules/backend-testing-rules.md`

Ví dụ:

- health-check trước khi test,
- ghi port/environment/mode,
- payload tiếng Việt dùng JSON UTF-8,
- test tenant allowed/forbidden,
- validation error phải 4xx.

### Rule Codex setup

Ghi vào:

- `docs/codex-setup.md`

Ví dụ:

- MCP/Figma setup,
- Codex đọc file nào,
- Codex dùng agent role ở đâu,
- lỗi/quota/tooling của Codex.

### Rule Claude setup

Ghi vào:

- `CLAUDE.md`
- `.claude/commands/*.md`
- `.claude/agents/*.md`

Ví dụ:

- slash command workflow,
- Claude agent role,
- Claude-specific MCP/hook/permission.

## 4. Khi Nào Không Nên Sửa Rules

Không nên sửa `rules/*.md` nếu:

- chỉ là bug một lần,
- chưa chắc có lặp lại,
- chỉ là sở thích style cá nhân,
- chưa được owner chốt,
- task nhỏ không tạo convention mới.

Thay vào đó, report:

```txt
Tôi thấy có pattern có thể thành rule: ...
Đề xuất thêm vào rules/backend-coding-rules.md, chờ owner duyệt.
```

## 5. Sau Khi Code Xong Nên Bảo Agent Làm Gì

Prompt gợi ý:

```txt
Sau khi code xong:
- cập nhật docs/current-task.md,
- report file đã sửa,
- report lệnh kiểm tra đã chạy,
- report phần còn thiếu/bị chặn,
- nếu roadmap thay đổi thì cập nhật plan.md,
- nếu phát hiện rule mới thì đề xuất bổ sung vào file rule phù hợp, chưa tự sửa rules nếu chưa hỏi.
```

## 6. Checklist Cho Owner Khi Review Agent

- Agent đã đọc đúng source of truth chưa?
- Có `temp/plan.md` trước khi code chưa?
- Plan có success criteria và verification steps chưa?
- Agent có sửa ngoài scope không?
- Có cập nhật `docs/current-task.md` sau khi làm không?
- Có report lại rõ ràng không?
- Nếu sửa rule, rule đó có đúng file không?
- Nếu có code tenant-owned data, có tenant isolation không?
- Nếu có API/backend, có validation/error/logging/test phù hợp không?
