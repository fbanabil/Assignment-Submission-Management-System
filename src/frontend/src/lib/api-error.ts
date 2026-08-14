import { getAuthToken, removeAuthToken, setAuthToken } from "./auth";

/**
 * Formats non-OK fetch API responses into clean human-readable error messages
 * including status codes (e.g. "Status 400: Email is required.")
 */
export async function parseApiResponseError(response: Response): Promise<string> {
  const status = response.status;
  if (status === 403) {
    return "Status 403: User is not permitted.";
  }
  if (status === 401) {
    return "Status 401: Login first";
  }

  let extractedMessage = response.statusText || `Request failed`;

  try {
    const text = await response.text();
    if (text && text.trim()) {
      try {
        const data = JSON.parse(text);
        if (typeof data === "string" && data.trim()) {
          extractedMessage = data.trim();
        } else if (data && typeof data === "object") {
          if (data.errors && typeof data.errors === "object") {
            const messages: string[] = [];
            for (const key of Object.keys(data.errors)) {
              const errList = data.errors[key];
              if (Array.isArray(errList)) {
                messages.push(...errList.filter((m): m is string => typeof m === "string"));
              } else if (typeof errList === "string") {
                messages.push(errList);
              }
            }
            if (messages.length > 0) {
              extractedMessage = messages.join(" ");
            } else if (typeof data.title === "string") {
              extractedMessage = data.title;
            }
          } else if (typeof data.message === "string" && data.message.trim()) {
            extractedMessage = data.message.trim();
          } else if (typeof data.error === "string" && data.error.trim()) {
            extractedMessage = data.error.trim();
          } else if (typeof data.title === "string" && data.title.trim()) {
            extractedMessage = data.title.trim();
          } else if (typeof data.detail === "string" && data.detail.trim()) {
            extractedMessage = data.detail.trim();
          }
        }
      } catch {
        extractedMessage = text.trim();
      }
    }
  } catch {
    // Fallback if text reading fails
  }

  return `Status ${status}: ${extractedMessage}`;
}

/**
 * Safely parses response JSON body without throwing "Unexpected end of JSON input"
 * if the response body is empty (e.g. 204 No Content or empty 201/200).
 */
export async function safeParseJson<T>(response: Response, fallback?: Partial<T>): Promise<T> {
  try {
    const text = await response.text();
    if (!text || !text.trim()) {
      return (fallback ?? {}) as T;
    }
    return JSON.parse(text) as T;
  } catch {
    return (fallback ?? {}) as T;
  }
}

/**
 * Builds API endpoint URLs without duplicating the `/api` prefix if it's already in base URL.
 */
export function getApiUrl(path: string): string {
  if (!path) return "";
  if (path.startsWith("http://") || path.startsWith("https://")) {
    return path;
  }

  const baseUrl = (
    process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
    process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ??
    ""
  );

  if (!baseUrl) return path;

  let cleanPath = path.startsWith("/") ? path : `/${path}`;

  if (baseUrl.toLowerCase().endsWith("/api") && cleanPath.toLowerCase().startsWith("/api/")) {
    cleanPath = cleanPath.substring(4);
  }

  return `${baseUrl}${cleanPath}`;
}

let isRefreshing = false;
let refreshSubscribers: Array<(token: string | null) => void> = [];

function onRefreshed(token: string | null) {
  refreshSubscribers.forEach((cb) => cb(token));
  refreshSubscribers = [];
}

function addRefreshSubscriber(cb: (token: string | null) => void) {
  refreshSubscribers.push(cb);
}

/**
 * Sends request to /api/Auth/RefreshToken using HTTP-only cookie credentials to retrieve a new token
 */
export async function tryRefreshToken(): Promise<string | null> {
  const refreshUrl = getApiUrl("/Auth/RefreshToken");
  if (!refreshUrl) return null;

  try {
    const response = await fetch(refreshUrl, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
    });

    if (!response.ok) {
      return null;
    }

    const data = await safeParseJson<{ token: string }>(response);
    if (data && data.token) {
      setAuthToken(data.token);
      return data.token;
    }
    return null;
  } catch {
    return null;
  }
}

function redirectToLogin(message = "Login first") {
  removeAuthToken();
  if (typeof window !== "undefined") {
    const currentPath = window.location.pathname;
    if (currentPath !== "/login") {
      window.location.href = `/login?message=${encodeURIComponent(message)}`;
    }
  }
}

/**
 * Global fetch wrapper that:
 * 1. Attaches Authorization Bearer token header from localStorage
 * 2. On 401 Unauthorized, automatically calls /api/Auth/RefreshToken and retries request
 * 3. If refresh token fails, forwards to /login with "Login first"
 * 4. On 403 Forbidden, raises "Status 403: User is not permitted."
 */
export async function authenticatedFetch(
  input: string,
  init?: RequestInit,
  isRetry = false
): Promise<Response> {
  const url = getApiUrl(input) || input;
  const token = getAuthToken();

  const headers = new Headers(init?.headers || {});
  if (token && !headers.has("Authorization")) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const mergedInit: RequestInit = {
    ...init,
    headers,
    credentials: "include",
  };

  const response = await fetch(url, mergedInit);

  // 1. Handle 401 Unauthorized -> Try RefreshToken
  if (response.status === 401) {
    if (!isRetry) {
      if (!isRefreshing) {
        isRefreshing = true;
        const newToken = await tryRefreshToken();
        isRefreshing = false;

        if (newToken) {
          onRefreshed(newToken);
          headers.set("Authorization", `Bearer ${newToken}`);
          return await authenticatedFetch(input, { ...mergedInit, headers }, true);
        } else {
          onRefreshed(null);
          redirectToLogin("Login first");
          throw new Error("Status 401: Login first");
        }
      } else {
        return new Promise<Response>((resolve, reject) => {
          addRefreshSubscriber((newToken) => {
            if (newToken) {
              headers.set("Authorization", `Bearer ${newToken}`);
              authenticatedFetch(input, { ...mergedInit, headers }, true).then(resolve).catch(reject);
            } else {
              redirectToLogin("Login first");
              reject(new Error("Status 401: Login first"));
            }
          });
        });
      }
    } else {
      // Retried request still got 401 -> Refresh token expired/invalid
      redirectToLogin("Login first");
      throw new Error("Status 401: Login first");
    }
  }

  // 2. Handle 403 Forbidden
  if (response.status === 403) {
    throw new Error("Status 403: User is not permitted.");
  }

  return response;
}
