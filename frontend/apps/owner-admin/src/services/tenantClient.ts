import { createTenantClient, type TenantClientMode } from "@clinic-saas/api-client";

function getMode(value: string | undefined): TenantClientMode {
  if (value === "real" || value === "mock" || value === "auto") {
    return value;
  }

  return "auto";
}

export const tenantClient = createTenantClient({
  baseUrl: import.meta.env.VITE_API_BASE_URL,
  mode: getMode(import.meta.env.VITE_TENANT_API_MODE),
  fallbackToMock: import.meta.env.VITE_TENANT_API_FALLBACK !== "false",
  ownerRole: "OwnerSuperAdmin"
});
