import { createHttpClient, resolveHttpHeaders, type HttpClient, type HttpHeaderSource } from "./httpClient";

export type ClinicTenantContext = string | (() => string | undefined | null);

export type ClinicHttpClientOptions = {
  baseUrl: string;
  tenantId: ClinicTenantContext;
  headers?: HttpHeaderSource;
};

function resolveTenantId(source: ClinicTenantContext): string {
  const tenantId = typeof source === "function" ? source() : source;
  if (!tenantId || tenantId.trim() === "") {
    throw new Error("Clinic API client requires tenant context.");
  }

  return tenantId.trim();
}

export function createClinicHttpClient(options: ClinicHttpClientOptions): HttpClient {
  return createHttpClient({
    baseUrl: options.baseUrl,
    headers: () => ({
      ...resolveHttpHeaders(options.headers),
      "X-Tenant-Id": resolveTenantId(options.tenantId)
    })
  });
}
