import { ref, type Ref } from "vue";

export type TenantContextRole = "owner" | "clinic" | "public";

export type TenantContextInput = {
  tenantId?: string;
  tenantSlug?: string;
  role?: TenantContextRole;
};

export type TenantContextControls = {
  tenantId: Ref<string | undefined>;
  tenantSlug: Ref<string | undefined>;
  role: Ref<TenantContextRole>;
  setTenantContext(context: TenantContextInput): void;
  clearTenantContext(): void;
  requireTenantId(): string;
};

export function useTenantContext(initial: TenantContextInput = {}): TenantContextControls {
  const tenantId = ref<string | undefined>(initial.tenantId);
  const tenantSlug = ref<string | undefined>(initial.tenantSlug);
  const role = ref<TenantContextRole>(initial.role ?? "public");

  function setTenantContext(context: TenantContextInput): void {
    if ("tenantId" in context) {
      tenantId.value = context.tenantId;
    }
    if ("tenantSlug" in context) {
      tenantSlug.value = context.tenantSlug;
    }
    if (context.role) {
      role.value = context.role;
    }
  }

  function clearTenantContext(): void {
    tenantId.value = undefined;
    tenantSlug.value = undefined;
    role.value = "public";
  }

  function requireTenantId(): string {
    const value = tenantId.value?.trim();
    if (!value) {
      throw new Error("Tenant context is required but tenantId is missing.");
    }

    return value;
  }

  return {
    tenantId,
    tenantSlug,
    role,
    setTenantContext,
    clearTenantContext,
    requireTenantId
  };
}
