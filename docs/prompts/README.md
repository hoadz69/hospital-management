# Prompt Files - Agent Runner

`docs/prompts/` chứa prompt ngắn cho từng task trong `docs/agent-queue.md`. Runner đọc `prompt_file` rồi truyền nội dung vào `codex exec`.

## Quy Tắc Prompt

- Prompt phải ngắn và có `Mode`, `Read`, `Scope`, `Out of scope`, `Implement`, `Verify`, `Stop if`, `Report`.
- Không paste lại toàn bộ `AGENTS.md`, full log, full diff hoặc nội dung archive.
- Không commit/push/stage.
- Không gọi screenshot mặc định.
- Không gọi Figma nếu task không cần visual/Figma source.
- Prompt feature thật nên tách backend/frontend nếu contract đã rõ.
- Prompt backend phải nói rõ tenant/security/database impact khi chạm API hoặc persistence.
- Prompt frontend phải nói rõ route/component/API client trong scope và mock/real mode.

## Cách Runner Gọi

Runner gọi dạng stdin để tránh lỗi quoting trên PowerShell:

```powershell
Get-Content -Raw docs/prompts/<task>.md | codex exec -C <repo-root> --json -o temp/agent-runner/<task>.final.md -
```

Log JSONL nằm trong `temp/agent-runner/` và không được stage/commit.
Log này là artifact chạy task, có thể xóa sau khi đã đọc kết quả hoặc đã ghi summary vào docs/checkpoint.
