# Session Continuity Guide

File nay dung de chong mat ngu canh khi session Codex/Claude bi compact, reset hoac het han. Day la rule resume/checkpoint, khong phai noi chua active task data.

## Reading Policy Khi Resume

Doc theo lane/scope trong `AGENTS.md`, khong doc full repo docs theo mac dinh.

- Frontend nho: `docs/current-task.md`, `docs/current-task.frontend.md`, `temp/plan.frontend.md`, `rules/coding-rules.md`, source lien quan.
- Backend nho: `docs/current-task.md`, `docs/current-task.backend.md`, `temp/plan.backend.md`, `rules/coding-rules.md`, `rules/backend-coding-rules.md`; them DB/testing rules neu scope can.
- Docs/workflow/Lead/cross-lane: `docs/agent-playbook.md`, file docs lien quan.
- Archive chi doc khi owner yeu cau ro, active file tro toi section archive cu the, hoac can bang chung debug cu.

## Server Test Runtime Rule

Khi resume task backend/FE integration, agent kiem tra shell/session hoac `deploy.local.ps1` da ignore co runtime env do owner cung cap hay khong, vi du `DEPLOY_HOST`, `DEPLOY_USER`, `SSH_KEY_PATH`.

Neu shell chua co env va `deploy.local.ps1` ton tai o repo root, agent co the nap file local truoc khi preflight:

```powershell
. .\deploy.local.ps1
```

Neu env co san va task yeu cau backend/DB/API smoke that:
- server test/dev smoke la runtime chinh cho PostgreSQL, Tenant Service, API Gateway va API integration smoke;
- thieu Docker/.NET local tren Windows khong tu dong la blocker;
- FE real API smoke tro toi API Gateway that tren server test hoac qua tunnel neu can.

Checkpoint/handoff chi ghi:
- runtime smoke dung server test hay local;
- gateway/tunnel dang dung o muc mo ta, khong ghi IP/server path/private key;
- API smoke PASS/FAIL/blocker;
- bien/session nao bi thieu neu bi block.

Khong ghi private key, token, secret, IP server that, SSH key path that, connection string that vao repo/docs/log.

Stub chi la fallback cuoi cung de kiem contract path khi server test khong truy cap duoc; khong dung stub de danh dau E2E Done neu server test co the chay API that.

## Phan Biet File Plan / Handoff / Roadmap

- `docs/current-task.md`: dashboard tong cross-lane ngan, chi ghi state tong va link sang lane.
- `docs/current-task.frontend.md`: handoff ngan cho frontend lane.
- `docs/current-task.backend.md`: handoff ngan cho Backend/DevOps lane.
- `temp/plan.frontend.md`: living active plan frontend, du de resume task dang lam.
- `temp/plan.backend.md`: living active plan Backend/DevOps, du de resume task dang lam.
- `temp/plan.md`: index tuong thich cu, khong chua plan chi tiet cua lane.
- `docs/roadmap/clinic-saas-roadmap.md`: phase/wave tracker dai han; danh Done/In Progress/Blocked cho phase lon, khong thay the active plan.
- `docs/archive/**`, `temp/archive/**`: history lanh, khong doc mac dinh.

Active plan được phép dài vừa phải. Nó phải có `Current Active Slice`, điểm dừng gần nhất, progress checklist, next decision, likely files, acceptance criteria, verify plan, blockers và archive index. Nếu chưa có slice implement được duyệt, ghi rõ `No active implementation slice approved`; agent không được tự code từ một plan mơ hồ. Không append full history/log vào active plan.

## Crash Recovery & Checkpoint Protocol

Muc tieu: session moi resume duoc tu worktree that ma khong can nho chat cu.

### Khi Nao Ghi Checkpoint

Ghi checkpoint ngan vao lane current-task phu hop khi:

- Task du kien keo dai hon 30 phut.
- Task sua/tao tu 5 file tro len.
- Vua hoan tat mot wave nho trong task lon.
- Truoc khi chay verify/build/test dai hoac truoc buoc rui ro.
- Tool loi, session co nguy co mat context, hoac can dung giua chung.

