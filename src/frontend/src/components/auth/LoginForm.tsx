"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { getApiUrl, parseApiResponseError, safeParseJson } from "@/lib/api-error";
import { getUserRole, setAuthToken } from "@/lib/auth";

export function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [infoMessage, setInfoMessage] = useState<string | null>(null);

  useEffect(() => {
    const msg = searchParams.get("message");
    if (msg) {
      setInfoMessage(msg);
    }
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim() || !password) {
      setError("Please enter both email and password.");
      return;
    }

    setLoading(true);
    setError(null);

    const loginUrl = getApiUrl("/Auth/Login");

    try {
      if (!loginUrl) {
        setError("API Base URL is not configured in environment variables.");
        return;
      }

      const response = await fetch(loginUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ email: email.trim(), password }),
      });

      if (!response.ok) {
        const errMessage = await parseApiResponseError(response);
        setError(errMessage);
        return;
      }

      const data = await safeParseJson<{ token: string }>(response);
      if (!data || !data.token) {
        setError("Invalid response format from authentication server.");
        return;
      }

      setAuthToken(data.token);

      const role = getUserRole(data.token);
      if (role === "Admin") {
        router.push("/admin");
      } else if (role === "Teacher") {
        router.push("/teacher");
      } else if (role === "Student") {
        router.push("/student");
      } else {
        router.push("/admin");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to connect to authentication server.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen flex-col justify-center px-4 py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <div className="text-center">
          <div className="mx-auto inline-flex items-center gap-2 rounded-full border border-teal-500/20 bg-teal-500/10 px-4 py-1 text-xs font-semibold uppercase tracking-[0.2em] text-teal-700">
            Assignment System
          </div>
          <h2 className="mt-4 text-3xl font-extrabold tracking-tight text-slate-900 sm:text-4xl">
            Account Login
          </h2>
          <p className="mt-2 text-sm text-slate-500">
            Enter your credentials to access your dashboard
          </p>
        </div>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="rounded-3xl border border-white/80 bg-white/90 p-8 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:p-10">
          {infoMessage && (
            <div className="mb-6 rounded-2xl border border-amber-200 bg-amber-50 p-4 text-xs font-semibold text-amber-800 flex items-center justify-between">
              <span>⚠️ {infoMessage}</span>
              <button onClick={() => setInfoMessage(null)} className="text-amber-600 hover:text-amber-900">
                ✕
              </button>
            </div>
          )}

          {error && (
            <div className="mb-6 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-semibold text-rose-700">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Email Address
              </label>
              <input
                type="email"
                required
                placeholder="user@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/50 px-4 py-3 text-sm font-medium text-slate-900 placeholder:text-slate-400 focus:border-teal-500 focus:bg-white focus:outline-none transition shadow-2xs"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Password
              </label>
              <input
                type="password"
                required
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/50 px-4 py-3 text-sm font-medium text-slate-900 placeholder:text-slate-400 focus:border-teal-500 focus:bg-white focus:outline-none transition shadow-2xs"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full rounded-full bg-slate-900 px-6 py-3.5 text-sm font-semibold text-white shadow-md hover:bg-slate-800 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              {loading ? "Authenticating..." : "Sign In to Account"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
