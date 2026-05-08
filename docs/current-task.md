# Current Task - Cập Nhật Plan Phase 2 Tenant MVP Backend

Ngày cập nhật: 2026-05-09

## Trạng Thái

🟡 Plan Updated - Awaiting Owner Approval

## Yêu Cầu Của Owner

Cập nhật lại Phase 2 plan trước khi implement theo quyết định kiến trúc mới của owner.

## Phase Hiện Tại

Phase 2 - Tenant MVP Backend.

Phase 1.3 API Boundary Standardization đã Done và đã verify. Phase 2 implementation chưa bắt đầu.

## Plan Đã Cập Nhật

Plan hiện tại nằm ở:

- `temp/plan.md`

Các điểm đã cập nhật:

- Không dùng EF Core.
- Tenant Service persistence dùng Dapper + Npgsql.
- Migration dùng SQL migration files; khuyến nghị DbUp nếu cần runner, chưa implement runner nếu chưa cần.
- Runtime production-safe đề xuất là .NET 10 LTS; giữ net9.0 tạm nếu cần build local và chưa có task upgrade riêng.
- API Gateway Phase 2 dùng typed HttpClient forwarding; YARP để phase gateway hardening sau.
- TenantStatus chốt: Draft, Active, Suspended, Archived.
- TenantModule dùng string module_code; chưa implement billing/plan logic thật.

## Scope Triển Khai Sau Khi Owner Duyệt

- `backend/services/tenant-service`
- `backend/services/api-gateway`
- `backend/shared/contracts`
- `infrastructure/postgres`
- `docs/current-task.md`
- `docs/roadmap/clinic-saas-roadmap.md`

## Ngoài Scope Cho Tới Khi Có Duyệt Riêng

- Không implementation trước khi owner duyệt lại plan.
- Không frontend changes.
- Không real auth/JWT provider integration.
- Không billing implementation.
- Không domain verification hoặc SSL flow.
- Không template/CMS implementation.
- Không Figma file creation.
- Không commit.

## Cần Owner Duyệt

Chỉ được implement sau khi owner duyệt rõ bằng câu như:

- "Tôi duyệt plan"
- "Duyệt, làm tiếp"
- "Bắt đầu implement"
- "Quất theo plan"

## Bước Tiếp Theo

Owner review lại `temp/plan.md`. Nếu duyệt, implement Phase 2 đúng scope và đúng quyết định kiến trúc đã chốt.
