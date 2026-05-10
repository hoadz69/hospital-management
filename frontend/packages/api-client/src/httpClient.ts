export type HttpClientOptions = {
  baseUrl: string;
  headers?: Record<string, string>;
};

export type HttpRequestOptions = {
  method?: "GET" | "POST" | "PATCH";
  body?: unknown;
};

export class HttpError extends Error {
  readonly status: number;
  readonly payload: unknown;

  constructor(status: number, message: string, payload: unknown) {
    super(message);
    this.name = "HttpError";
    this.status = status;
    this.payload = payload;
  }
}

export function createHttpClient(options: HttpClientOptions) {
  const baseUrl = options.baseUrl.replace(/\/$/, "");

  async function request<T>(path: string, requestOptions: HttpRequestOptions = {}): Promise<T> {
    const response = await fetch(`${baseUrl}${path}`, {
      method: requestOptions.method ?? "GET",
      headers: {
        "Content-Type": "application/json",
        "X-Owner-Role": "OwnerSuperAdmin",
        ...(options.headers ?? {})
      },
      body: requestOptions.body === undefined ? undefined : JSON.stringify(requestOptions.body)
    });

    const text = await response.text();
    const payload = text ? JSON.parse(text) : undefined;

    if (!response.ok) {
      const message =
        typeof payload === "object" && payload && "message" in payload
          ? String((payload as { message: unknown }).message)
          : `Request failed with status ${response.status}`;

      throw new HttpError(response.status, message, payload);
    }

    return payload as T;
  }

  return { request };
}
