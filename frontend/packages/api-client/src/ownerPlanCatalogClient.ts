import type {
  OwnerModuleCatalogRow,
  OwnerModuleEntitlement,
  OwnerPlanBulkChangeRequest,
  OwnerPlanBulkChangeResponse,
  OwnerPlanCatalogItem,
  OwnerPlanTone,
  OwnerTenantPlanAssignment,
  TenantPlanCode
} from "@clinic-saas/shared-types";
import type { HttpHeaderSource } from "./httpClient";
import { createOwnerHttpClient } from "./owner";

type ItemsEnvelope<T> = {
  items?: T[];
};

type BackendPlanCatalogItem = Partial<Record<keyof OwnerPlanCatalogItem, unknown>>;
type BackendModuleCatalogRow = Partial<Record<keyof OwnerModuleCatalogRow, unknown>>;
type BackendTenantPlanAssignment = Partial<Record<keyof OwnerTenantPlanAssignment, unknown>>;

type BackendBulkChangeResponse = Partial<Record<keyof OwnerPlanBulkChangeResponse, unknown>>;

export type OwnerPlanCatalogClientOptions = {
  baseUrl: string;
  ownerRole: string;
  headers?: HttpHeaderSource;
};

export type OwnerPlanCatalogClient = {
  listPlans(): Promise<OwnerPlanCatalogItem[]>;
  listModules(): Promise<OwnerModuleCatalogRow[]>;
  listTenantPlanAssignments(): Promise<OwnerTenantPlanAssignment[]>;
  bulkChangeTenantPlans(payload: OwnerPlanBulkChangeRequest): Promise<OwnerPlanBulkChangeResponse>;
};

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function unwrapItems<T>(payload: ItemsEnvelope<T> | T[]): T[] {
  if (Array.isArray(payload)) {
    return payload;
  }

  return Array.isArray(payload.items) ? payload.items : [];
}

function toString(value: unknown, fallback: string) {
  return typeof value === "string" && value.trim() !== "" ? value : fallback;
}

function toNumber(value: unknown, fallback = 0) {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string") {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
  }

  return fallback;
}

function toBoolean(value: unknown, fallback = false) {
  return typeof value === "boolean" ? value : fallback;
}

function toPlanCode(value: unknown, fallback: TenantPlanCode): TenantPlanCode {
  return value === "starter" || value === "growth" || value === "premium" ? value : fallback;
}

function toTone(value: unknown): OwnerPlanTone {
  return value === "info" || value === "neutral" || value === "warning" ? value : "neutral";
}

function toEntitlement(value: unknown): OwnerModuleEntitlement {
  if (typeof value === "boolean") {
    return value;
  }

  if (typeof value === "string" && value.trim() !== "") {
    return value;
  }

  return false;
}

function adaptPlanCatalogItem(raw: BackendPlanCatalogItem): OwnerPlanCatalogItem {
  const code = toPlanCode(raw.code, "starter");
  return {
    code,
    name: toString(raw.name, code),
    price: toNumber(raw.price),
    description: toString(raw.description, ""),
    tenantCount: toNumber(raw.tenantCount),
    tone: toTone(raw.tone),
    popular: toBoolean(raw.popular)
  };
}

function adaptModuleCatalogRow(raw: BackendModuleCatalogRow): OwnerModuleCatalogRow {
  return {
    id: toString(raw.id, "unknown-module"),
    name: toString(raw.name, "Unknown module"),
    category: toString(raw.category, "Platform"),
    starter: toEntitlement(raw.starter),
    growth: toEntitlement(raw.growth),
    premium: toEntitlement(raw.premium)
  };
}

function adaptTenantPlanAssignment(raw: BackendTenantPlanAssignment): OwnerTenantPlanAssignment {
  const currentPlan = toPlanCode(raw.currentPlan, "starter");
  return {
    id: toString(raw.id, "unknown-tenant"),
    slug: toString(raw.slug, "unknown-tenant"),
    currentPlan,
    currentPlanName: toString(raw.currentPlanName, currentPlan),
    currentMrr: toNumber(raw.currentMrr),
    nextRenewal: toString(raw.nextRenewal, "next renewal"),
    selected: toBoolean(raw.selected),
    targetPlan: toPlanCode(raw.targetPlan, currentPlan)
  };
}

function adaptBulkChangeResponse(raw: BackendBulkChangeResponse): OwnerPlanBulkChangeResponse {
  return {
    changedCount: toNumber(raw.changedCount),
    mrrDiff: toNumber(raw.mrrDiff),
    status: toString(raw.status, "accepted-stub"),
    message: toString(raw.message, "Plan changes accepted."),
    effectiveAt: toString(raw.effectiveAt, "next_renewal"),
    auditReason: toString(raw.auditReason, "")
  };
}

export function createOwnerPlanCatalogClient(options: OwnerPlanCatalogClientOptions): OwnerPlanCatalogClient {
  const http = createOwnerHttpClient(options);

  return {
    async listPlans() {
      const raw = await http.request<ItemsEnvelope<BackendPlanCatalogItem> | BackendPlanCatalogItem[]>("/api/owner/plans");
      return unwrapItems(raw).filter(isRecord).map(adaptPlanCatalogItem);
    },

    async listModules() {
      const raw = await http.request<ItemsEnvelope<BackendModuleCatalogRow> | BackendModuleCatalogRow[]>("/api/owner/modules");
      return unwrapItems(raw).filter(isRecord).map(adaptModuleCatalogRow);
    },

    async listTenantPlanAssignments() {
      const raw = await http.request<ItemsEnvelope<BackendTenantPlanAssignment> | BackendTenantPlanAssignment[]>(
        "/api/owner/tenant-plan-assignments"
      );
      return unwrapItems(raw).filter(isRecord).map(adaptTenantPlanAssignment);
    },

    async bulkChangeTenantPlans(payload) {
      const raw = await http.request<BackendBulkChangeResponse>("/api/owner/tenant-plan-assignments/bulk-change", {
        method: "POST",
        body: payload
      });
      return adaptBulkChangeResponse(raw);
    }
  };
}
