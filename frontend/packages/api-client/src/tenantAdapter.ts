// Adapter chuyển wire format backend (Phase 2) sang FE shape (TenantSummary, TenantDetail).
//
// Lý do tồn tại:
// - Backend trả detail dạng nested có `profile`, `domains[]`, `modules[]` cùng một số timestamp Utc.
// - Backend trả list dạng paged `{ items, total, limit, offset }` với từng item flat partial
//   (chỉ `id, slug, displayName, status, planCode, clinicName, createdAtUtc, updatedAtUtc`),
//   thiếu `planDisplayName`, `specialty`, `defaultDomainName`, `domainStatus`, `moduleCodes`,
//   `contactEmail` so với FE shape.
// - Một số enum (TenantDomainStatus) backend dùng PascalCase (`Pending`), FE shape dùng lowercase
//   (`pending|verified|failed|unknown`).
// - FE shape hiện tại flat (TenantSummary/TenantDetail) đã được mock client + UI component dùng
//   ổn định, nên Lead chốt fix ở FE side, không sửa backend.
//
// Adapter chỉ áp dụng ở real path trong `tenantClient.ts`. Mock client giữ nguyên contract FE shape.
import type {
  TenantDetail,
  TenantDomain,
  TenantDomainStatus,
  TenantModuleCode,
  TenantPlanCode,
  TenantStatus,
  TenantSummary
} from "@clinic-saas/shared-types";

/** Wire format domain mà backend trả trong detail/create/update response. */
export type BackendTenantDomain = {
  id?: string;
  domainName: string;
  normalizedDomainName?: string;
  domainType?: string;
  status: string;
  isPrimary?: boolean;
  createdAtUtc?: string;
  verifiedAtUtc?: string | null;
};

/** Wire format module mà backend trả trong detail/create/update response. */
export type BackendTenantModule = {
  moduleCode: TenantModuleCode;
  isEnabled?: boolean;
  sourcePlanCode?: TenantPlanCode;
  createdAtUtc?: string;
  updatedAtUtc?: string;
};

/** Wire format profile nested trong tenant detail. */
export type BackendTenantProfile = {
  clinicName?: string;
  contactEmail?: string;
  phoneNumber?: string;
  addressLine?: string;
  specialty?: string;
};

/** Wire format detail mà backend trả ở GET/POST/PATCH `/api/tenants/{id}`. */
export type BackendTenantDetail = {
  id: string;
  slug: string;
  displayName: string;
  status: TenantStatus;
  planCode: TenantPlanCode;
  planDisplayName?: string;
  profile?: BackendTenantProfile;
  domains?: BackendTenantDomain[];
  modules?: BackendTenantModule[];
  createdAtUtc?: string;
  updatedAtUtc?: string;
  activatedAtUtc?: string | null;
  suspendedAtUtc?: string | null;
  archivedAtUtc?: string | null;
};

/** Wire format từng item trong list endpoint (flat partial, không có nested objects). */
export type BackendTenantSummary = {
  id: string;
  slug: string;
  displayName: string;
  status: TenantStatus;
  planCode: TenantPlanCode;
  planDisplayName?: string;
  clinicName?: string;
  createdAtUtc?: string;
  updatedAtUtc?: string;
};

/** Envelope phân trang mà backend trả ở GET `/api/tenants`. */
export type BackendTenantListResponse = {
  items: BackendTenantSummary[];
  total: number;
  limit: number;
  offset: number;
};

/**
 * Chuẩn hoá enum domain status từ backend (PascalCase) về FE shape (lowercase).
 * @param raw Giá trị enum thô do backend trả, có thể `Pending`, `Verified`, `Failed`...
 * @returns Một trong bốn giá trị FE chấp nhận; fallback `"unknown"` khi không khớp.
 */
function normalizeDomainStatus(raw: string | undefined): TenantDomainStatus {
  switch ((raw ?? "").toLowerCase()) {
    case "verified":
      return "verified";
    case "pending":
      return "pending";
    case "failed":
      return "failed";
    default:
      return "unknown";
  }
}

/**
 * Suy ra label hiển thị của plan khi backend trả `planDisplayName` trống.
 * @param planCode Mã plan đã chuẩn (lowercase) do backend trả.
 * @returns Tên plan dạng Title Case để Owner Admin đọc dễ trong drawer/table.
 */
function fallbackPlanDisplayName(planCode: TenantPlanCode): string {
  switch (planCode) {
    case "starter":
      return "Starter";
    case "growth":
      return "Growth";
    case "premium":
      return "Premium";
    default:
      return planCode;
  }
}

/**
 * Chọn domain mặc định để bind vào trường flat `defaultDomainName/domainStatus` của FE shape.
 * Ưu tiên domain có `isPrimary === true`, fallback domain đầu tiên trong mảng.
 * @param domains Danh sách domain do backend trả; có thể undefined hoặc rỗng.
 * @returns Domain được chọn hoặc undefined nếu mảng rỗng.
 */