Lane ghi checkpoint:

- Frontend: `docs/current-task.frontend.md`
- Backend/DevOps: `docs/current-task.backend.md`
- DevOps rieng: `temp/plan.devops.md` neu lane nay dang duoc dung
- Database rieng: `temp/plan.database.md` neu co lane rieng
- Cross-lane/Lead: `docs/current-task.md` chi ghi dashboard ngan va tro sang lane file

### Mau Checkpoint

```txt
## In-progress Checkpoint - YYYY-MM-DD HH:mm

Scope dang lam:
- ...

Da hoan thanh:
- ...

File da sua/tao:
- ...

Chua verify / con thieu:
- ...

Lenh da chay:
- ...

Lenh can chay tiep:
- ...

Buoc resume tiep theo:
1. Chay git status --short.
2. Chay git diff --stat.
3. Doc diff cac file trong scope.
4. Tiep tuc tu ...

Guardrail:
- Khong revert thay doi chua ro chu so huu.
- Khong commit/push neu owner chua yeu cau.
```

Checkpoint khong thay the report cuoi va khong duoc dung de danh dau Done. Neu verify chua chay thi ghi ro "chua verify".

## Quy Trinh Resume Sau Khi Session Chet

Session moi uu tien trang thai repo that:

```powershell
git status --short
git diff --stat
git diff --check
```

Sau do doc lane current-task/active plan gan nhat va diff file trong scope.

Nguyen tac resume:

- Khong revert thay doi dang do neu chua ro la cua ai.
- Khong tu code tiep chi dua vao doan chat cu; phai doi chieu `git diff` va checkpoint.
- Neu diff co file ngoai scope, bao owner va bo qua hoac hoi neu no chan task.
- Neu checkpoint noi da verify nhung repo hien tai khac diff, chay verify lai.
- Neu khong co checkpoint, tao recovery summary tu `git status` + `git diff --stat` truoc khi lam tiep.

## Khi Owner Yeu Cau Code That

Truoc khi code:

1. Doc source of truth va rules theo lane.
2. Tao/cap nhat active plan lane phu hop (`temp/plan.frontend.md` hoac `temp/plan.backend.md`) neu task chua co plan duyet.
3. Plan phai co scope, assumptions, file du kien sua/tao, success criteria, verification steps, tenant isolation impact, va Figma/FigJam reference neu lien quan UI/architecture.
4. Cho owner duyet, tru khi owner noi ro "lam luon/da duyet/bat dau implement/tiep tuc approved scope".

Sau khi owner duyet:

1. Implement dung approved scope.
2. Khong refactor ngoai scope.
3. Don unused code do chinh thay doi tao ra.
4. Verify theo plan.
5. Ghi checkpoint giua chung neu task dai/sua nhieu file.
6. Cap nhat lane current-task va active plan bang summary ngan.
7. Neu phase/wave lon doi trang thai, cap nhat roadmap.
8. Report lai cho owner: da lam gi, file nao, verify gi, con thieu/blocker gi, buoc tiep theo.

## Khi Nao Cap Nhat Roadmap

Cap nhat `docs/roadmap/clinic-saas-roadmap.md` khi:

- roadmap thay doi;
- them/bot milestone;
- chuyen phase/wave;
- owner doi uu tien;
- implementation lam thay doi huong trien khai tong.

Khong cap nhat roadmap cho thay doi nho khong anh huong phase/wave.

## Khi Nao Cap Nhat Rules

Khong tu sua `rules/*.md` lien tuc trong luc code.

Chi cap nhat rules khi:

- phat hien bug pattern co the lap lai;
- owner chot convention moi;
- co quyet dinh kien truc moi;
- tool/test/deploy workflow thay doi;
- rule cu sai hoac thieu so voi thuc te project.

Neu khong chac, de xuat rule moi trong report truoc, cho owner duyet roi moi sua.
