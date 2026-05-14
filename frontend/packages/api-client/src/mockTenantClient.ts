import type {
  ApiConflictError,
  TenantCreateRequest,
  TenantDetail,
  TenantDomainStatus,
  TenantStatusUpdateRequest,
  TenantSummary
} from "@clinic-saas/shared-types";
import type { DomainDnsStatus, DomainSslStatus, TenantDomainDnsSslState } from "./tenantDomainOperations";

const mockTenants: TenantDetail[] = [
  {
    id: "tenant-aurora-dental",
    slug: "aurora-dental",
    displayName: "Aurora Dental",
    clinicName: "Aurora Dental Center",
    status: "Active",
    planCode: "premium",
    planDisplayName: "Premium",
    specialty: "Dental",
    defaultDomainName: "aurora-dental.clinicos.vn",
    domainStatus: "verified",
    moduleCodes: ["website", "booking", "catalog", "payments", "reports"],
    createdAt: "2026-04-18T08:30:00.000Z",
    contactEmail: "ops@aurora-dental.example",
    phoneNumber: "+84 28 0000 1201",
    addressLine: "12 Nguyen Hue, District 1",
    ownerName: "Linh Nguyen",
    domains: [
      {
        id: "domain-aurora-default",
        domainName: "aurora-dental.clinicos.vn",
        isPrimary: true,
        status: "verified"
      }
    ]
  },
  {
    id: "tenant-river-eye",
    slug: "river-eye",
    displayName: "River Eye Clinic",
    clinicName: "River Eye Clinic",
    status: "Draft",
    planCode: "growth",
    planDisplayName: "Growth",
    specialty: "Eye clinic",
    defaultDomainName: "river-eye.clinicos.vn",
    domainStatus: "pending",
    moduleCodes: ["website", "booking", "catalog"],
    createdAt: "2026-05-02T10:15:00.000Z",
    contactEmail: "admin@river-eye.example",
    phoneNumber: "+84 28 0000 1302",
    addressLine: "88 Le Loi, District 3",
    ownerName: "Minh Tran",
    domains: [
      {
        id: "domain-river-default",
        domainName: "river-eye.clinicos.vn",
        isPrimary: true,
        status: "pending"
      }
    ]
  },
  {
    id: "tenant-nova-skin",
    slug: "nova-skin",
    displayName: "Nova Skin Lab",
    clinicName: "Nova Dermatology Lab",
    status: "Suspended",
    planCode: "starter",
    planDisplayName: "Starter",
    specialty: "Dermatology",
    defaultDomainName: "nova-skin.clinicos.vn",
    domainStatus: "failed",
    moduleCodes: ["website"],
    createdAt: "2026-03-25T14:45:00.000Z",
    contactEmail: "owner@nova-skin.example",
    phoneNumber: "+84 28 0000 1403",
    addressLine: "45 Pasteur, District 1",
    ownerName: "An Pham",
    domains: [
      {
        id: "domain-nova-default",
        domainName: "nova-skin.clinicos.vn",
        isPrimary: true,
        status: "failed"
      }
    ]
  }
];

function toSummary(tenant: TenantDetail): TenantSummary {
  const { phoneNumber, addressLine, ownerName, domains, ...summary } = tenant;
  void phoneNumber;
  void addressLine;
  void ownerName;
  void domains;
  return summary;
}

function createConflict(fields: ApiConflictError["fields"]): ApiConflictError {
  return {
    status: 409,
    message: "Tenant slug, domain, or contact already exists.",
    fields
  };
}

function delay<T>(value: T): Promise<T> {
  return new Promise((resolve) => {
    window.setTimeout(() => resolve(value), 160);
  });
}

function findMockTenant(tenantKey: string) {
  return mockTenants.find((item) => item.id === tenantKey || item.slug === tenantKey);
}

function dnsStatusFromDomain(status: TenantDomainStatus): DomainDnsStatus {
  if (status === "verified") {
    return "verified";
  }

  if (status === "failed") {
    return "failed";
  }

  return "propagating";
}

function sslStatusFromDomain(status: TenantDomainStatus): DomainSslStatus {
  if (status === "verified") {
    return "issued";
  }

  if (status === "failed") {
    return "failed";
  }

  return "pending";
}

