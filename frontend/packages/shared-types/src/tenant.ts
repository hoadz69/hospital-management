export type TenantStatus = "Draft" | "Active" | "Suspended" | "Archived";

export type TenantDomainStatus = "verified" | "pending" | "failed" | "unknown";

export type TenantModuleCode =
  | "website"
  | "booking"
  | "catalog"
  | "payments"
  | "reports"
  | "notifications";

export type TenantPlanCode = "starter" | "growth" | "premium";

export type TenantDomain = {
  id: string;
  domainName: string;
  isPrimary: boolean;
  status: TenantDomainStatus;
};

export type TenantSummary = {
  id: string;
  slug: string;
  displayName: string;
  clinicName: string;
  status: TenantStatus;
  planCode: TenantPlanCode;
  planDisplayName: string;
  specialty: string;
  defaultDomainName: string;
  domainStatus: TenantDomainStatus;
  moduleCodes: TenantModuleCode[];
  createdAt: string;
  contactEmail: string;
};

export type TenantDetail = TenantSummary & {
  phoneNumber: string;
  addressLine: string;
  // Backend wire format hiện tại chưa trả owner profile; field optional để adapter để undefined,
  // drawer chỉ hiển thị dòng "Chủ sở hữu" khi có giá trị.
  ownerName?: string;
  domains: TenantDomain[];
};

export type TenantCreateRequest = {
  slug: string;
  displayName: string;
  clinicName: string;
  planCode: TenantPlanCode;
  specialty: string;
  contactEmail: string;
  phoneNumber: string;
  addressLine: string;
  defaultDomainName: string;
  moduleCodes: TenantModuleCode[];
};

export type TenantStatusUpdateRequest = {
  status: TenantStatus;
};

export type TenantConflictField = "slug" | "defaultDomainName" | "contactEmail";

export type ApiConflictError = {
  status: 409;
  message: string;
  fields: TenantConflictField[];
};

export const mockTenant: TenantSummary = {
  id: "tenant-demo",
  slug: "medicare-demo",
  displayName: "MediCare+ Demo Clinic",
  clinicName: "MediCare+ Demo Clinic",
  status: "Active",
  planCode: "growth",
  planDisplayName: "Growth",
  specialty: "General clinic",
  defaultDomainName: "medicare-demo.clinicos.vn",
  domainStatus: "verified",
  moduleCodes: ["website", "booking", "catalog", "reports"],
  createdAt: "2026-05-01T09:00:00.000Z",
  contactEmail: "owner@medicare-demo.example"
};
