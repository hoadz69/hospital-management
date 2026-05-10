export type HttpHeaderValue = string | undefined | null;

export type HttpHeaders = Record<string, HttpHeaderValue>;

export type HttpHeaderSource = HttpHeaders | (() => HttpHeaders);

export type HttpClientOptions = {
  baseUrl: string;
  headers?: HttpHeaderSource;
};

export type HttpRequestOptions = {
  method?: "GET" | "POST" | "PATCH";
  body?: unknown;
  headers?: HttpHeaderSource;
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

export type HttpClient = {
  request<T>(path: string, requestOptions?: HttpRequestOptions): Promise<T>;
};

export function resolveHttpHeaders(source: HttpHeaderSource | undefined): Record<string, string> {
  const headers = typeof source === "function" ? source() : source;
  if (!headers) {
    return {};
  }

  return Object.fromEntries(
    Object.entries(headers).filter((entry): entry is [string, string] => {
      const [, value] = entry;
      return typeof value === "string" && value.trim() !== "";
    })
  );
}

export function createHttpClient(options: HttpClientOptions) {
  const baseUrl = options.baseUrl.replace(/\/$/, "");

  async function request<T>(path: string, requestOptions: HttpRequestOptions = {}): Promise<T> {
    const headers = {
      "Content-Type": "application/json",
      ...resolveHttpHeaders(options.headers),
      ...resolveHttpHeaders(requestOptions.headers)
    };

    const response = await fetch(`${baseUrl}${path}`, {
      method: requestOptions.method ?? "GET",
      headers,
      body: requestOptions.body === undefined ? undefined : JSON.stringify(requestOptions.body)
    });

    const text = await response.text();

    // Wrap JSON.parse trong try/catch để tránh SyntaxError không được catch khi
    // backend hoặc reverse proxy trả non-JSON (ví dụ HTML 502 từ nginx, plain text
    // "Bad Gateway"). Trong tình huống đó FE phải nhận được HttpError có status
    // và raw text payload thay vì crash uncaught.
    let payload: unknown = undefined;
    let parseFailed = false;
    if (text) {
      try {
        payload = JSON.parse(text);
      } catch {
        parseFailed = true;
        payload = text;
      }
    }

    if (!response.ok) {
      const message =
        typeof payload === "object" && payload && "message" in payload
          ? String((payload as { message: unknown }).message)
          : `Request failed with status ${response.status}`;

      throw new HttpError(response.status, message, payload);
    }

    if (parseFailed) {
      // Response 2xx nhưng body không phải JSON: vẫn coi là lỗi giao tiếp,
      // surface qua HttpError để upstream (tenantClient) xử lý đồng nhất với 4xx/5xx
      // thay vì để JSON.parse SyntaxError lan ra UI.
      // Message dùng 200 ký tự đầu của raw text để debugger thấy ngay nội dung
      // proxy/gateway trả (ví dụ snippet HTML 502 từ nginx) thay vì thông báo chung.
      const trimmedText = text.trim();
      const message =
        trimmedText.length > 0
          ? trimmedText.slice(0, 200)
          : "Empty response body";
      throw new HttpError(response.status, message, payload);
    }

    return payload as T;
  }

  return { request } satisfies HttpClient;
}