function buildMockDomainOperationState(tenant: TenantDetail, domainId: string): TenantDomainDnsSslState {
  const domain = tenant.domains.find((item) => item.id === domainId);
  if (!domain) {
    throw new Error("Domain not found.");
  }

  const dnsStatus = dnsStatusFromDomain(domain.status);
  const now = new Date().toISOString();
  return {
    domainId: domain.id,
    domainName: domain.domainName,
    dnsStatus,
    dnsRecords: [
      {
        recordType: domain.isPrimary ? "CNAME" : "TXT",
        host: domain.isPrimary ? domain.domainName : `_clinicos.${domain.domainName}`,
        expectedValue: domain.isPrimary ? "cname.owner-gateway.clinicos.vn" : `clinicos-tenant-verify=${tenant.slug}`,
        actualValue: dnsStatus === "failed" ? "legacy-gateway.clinicos.vn" : undefined,
        status: dnsStatus,
        message: dnsStatus === "verified" ? "DNS verified." : dnsStatus === "failed" ? "Record mismatch." : "Propagation pending."
      }
    ],
    lastCheckedAt: now,
    retryCount: dnsStatus === "verified" ? 0 : 1,
    nextRetryAt: new Date(Date.now() + 90 * 1000).toISOString(),
    sslStatus: sslStatusFromDomain(domain.status),
    sslIssuer: domain.status === "verified" ? "Mock CA" : undefined,
    expiresAt: domain.status === "verified" ? new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString() : undefined,
    message: domain.status === "verified" ? "Domain is ready." : "Mock domain operation state."
  };
}

function buildMockDomainOperationStates(tenant: TenantDetail): TenantDomainDnsSslState[] {
  return tenant.domains.map((domain) => buildMockDomainOperationState(tenant, domain.id));
}

export function createMockTenantClient() {
  return {
    async listTenants(): Promise<TenantSummary[]> {
      return delay(mockTenants.map(toSummary));
    },

    async getTenant(tenantId: string): Promise<TenantDetail> {
      const tenant = findMockTenant(tenantId);
      if (!tenant) {
        throw new Error("Tenant not found.");
      }

      return delay({ ...tenant, domains: tenant.domains.map((domain) => ({ ...domain })) });
    },

    async createTenant(payload: TenantCreateRequest): Promise<TenantDetail> {
      const conflictFields: ApiConflictError["fields"] = [];

      if (mockTenants.some((tenant) => tenant.slug === payload.slug)) {
        conflictFields.push("slug");
      }

      if (mockTenants.some((tenant) => tenant.defaultDomainName === payload.defaultDomainName)) {
        conflictFields.push("defaultDomainName");
      }

      if (mockTenants.some((tenant) => tenant.contactEmail === payload.contactEmail)) {
        conflictFields.push("contactEmail");
      }

      if (conflictFields.length > 0) {
        throw createConflict(conflictFields);
      }

      const tenant: TenantDetail = {
        ...payload,
        id: `tenant-${payload.slug}`,
        status: "Draft",
        planDisplayName: payload.planCode[0].toUpperCase() + payload.planCode.slice(1),
        domainStatus: "pending",
        createdAt: new Date().toISOString(),
        ownerName: payload.displayName,
        domains: [
          {
            id: `domain-${payload.slug}`,
            domainName: payload.defaultDomainName,
            isPrimary: true,
            status: "pending"
          }
        ]
      };

      mockTenants.unshift(tenant);
      return delay(tenant);
    },

    async updateTenantStatus(tenantId: string, payload: TenantStatusUpdateRequest): Promise<TenantDetail> {
      const tenant = findMockTenant(tenantId);
      if (!tenant) {
        throw new Error("Tenant not found.");
      }

      tenant.status = payload.status;
      return delay({ ...tenant, domains: tenant.domains.map((domain) => ({ ...domain })) });
    },

    async listTenantDomainDnsSslStates(tenantId: string): Promise<TenantDomainDnsSslState[]> {
      const tenant = findMockTenant(tenantId);
      if (!tenant) {
        throw new Error("Tenant not found.");
      }

      return delay(buildMockDomainOperationStates(tenant));
    },

    async retryTenantDomainDns(tenantId: string, domainId: string): Promise<TenantDomainDnsSslState> {
      const tenant = findMockTenant(tenantId);
      if (!tenant) {
        throw new Error("Tenant not found.");
      }

      return delay({
        ...buildMockDomainOperationState(tenant, domainId),
        retryCount: 2,
        lastCheckedAt: new Date().toISOString(),
        message: "Mock DNS retry accepted."
      });
    },

    async getTenantDomainSslStatus(tenantId: string, domainId: string): Promise<TenantDomainDnsSslState> {
      const tenant = findMockTenant(tenantId);
      if (!tenant) {
        throw new Error("Tenant not found.");
      }

      return delay(buildMockDomainOperationState(tenant, domainId));
    }
  };
}
