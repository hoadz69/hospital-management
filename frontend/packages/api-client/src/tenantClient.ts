import type {
  ApiConflictError,
  TenantConflictField,
  TenantCreateRequest,
  TenantDetail,
  TenantStatusUpdateRequest,
  TenantSummary
} from "@clinic-saas/shared-types";
import { HttpError, type HttpHeaderSource } from "./httpClient";
import { createMockTenantClient } from "./mockTenantClient";
import { createOwnerHttpClient } from "./owner";
import {
  adaptTenantDetail,
  adaptTenantListResponse,
  type BackendTenantDetail,
  type BackendTenantListResponse,
  type BackendTenantSummary
} from "./tenantAdapter";

export type TenantClientMode = "auto" | "real" | "mock";

export type TenantClientOptions = {
  baseUrl?: string;
  mode?: TenantClientMode;
  fallbackToMock?: boolean;
  ownerRole?: string;
  headers?: HttpHeaderSource;
};

export type TenantClient = {
  listTenants(): Promise<TenantSummary[]>;
  getTenant(tenantId: string): Promise<TenantDetail>;
  createTenant(payload: TenantCreateRequest): Promise<TenantDetail>;
  updateTenantStatus(tenantId: string, payload: TenantStatusUpdateRequest): Promise<TenantDetail>;
};

/**
 * Wire format ProblemDetails RFC 9457 mà gateway/backend trả khi gặp conflict (HTTP 409).
 * Backend không trả `fields` nên FE phải parse `detail` text để suy ra field bị trùng.
 */
type ProblemDetailsConflict = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
};

/**
 * Phán đoán payload có phải ProblemDetails RFC 9457 không (ít nhất có `title` hoặc `detail`,
 * và KHÔNG có `fields`).
 * @param payload Body 409 đã JSON.parse từ HttpError.
 * @returns true nếu payload trông giống ProblemDetails và cần parse text.
 */
function isProblemDetailsPayload(payload: unknown): payload is ProblemDetailsConflict {
  if (typeof payload !== "object" || payload === null) {
    return false;
  }

  const candidate = payload as Record<string, unknown>;
  const looksLikeProblemDetails =
    typeof candidate.title === "string" ||
    typeof candidate.detail === "string" ||
    typeof candidate.type === "string" ||
    typeof candidate.traceId === "string";
  const hasFieldsArray = Array.isArray((candidate as { fields?: unknown }).fields);

  return looksLikeProblemDetails && !hasFieldsArray;
}

/**
 * Parse text `detail` của ProblemDetails để suy ra danh sách field bị trùng.
 * Backend hiện trả các message dạng "Tenant slug or domain already exists." hoặc tương tự,
 * nên chỉ cần keyword match case-insensitive là đủ. Không match -> mảng rỗng (graceful fallback).
 * @param detail Text detail do backend gửi.
 * @returns Mảng field FE shape (không trùng, giữ thứ tự slug/domain/email).
 */
function parseConflictFieldsFromDetail(detail: string | undefined): TenantConflictField[] {
  if (!detail) {
    return [];
  }

  const fields: TenantConflictField[] = [];
  if (/slug/i.test(detail)) {
    fields.push("slug");
  }
  if (/domain/i.test(detail)) {
    fields.push("defaultDomainName");
  }
  if (/email|contact/i.test(detail)) {
    fields.push("contactEmail");
  }

  return fields;
}

/**
 * Chuẩn hoá lỗi 409 từ nhiều nguồn (HttpError ProblemDetails, HttpError JSON tự định nghĩa,
 * hay object đã có shape ApiConflictError) về kiểu `ApiConflictError` mà UI mong đợi.
 * @param error Bất cứ giá trị nào throw ra từ http layer hoặc upstream.
 * @returns ApiConflictError nếu là conflict 409, undefined nếu không phải.
 */
