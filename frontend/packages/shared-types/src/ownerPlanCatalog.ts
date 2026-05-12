import type { TenantPlanCode } from "./tenant";

export type OwnerPlanTone = "info" | "neutral" | "warning";

export type OwnerPlanCatalogItem = {
  code: TenantPlanCode;
  name: string;
  price: number;
  description: string;
  tenantCount: number;
  tone: OwnerPlanTone;
  popular?: boolean;
};

export type OwnerModuleEntitlement = string | boolean;

export type OwnerModuleCatalogRow = {
  id: string;
  name: string;
  category: string;
  starter: OwnerModuleEntitlement;
  growth: OwnerModuleEntitlement;
  premium: OwnerModuleEntitlement;
};

export type OwnerTenantPlanAssignment = {
  id: string;
  slug: string;
  currentPlan: TenantPlanCode;
  currentPlanName: string;
  currentMrr: number;
  nextRenewal: string;
  selected: boolean;
  targetPlan: TenantPlanCode;
};

export type OwnerPlanBulkChangeRequest = {
  selectedTenantIds: string[];
  targetPlan: TenantPlanCode;
  effectiveAt: "next_renewal";
  auditReason: string;
};

export type OwnerPlanBulkChangeResponse = {
  changedCount: number;
  mrrDiff: number;
  status: string;
  message: string;
  effectiveAt: string;
  auditReason: string;
};
