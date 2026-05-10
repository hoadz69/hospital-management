import { createHttpClient, resolveHttpHeaders, type HttpClient, type HttpHeaderSource } from "./httpClient";

export type OwnerHttpClientOptions = {
  baseUrl: string;
  ownerRole: string;
  headers?: HttpHeaderSource;
};

export function createOwnerHttpClient(options: OwnerHttpClientOptions): HttpClient {
  const ownerRole = options.ownerRole.trim();
  if (!ownerRole) {
    throw new Error("Owner API client requires a non-empty ownerRole.");
  }

  return createHttpClient({
    baseUrl: options.baseUrl,
    headers: () => ({
      ...resolveHttpHeaders(options.headers),
      "X-Owner-Role": ownerRole
    })
  });
}
