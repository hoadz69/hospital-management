import { createHttpClient, resolveHttpHeaders, type HttpClient, type HttpHeaderSource } from "./httpClient";

export type PublicTenantDomain = string | (() => string | undefined | null);

export type PublicHttpClientOptions = {
  baseUrl: string;
  tenantDomain?: PublicTenantDomain;
  headers?: HttpHeaderSource;
};

function resolveTenantDomain(source: PublicTenantDomain | undefined): string | undefined {
  const tenantDomain = typeof source === "function" ? source() : source;
  if (!tenantDomain || tenantDomain.trim() === "") {
    return undefined;
  }

  return tenantDomain.trim();
}

export function createPublicHttpClient(options: PublicHttpClientOptions): HttpClient {
  return createHttpClient({
    baseUrl: options.baseUrl,
    headers: () => ({
      ...resolveHttpHeaders(options.headers),
      "X-Tenant-Domain": resolveTenantDomain(options.tenantDomain)
    })
  });
}
