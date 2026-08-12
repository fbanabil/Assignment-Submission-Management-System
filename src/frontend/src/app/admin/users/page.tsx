import type { Metadata } from "next";
import Link from "next/link";

import { getAdminUsersSnapshot } from "@/lib/admin-users";

export const metadata: Metadata = {
  title: "User Management | Assignment Manager",
  description: "Browse the admin user list and review account activity.",
};

export const dynamic = "force-dynamic";

function formatDateTime(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function formatValue(value: unknown) {
  if (value === null || value === undefined || value === "") {
    return "—";
  }

  if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
    return `${value}`;
  }

  return JSON.stringify(value);
}

function statusTone(status: string | undefined) {
  const normalized = `${status ?? ""}`.toLowerCase();

  if (normalized.includes("active") || normalized.includes("enabled")) {
    return "border-emerald-500/15 bg-emerald-500/10 text-emerald-700";
  }

  if (normalized.includes("pending") || normalized.includes("invited")) {
    return "border-amber-500/15 bg-amber-500/10 text-amber-700";
  }

  return "border-slate-500/15 bg-slate-500/10 text-slate-700";
}

export default async function UserManagementPage() {
  let snapshot = null as Awaited<ReturnType<typeof getAdminUsersSnapshot>> | null;
  let errorMessage: string | null = null;

  try {
    snapshot = await getAdminUsersSnapshot();
  } catch (error) {
    errorMessage = error instanceof Error ? error.message : "Unable to load user management data.";
  }

  if (errorMessage || !snapshot) {
    return (
      <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto flex w-full max-w-3xl flex-col gap-4 rounded-4xl border border-rose-200 bg-white/90 p-8 shadow-[0_16px_50px_rgba(15,23,42,0.08)]">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-rose-600">User management error</p>
          <h1 className="text-2xl font-semibold text-foreground">Unable to load users</h1>
          <p className="text-sm leading-6 text-(--color-muted)">{errorMessage ?? "Unable to load user management data."}</p>
          <Link className="w-fit rounded-full bg-foreground px-4 py-2 text-sm font-medium text-background transition hover:opacity-90" href="/admin">
            Back to dashboard
          </Link>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                User Management
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">Admin users</h1>
                <p className="mt-3 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  Review account status, roles, and recent activity from the user management endpoint.
                </p>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-3 text-sm">
              <Link className="rounded-full border border-black/10 bg-white px-4 py-2 font-medium text-foreground shadow-sm transition hover:border-black/20 hover:bg-black/2" href="/admin">
                Back to dashboard
              </Link>
              <span className="rounded-full border border-black/5 bg-white px-4 py-2 font-medium text-foreground shadow-sm">
                Source: {snapshot.dataSource}
              </span>
              <span className="rounded-full border border-black/5 bg-white px-4 py-2 font-medium text-foreground shadow-sm">
                Refreshed {formatDateTime(snapshot.fetchedAt)}
              </span>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <p className="text-sm font-medium text-(--color-muted)">Total users</p>
            <p className="mt-2 text-4xl font-semibold tracking-tight text-foreground">{snapshot.totalUsers}</p>
          </div>
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <p className="text-sm font-medium text-(--color-muted)">Active users</p>
            <p className="mt-2 text-4xl font-semibold tracking-tight text-foreground">{snapshot.activeUsers}</p>
          </div>
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <p className="text-sm font-medium text-(--color-muted)">Inactive users</p>
            <p className="mt-2 text-4xl font-semibold tracking-tight text-foreground">{snapshot.inactiveUsers}</p>
          </div>
        </section>

        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex items-center justify-between gap-4">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">Users</h2>
              <p className="mt-1 text-sm text-(--color-muted)">The first pass displays the structured user list returned by the API.</p>
            </div>
          </div>

          <div className="mt-6 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-sm">
            <div className="max-h-[36rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3 font-semibold">Name</th>
                    <th className="border-b border-black/5 px-4 py-3 font-semibold">Email</th>
                    <th className="border-b border-black/5 px-4 py-3 font-semibold">Role</th>
                    <th className="border-b border-black/5 px-4 py-3 font-semibold">Status</th>
                    <th className="border-b border-black/5 px-4 py-3 font-semibold">Created</th>
                    <th className="border-b border-black/5 px-4 py-3 font-semibold">Last login</th>
                  </tr>
                </thead>
                <tbody>
                  {snapshot.users.map((user, index) => (
                    <tr key={String(user.id ?? user.email ?? index)} className="odd:bg-white even:bg-slate-50/70">
                      <td className="border-b border-black/5 px-4 py-4 font-medium text-foreground">{formatValue(user.name ?? user.fullName ?? user.displayName)}</td>
                      <td className="border-b border-black/5 px-4 py-4 text-(--color-muted)">{formatValue(user.email)}</td>
                      <td className="border-b border-black/5 px-4 py-4 text-(--color-muted)">{formatValue(user.role)}</td>
                      <td className="border-b border-black/5 px-4 py-4">
                        <span className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] ${statusTone(user.status)}`}>
                          {formatValue(user.status)}
                        </span>
                      </td>
                      <td className="border-b border-black/5 px-4 py-4 text-(--color-muted)">{formatValue(user.createdAt ? formatDateTime(user.createdAt) : null)}</td>
                      <td className="border-b border-black/5 px-4 py-4 text-(--color-muted)">{formatValue(user.lastLoginAt ? formatDateTime(user.lastLoginAt) : null)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </section>
      </div>
    </main>
  );
}