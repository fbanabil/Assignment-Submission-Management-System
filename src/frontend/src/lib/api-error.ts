/**
 * Formats non-OK fetch API responses into clean human-readable error messages
 * including status codes (e.g. "Status 400: Email is required.")
 */
export async function parseApiResponseError(response: Response): Promise<string> {
  const status = response.status;
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
  const baseUrl = (
    process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
    process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ??
    ""
  );

  if (!baseUrl) return "";

  let cleanPath = path.startsWith("/") ? path : `/${path}`;

  if (baseUrl.toLowerCase().endsWith("/api") && cleanPath.toLowerCase().startsWith("/api/")) {
    cleanPath = cleanPath.substring(4);
  }

  return `${baseUrl}${cleanPath}`;
}