function normalizeConflict(error: unknown): ApiConflictError | undefined {
  if (error instanceof HttpError && error.status === 409) {
    const payload = error.payload;

    // Forward-compat: nếu backend đã đẩy sẵn `fields` thì respect mảng đó luôn.
    if (
      typeof payload === "object" &&
      payload !== null &&
      Array.isArray((payload as { fields?: unknown }).fields)
    ) {
      const typed = payload as Partial<ApiConflictError>;
      return {
        status: 409,
        message: typed.message ?? error.message,
        fields: typed.fields ?? []
      };
    }

    // ProblemDetails RFC 9457: parse `detail` text để suy ra field bị trùng.
    if (isProblemDetailsPayload(payload)) {
      const detail = payload.detail ?? payload.title ?? error.message;
      return {
        status: 409,
        message: detail,
        fields: parseConflictFieldsFromDetail(payload.detail)
      };
    }

    // Fallback cuối: dùng message HttpError, không có fields.
    return {
      status: 409,
      message: error.message,
      fields: []
    };
  }

  if (
    typeof error === "object" &&
    error !== null &&
    "status" in error &&
    (error as { status: unknown }).status === 409
  ) {
    return error as ApiConflictError;
  }

  return undefined;
}

export function isApiConflictError(error: unknown): error is ApiConflictError {
  return normalizeConflict(error) !== undefined;
}

export function createTenantClient(options: TenantClientOptions = {}): TenantClient {
  const mode = options.mode ?? "auto";
  const mockClient = createMockTenantClient();

  if (mode === "mock" || (!options.baseUrl && mode === "auto")) {
    return mockClient;
  }

  if (!options.baseUrl) {
    throw new Error("Tenant API baseUrl is required in real mode.");
  }

  const http = createOwnerHttpClient({
    baseUrl: options.baseUrl,
    ownerRole: options.ownerRole ?? "",
    headers: options.headers
  });
  const fallbackToMock = options.fallbackToMock ?? mode === "auto";

  async function withFallback<T>(operation: () => Promise<T>, fallback: () => Promise<T>): Promise<T> {
    try {
      return await operation();
    } catch (error) {
      const conflict = normalizeConflict(error);
      if (conflict) {
        throw conflict;
      }

      if (fallbackToMock) {
        return fallback();
      }

      throw error;
    }
  }

  return {
    listTenants() {
      return withFallback(
        async () => {
          // Backend trả paged envelope `{items,total,limit,offset}` với item flat partial,
          // adapter sẽ map sang `TenantSummary[]` và fallback các field thiếu thành rỗng.
          const raw = await http.request<BackendTenantListResponse | BackendTenantSummary[]>(
            "/api/tenants"
          );
          return adaptTenantListResponse(raw);
        },
        () => mockClient.listTenants()
      );
    },

    getTenant(tenantId: string) {
      return withFallback(
        async () => {
          // Backend detail dạng nested (profile/domains/modules); adapter chuyển sang FE shape flat.
          const raw = await http.request<BackendTenantDetail>(`/api/tenants/${tenantId}`);
          return adaptTenantDetail(raw);
        },
        () => mockClient.getTenant(tenantId)
      );
    },

    createTenant(payload: TenantCreateRequest) {
      return withFallback(
        async () => {
          // Request body giữ nguyên FE shape vì backend đã accept đúng key set theo spec wizard.
          const raw = await http.request<BackendTenantDetail>("/api/tenants", {
            method: "POST",
            body: payload
          });
          return adaptTenantDetail(raw);
        },
        () => mockClient.createTenant(payload)
      );
    },

    updateTenantStatus(tenantId: string, payload: TenantStatusUpdateRequest) {
      return withFallback(
        async () => {
          const raw = await http.request<BackendTenantDetail>(
            `/api/tenants/${tenantId}/status`,
            {
              method: "PATCH",
              body: payload
            }
          );
          return adaptTenantDetail(raw);
        },
        () => mockClient.updateTenantStatus(tenantId, payload)
      );
    }
  };
}