function pickPrimaryDomain(domains: BackendTenantDomain[] | undefined): BackendTenantDomain | undefined {
  if (!domains || domains.length === 0) {
    return undefined;
  }

  return domains.find((domain) => domain.isPrimary === true) ?? domains[0];
}

/**
 * Map module list (mỗi item có cờ `isEnabled`) thành mảng moduleCode đã enable phẳng cho FE shape.
 * @param modules Danh sách module do backend trả.
 * @returns Mảng `TenantModuleCode` chỉ chứa module đang bật, theo thứ tự backend trả.
 */
function pickEnabledModuleCodes(modules: BackendTenantModule[] | undefined): TenantModuleCode[] {
  if (!modules) {
    return [];
  }

  return modules.filter((module) => module.isEnabled !== false).map((module) => module.moduleCode);
}

/**
 * Đảm bảo mỗi domain có id ổn định để dùng làm Vue `:key`.
 * Backend list endpoint hiện chưa kèm `id` cho domain trong tương lai (forward-compat).
 * @param tenantId Id tenant cha, dùng làm namespace fallback.
 * @param domain Domain raw từ wire format backend.
 * @returns FE TenantDomain với id chắc chắn không undefined.
 */
function adaptDomain(tenantId: string, domain: BackendTenantDomain): TenantDomain {
  return {
    id: domain.id ?? `${tenantId}-${domain.domainName}`,
    domainName: domain.domainName,
    isPrimary: Boolean(domain.isPrimary),
    status: normalizeDomainStatus(domain.status)
  };
}

/**
 * Adapter chính: chuyển detail wire format → FE TenantDetail flat.
 * @param raw Body detail thô do backend trả (đã `JSON.parse`).
 * @returns TenantDetail đầy đủ field mà UI (drawer, page) đang đọc.
 */
export function adaptTenantDetail(raw: BackendTenantDetail): TenantDetail {
  const primaryDomain = pickPrimaryDomain(raw.domains);
  const enabledModules = pickEnabledModuleCodes(raw.modules);
  const planDisplayName =
    raw.planDisplayName && raw.planDisplayName.trim().length > 0
      ? raw.planDisplayName
      : fallbackPlanDisplayName(raw.planCode);

  return {
    id: raw.id,
    slug: raw.slug,
    displayName: raw.displayName,
    status: raw.status,
    planCode: raw.planCode,
    planDisplayName,
    clinicName: raw.profile?.clinicName ?? raw.displayName,
    specialty: raw.profile?.specialty ?? "",
    contactEmail: raw.profile?.contactEmail ?? "",
    phoneNumber: raw.profile?.phoneNumber ?? "",
    addressLine: raw.profile?.addressLine ?? "",
    defaultDomainName: primaryDomain?.domainName ?? "",
    domainStatus: normalizeDomainStatus(primaryDomain?.status),
    moduleCodes: enabledModules,
    createdAt: raw.createdAtUtc ?? new Date(0).toISOString(),
    ownerName: undefined,
    domains: (raw.domains ?? []).map((domain) => adaptDomain(raw.id, domain))
  };
}

/**
 * Adapter cho list endpoint: list trả flat partial (không có nested profile/domains/modules).
 * Các field thiếu sẽ fallback chuỗi rỗng / mảng rỗng để UI render được nhưng vẫn rõ là chưa có data.
 * @param raw Item summary thô do backend list trả.
 * @returns TenantSummary với mọi field FE shape mong đợi.
 */
export function adaptTenantSummary(raw: BackendTenantSummary): TenantSummary {
  const planDisplayName =
    raw.planDisplayName && raw.planDisplayName.trim().length > 0
      ? raw.planDisplayName
      : fallbackPlanDisplayName(raw.planCode);

  return {
    id: raw.id,
    slug: raw.slug,
    displayName: raw.displayName,
    clinicName: raw.clinicName ?? raw.displayName,
    status: raw.status,
    planCode: raw.planCode,
    planDisplayName,
    specialty: "",
    defaultDomainName: "",
    domainStatus: "unknown",
    moduleCodes: [],
    createdAt: raw.createdAtUtc ?? new Date(0).toISOString(),
    contactEmail: ""
  };
}

/**
 * Adapter cho envelope phân trang: chỉ trả mảng FE shape, drop metadata `total/limit/offset`
 * vì UI Phase 3 chưa cần (FE list giả định mảng phẳng). Có thể nâng cấp trả paginated sau.
 * @param raw Envelope list thô do backend trả; nếu đã là mảng thuần thì map trực tiếp.
 * @returns Mảng TenantSummary đã chuẩn FE shape.
 */
export function adaptTenantListResponse(
  raw: BackendTenantListResponse | BackendTenantSummary[]
): TenantSummary[] {
  if (Array.isArray(raw)) {
    return raw.map(adaptTenantSummary);
  }

  if (!raw || !Array.isArray(raw.items)) {
    return [];
  }

  return raw.items.map(adaptTenantSummary);
}
