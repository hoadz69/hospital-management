import type {
  OwnerModuleCatalogRow,
  OwnerPlanCatalogItem,
  OwnerTenantPlanAssignment,
  TenantPlanCode
} from "@clinic-saas/shared-types";

export const planCatalog: OwnerPlanCatalogItem[] = [
  {
    code: "starter",
    name: "Starter",
    price: 49,
    description: "Cho phòng khám nhỏ, tối đa 2 bác sĩ",
    tenantCount: 84,
    tone: "info"
  },
  {
    code: "growth",
    name: "Growth",
    price: 129,
    description: "Phổ biến nhất cho phòng khám 3-10 bác sĩ",
    tenantCount: 132,
    tone: "neutral",
    popular: true
  },
  {
    code: "premium",
    name: "Premium",
    price: 299,
    description: "Phòng khám lớn, đa chi nhánh và vận hành nâng cao",
    tenantCount: 32,
    tone: "warning"
  }
];

export const moduleCatalog: OwnerModuleCatalogRow[] = [
  { id: "public-website", name: "Public Website", category: "Website", starter: "limit-3-pages", growth: "unlimited", premium: "unlimited" },
  { id: "booking-online", name: "Booking Online", category: "Booking", starter: "100/month", growth: "1000/month", premium: "unlimited" },
  { id: "doctor-schedule", name: "Doctor Schedule", category: "Clinic Ops", starter: true, growth: true, premium: true },
  { id: "patient-records", name: "Patient Records", category: "Customer", starter: false, growth: true, premium: true },
  { id: "apso-eprescribe", name: "APSO E-prescribe", category: "Records", starter: false, growth: false, premium: true },
  { id: "custom-domain", name: "Custom domain", category: "Domain", starter: false, growth: true, premium: true },
  { id: "ssl-auto-renew", name: "SSL auto-renew", category: "Domain", starter: true, growth: true, premium: true },
  { id: "telehealth", name: "Telehealth", category: "Care", starter: false, growth: "limit-50/m", premium: "unlimited" },
  { id: "reports-analytics", name: "Reports analytics", category: "Analytics", starter: "basic", growth: "advanced", premium: "advanced + export" },
  { id: "multi-branch", name: "Multi-branch", category: "Operations", starter: false, growth: false, premium: true },
  { id: "api-access", name: "API access", category: "Platform", starter: false, growth: false, premium: true },
  { id: "priority-support", name: "Priority support", category: "Support", starter: false, growth: "email", premium: "phone+email 24/7" }
];

export const tenantPlanAssignments: OwnerTenantPlanAssignment[] = [
  {
    id: "tenant-aurora-dental",
    slug: "aurora-dental",
    currentPlan: "premium",
    currentPlanName: "Premium",
    currentMrr: 299,
    nextRenewal: "01/06/2026",
    selected: true,
    targetPlan: "premium"
  },
  {
    id: "tenant-river-eye",
    slug: "river-eye",
    currentPlan: "growth",
    currentPlanName: "Growth",
    currentMrr: 129,
    nextRenewal: "08/06/2026",
    selected: true,
    targetPlan: "premium"
  },
  {
    id: "tenant-ndtp-pediatric",
    slug: "ndtp-pediatric",
    currentPlan: "premium",
    currentPlanName: "Premium",
    currentMrr: 299,
    nextRenewal: "03/06/2026",
    selected: true,
    targetPlan: "premium"
  },
  {
    id: "tenant-nova-skin",
    slug: "nova-skin",
    currentPlan: "starter",
    currentPlanName: "Starter",
    currentMrr: 49,
    nextRenewal: "05/06/2026",
    selected: false,
    targetPlan: "growth"
  },
  {
    id: "tenant-phuc-lam-ent",
    slug: "phuc-lam-ent",
    currentPlan: "growth",
    currentPlanName: "Growth",
    currentMrr: 129,
    nextRenewal: "28/05/2026",
    selected: false,
    targetPlan: "growth"
  },
  {
    id: "tenant-hongduc-obgyn",
    slug: "hongduc-obgyn",
    currentPlan: "premium",
    currentPlanName: "Premium",
    currentMrr: 299,
    nextRenewal: "26/05/2026",
    selected: false,
    targetPlan: "growth"
  }
];

export function getPlanPrice(planCode: TenantPlanCode) {
  return planCatalog.find((plan) => plan.code === planCode)?.price ?? 0;
}
