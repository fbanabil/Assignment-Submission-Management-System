import { authenticatedFetch, getApiUrl } from "./api-error";

/**
 * LocalStorage JWT token management & helper utilities
 */

const TOKEN_KEY = "authToken";

export function getAuthToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function setAuthToken(token: string): void {
  if (typeof window !== "undefined") {
    localStorage.setItem(TOKEN_KEY, token);
  }
}

export function removeAuthToken(): void {
  if (typeof window !== "undefined") {
    localStorage.removeItem(TOKEN_KEY);
  }
}

export type JwtPayload = {
  sub?: string;
  name?: string;
  unique_name?: string;
  email?: string;
  role?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string;
  exp?: number;
};

export function parseJwt(token: string): JwtPayload | null {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) return null;
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

export function getUserRole(token: string | null): string | null {
  if (!token) return null;
  const payload = parseJwt(token);
  if (!payload) return null;
  return (
    payload.role ||
    payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
    null
  );
}

/**
 * Calls /api/logout, removes local auth token, and redirects to /login
 */
export async function logoutUser(): Promise<void> {
  const logoutUrl = getApiUrl("/logout") || getApiUrl("/Logout") || getApiUrl("/Auth/Logout");
  try {
    if (logoutUrl) {
      await authenticatedFetch(logoutUrl, { method: "POST" });
    }
  } catch {
    // Ignore server/network errors during logout
  } finally {
    removeAuthToken();
    if (typeof window !== "undefined") {
      window.location.href = "/login?message=" + encodeURIComponent("Logged out successfully");
    }
  }
}
