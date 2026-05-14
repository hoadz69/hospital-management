export type DomainDnsStatus = "pending" | "propagating" | "failed" | "verified";

export type DomainSslStatus = "none" | "pending" | "issued" | "failed";

export type TenantDomainDnsRecord = {
  recordType: string;
  host: string;
  expectedValue: string;
  actualValue?: string;
  status: DomainDnsStatus;
  message?: string;
};

export type TenantDomainDnsSslState = {
  domainId: string;
  domainName: string;
  dnsStatus: DomainDnsStatus;
  dnsRecords: TenantDomainDnsRecord[];
  lastCheckedAt?: string;
  retryCount: number;
  nextRetryAt?: string;
  sslStatus: DomainSslStatus;
  sslIssuer?: string;
  expiresAt?: string;
  message?: string;
};

type BackendDomainDnsRecord = {
  recordType?: string;
  RecordType?: string;
  host?: string;
  Host?: string;
  expectedValue?: string;
  ExpectedValue?: string;
  actualValue?: string | null;
  ActualValue?: string | null;
  status?: string;
  Status?: string;
  message?: string | null;
  Message?: string | null;
};

type BackendDomainDnsSslState = {
  domainId?: string;
  DomainId?: string;
  domainName?: string;
  DomainName?: string;
  dnsStatus?: string;
  DnsStatus?: string;
  dnsRecords?: BackendDomainDnsRecord[];
  DnsRecords?: BackendDomainDnsRecord[];
  lastCheckedAt?: string | null;
  LastCheckedAt?: string | null;
  retryCount?: number;
  RetryCount?: number;
  nextRetryAt?: string | null;
  NextRetryAt?: string | null;
  sslStatus?: string;
  SslStatus?: string;
  sslIssuer?: string | null;
  SslIssuer?: string | null;
  expiresAt?: string | null;
  ExpiresAt?: string | null;
  message?: string | null;
  Message?: string | null;
};

type BackendDomainDnsSslList = {
  items?: BackendDomainDnsSslState[];
  Items?: BackendDomainDnsSslState[];
};

function normalizeDnsStatus(value: string | undefined): DomainDnsStatus {
  switch ((value ?? "").toLowerCase()) {
    case "verified":
      return "verified";
    case "failed":
      return "failed";
    case "propagating":
      return "propagating";
    default:
      return "pending";
  }
}

function normalizeSslStatus(value: string | undefined): DomainSslStatus {
  switch ((value ?? "").toLowerCase()) {
    case "issued":
      return "issued";
    case "pending":
      return "pending";
    case "failed":
      return "failed";
    default:
      return "none";
  }
}

function optionalString(value: string | null | undefined): string | undefined {
  return value === null ? undefined : value;
}

function adaptRecord(raw: BackendDomainDnsRecord): TenantDomainDnsRecord {
  return {
    recordType: raw.recordType ?? raw.RecordType ?? "CNAME",
    host: raw.host ?? raw.Host ?? "",
    expectedValue: raw.expectedValue ?? raw.ExpectedValue ?? "",
    actualValue: optionalString(raw.actualValue ?? raw.ActualValue),
    status: normalizeDnsStatus(raw.status ?? raw.Status),
    message: optionalString(raw.message ?? raw.Message)
  };
}

export function adaptTenantDomainDnsSslState(payload: unknown): TenantDomainDnsSslState {
  const raw = (typeof payload === "object" && payload !== null ? payload : {}) as BackendDomainDnsSslState;

  return {
    domainId: raw.domainId ?? raw.DomainId ?? "",
    domainName: raw.domainName ?? raw.DomainName ?? "",
    dnsStatus: normalizeDnsStatus(raw.dnsStatus ?? raw.DnsStatus),
    dnsRecords: (raw.dnsRecords ?? raw.DnsRecords ?? []).map(adaptRecord),
    lastCheckedAt: optionalString(raw.lastCheckedAt ?? raw.LastCheckedAt),
    retryCount: raw.retryCount ?? raw.RetryCount ?? 0,
    nextRetryAt: optionalString(raw.nextRetryAt ?? raw.NextRetryAt),
    sslStatus: normalizeSslStatus(raw.sslStatus ?? raw.SslStatus),
    sslIssuer: optionalString(raw.sslIssuer ?? raw.SslIssuer),
    expiresAt: optionalString(raw.expiresAt ?? raw.ExpiresAt),
    message: optionalString(raw.message ?? raw.Message)
  };
}

export function adaptTenantDomainDnsSslListResponse(
  payload: unknown
): TenantDomainDnsSslState[] {
  const raw = (typeof payload === "object" && payload !== null ? payload : {}) as
    | BackendDomainDnsSslList
    | BackendDomainDnsSslState[];

  if (Array.isArray(raw)) {
    return raw.map(adaptTenantDomainDnsSslState);
  }

  return (raw.items ?? raw.Items ?? []).map(adaptTenantDomainDnsSslState);
}
