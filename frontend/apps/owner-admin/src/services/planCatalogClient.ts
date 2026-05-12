import {
  createOwnerPlanCatalogClient,
  type OwnerPlanCatalogClient as RealOwnerPlanCatalogClient
} from "@clinic-saas/api-client";
import type {
  OwnerModuleCatalogRow,
  OwnerPlanBulkChangeRequest,
  OwnerPlanBulkChangeResponse,
  OwnerPlanCatalogItem,
  OwnerTenantPlanAssignment
} from "@clinic-saas/shared-types";
import {
  getPlanPrice,
  moduleCatalog,
  planCatalog,
  tenantPlanAssignments
} from "./planCatalogMock";

export type OwnerPlanCatalogClientMode = "auto" | "real" | "mock";

export type PlanCatalogClient = {
  listPlans(): Promise<OwnerPlanCatalogItem[]>;
  listModules(): Promise<OwnerModuleCatalogRow[]>;
  listTenantPlanAssignments(): Promise<OwnerTenantPlanAssignment[]>;
  bulkChangeTenantPlans(payload: OwnerPlanBulkChangeRequest): Promise<OwnerPlanBulkChangeResponse>;
};

function getMode(value: string | undefined): OwnerPlanCatalogClientMode {
  if (value === "real" || value === "mock" || value === "auto") {
    return value;
  }

  return "auto";
}

function cloneAssignments() {
  return tenantPlanAssignments.map((assignment) => ({ ...assignment }));
}

function createMockPlanCatalogClient(): PlanCatalogClient {
  return {
    async listPlans() {
      return planCatalog.map((plan) => ({ ...plan }));
    },

    async listModules() {
      return moduleCatalog.map((module) => ({ ...module }));
    },

    async listTenantPlanAssignments() {
      return cloneAssignments();
    },

    async bulkChangeTenantPlans(payload) {
      const selectedIds = new Set(payload.selectedTenantIds);
      const targetPrice = getPlanPrice(payload.targetPlan);
      const selectedAssignments = tenantPlanAssignments.filter((assignment) => selectedIds.has(assignment.id));
      const mrrDiff = selectedAssignments.reduce(
        (total, assignment) => total + targetPrice - assignment.currentMrr,
        0
      );

      return {
        changedCount: selectedAssignments.length,
        mrrDiff,
        status: selectedAssignments.length === 0 ? "noop" : "accepted-mock",
        message:
          selectedAssignments.length === 0
            ? "Chưa có tenant nào khớp dữ liệu mock."
            : `${selectedAssignments.length} tenant sẽ đổi gói ở chu kỳ kế tiếp.`,
        effectiveAt: payload.effectiveAt,
        auditReason: payload.auditReason
      };
    }
  };
}

function hasBaseUrl(value: string | undefined): value is string {
  return value !== undefined;
}

function createPlanCatalogClient(options: {
  baseUrl?: string;
  mode?: OwnerPlanCatalogClientMode;
  fallbackToMock?: boolean;
  ownerRole: string;
}): PlanCatalogClient {
  const mode = options.mode ?? "auto";
  const mockClient = createMockPlanCatalogClient();

  if (mode === "mock" || (!hasBaseUrl(options.baseUrl) && mode === "auto")) {
    return mockClient;
  }

  if (!hasBaseUrl(options.baseUrl)) {
    throw new Error("Owner plan API baseUrl is required in real mode.");
  }

  const realClient = createOwnerPlanCatalogClient({
    baseUrl: options.baseUrl,
    ownerRole: options.ownerRole
  });
  const fallbackToMock = options.fallbackToMock ?? mode === "auto";

  async function withFallback<T>(
    operation: (client: RealOwnerPlanCatalogClient) => Promise<T>,
    fallback: () => Promise<T>
  ) {
    try {
      return await operation(realClient);
    } catch (error) {
      if (fallbackToMock) {
        return fallback();
      }

      throw error;
    }
  }

  return {
    listPlans() {
      return withFallback((client) => client.listPlans(), () => mockClient.listPlans());
    },

    listModules() {
      return withFallback((client) => client.listModules(), () => mockClient.listModules());
    },

    listTenantPlanAssignments() {
      return withFallback((client) => client.listTenantPlanAssignments(), () => mockClient.listTenantPlanAssignments());
    },

    bulkChangeTenantPlans(payload) {
      return withFallback((client) => client.bulkChangeTenantPlans(payload), () => mockClient.bulkChangeTenantPlans(payload));
    }
  };
}

export const planCatalogClient = createPlanCatalogClient({
  baseUrl: import.meta.env.VITE_API_BASE_URL,
  mode: getMode(import.meta.env.VITE_OWNER_PLAN_API_MODE),
  fallbackToMock: import.meta.env.VITE_OWNER_PLAN_API_FALLBACK !== "false",
  ownerRole: "OwnerSuperAdmin"
});
