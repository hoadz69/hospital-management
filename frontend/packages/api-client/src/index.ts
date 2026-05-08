export type TenantRequestContext = {
  tenantId?: string;
  role: "owner" | "clinic-admin" | "public";
};

export function requireTenantContext(context: TenantRequestContext): string {
  if (!context.tenantId && context.role !== "owner") {
    throw new Error("Tenant context is required for tenant-owned requests.");
  }

  return context.tenantId ?? "platform";
}
