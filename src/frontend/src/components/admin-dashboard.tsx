"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import type { ReactNode } from "react";

import { getDashboardSummarySnapshot, type DashboardSummaryDto } from "@/lib/admin-dashboard";
import { logoutUser } from "@/lib/auth";

type StatCardProps = {
  label: string;
  value: string;
  helper: string;
  tone: "teal" | "blue" | "amber";
};

type SectionProps = {
  id: string;
  title: string;
  description: string;
  children: ReactNode;
};

function formatNumber(value: number) {
  return new Intl.NumberFormat("en-US").format(value);
}

function formatPercent(value: number) {
  return `${Math.round(value)}%`;
}

function toneClasses(tone: StatCardProps["tone"]) {
  switch (tone) {
    case "blue":
      return "from-blue-500/15 to-sky-500/10 border-blue-500/15";
    case "amber":
      return "from-amber-500/15 to-orange-500/10 border-amber-500/15";
    default:
      return "from-teal-500/15 to-emerald-500/10 border-teal-500/15";
  }
}

function StatCard({ label, value, helper, tone }: StatCardProps) {
  return (
    <article className={`rounded-3xl border bg-(--color-surface) p-5 shadow-[0_12px_40px_rgba(15,23,42,0.08)] backdrop-blur ${toneClasses(tone)}`}>
      <p className="text-sm font-medium text-(--color-muted)">{label}</p>
      <div className="mt-3 flex items-end justify-between gap-4">
        <span className="text-4xl font-semibold tracking-tight text-foreground">{value}</span>
        <span className="rounded-full border border-black/5 bg-white/80 px-3 py-1 text-xs font-medium text-foreground/80 shadow-sm">
          Live overview
        </span>
      </div>
      <p className="mt-3 text-sm leading-6 text-(--color-muted)">{helper}</p>
    </article>
  );
}

function Section({ id, title, description, children }: SectionProps) {
  return (
    <section id={id} className="scroll-mt-24">
      <div className="mb-4 flex items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold tracking-tight text-foreground">{title}</h2>
          <p className="mt-1 text-sm text-(--color-muted)">{description}</p>
        </div>
      </div>
      {children}
    </section>
  );
}

function BarChart({ items }: { items: { label: string; count: number }[] }) {
  const max = Math.max(...items.map((item) => item.count), 1);

  return (
    <div className="grid grid-cols-7 gap-3 sm:gap-4">
      {items.map((item, index) => (
        <div key={`${item.label}-${index}`} className="flex flex-col items-center gap-2">
          <div className="flex h-40 w-full items-end rounded-2xl border border-black/5 bg-white/80 p-2 shadow-sm">
            <div
              className="w-full rounded-xl bg-linear-to-t from-teal-500 via-cyan-400 to-blue-400"
              style={{ height: `${Math.max((item.count / max) * 100, 12)}%` }}
              aria-label={`${item.label}: ${item.count}`}
            />
          </div>
          <span className="text-xs font-medium text-(--color-muted)">{item.label}</span>
          <span className="text-sm font-semibold text-foreground">{item.count}</span>
        </div>
      ))}
    </div>
  );
}

export function DashboardSummary() {
  const [snapshot, setSnapshot] = useState<DashboardSummaryDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    async function loadSnapshot() {
      try {
        const data = await getDashboardSummarySnapshot();

        if (active) {
          setSnapshot(data);
          console.log(data);
          setError(null);
        }
      } catch (loadError) {
        if (active) {
          console.error(loadError);
          setError(loadError instanceof Error ? loadError.message : "Unable to load dashboard data.");
        }
      }
    }

    loadSnapshot();

    return () => {
      active = false;
    };
  }, []);

  if (error) {
    return (
      <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto flex w-full max-w-3xl flex-col gap-3 rounded-3xl border border-rose-200 bg-rose-50/90 p-6 shadow-sm text-rose-700">
          <p className="text-sm font-semibold">{error}</p>
        </div>
      </main>
    );
  }

  if (!snapshot) {
    return (
      <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
          <div className="h-56 animate-pulse rounded-4xl border border-white/70 bg-white/70 shadow-[0_16px_50px_rgba(15,23,42,0.08)]" />
          <div className="grid gap-4 lg:grid-cols-3">
            <div className="h-40 animate-pulse rounded-3xl border border-white/70 bg-white/70" />
            <div className="h-40 animate-pulse rounded-3xl border border-white/70 bg-white/70" />
            <div className="h-40 animate-pulse rounded-3xl border border-white/70 bg-white/70" />
          </div>
        </div>
      </main>
    );
  }

  const totalUsers = formatNumber(snapshot.users.totalUsers);
  const activeAssignments = formatNumber(snapshot.assignments.activeAssignments);
  const totalSubmissions = formatNumber(snapshot.submissions.totalSubmissions);

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Hero Header Section */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="max-w-2xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/20 bg-teal-500/10 px-3.5 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                <span className="h-2 w-2 rounded-full bg-teal-500 animate-pulse"></span>
                System Administration
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Platform Control Center
                </h1>
                <p className="mt-2 text-sm leading-7 text-(--color-muted) sm:text-base">
                  Real-time operational dashboard for system-wide users, course allocations, assignments, and submission metrics.
                </p>
              </div>
            </div>

            {/* Quick Status Info */}
            <div className="flex flex-wrap items-center gap-3 text-xs font-medium text-slate-500 shrink-0">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-black/5 bg-white px-3.5 py-1.5 font-medium text-foreground shadow-2xs">
                System Status: <strong className="text-emerald-600 font-semibold">Active 🟢</strong>
              </span>
              <span className="inline-flex items-center gap-1.5 rounded-full border border-black/5 bg-white px-3.5 py-1.5 font-medium text-foreground shadow-2xs">
                Refreshed: <strong className="text-foreground">{new Intl.DateTimeFormat("en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(snapshot.fetchedAt))}</strong>
              </span>
            </div>
          </div>

          {/* Persistent Admin Portal Navigation */}
          <nav className="mt-6 flex flex-wrap items-center gap-1.5 sm:gap-2 border-t border-black/5 pt-5 text-xs sm:text-sm font-medium shrink-0">
            <Link className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap" href="/admin">
              Dashboard
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap" href="/admin/users">
              User Management
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap" href="/admin/classes">
              Class Management
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap" href="/admin/subjects">
              Subject Management
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap" href="/admin/teacher-assignments">
              Teacher Assignments
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap" href="/admin/assignments">
              All Assignments
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap" href="/admin/submissions">
              All Submissions
            </Link>
            <button
              onClick={() => logoutUser()}
              className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
            >
              Logout 🚪
            </button>
          </nav>
        </header>

        {/* Primary Metric Overview Cards */}
        <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-3xl border border-teal-200/60 bg-linear-to-br from-teal-50/80 to-emerald-50/40 p-6 shadow-[0_16px_50px_rgba(20,184,166,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <p className="text-xs font-semibold uppercase tracking-wider text-teal-800">Total Users</p>
              <span className="rounded-full bg-teal-100 px-2.5 py-0.5 text-xs font-bold text-teal-800">Accounts</span>
            </div>
            <p className="mt-3 text-4xl font-extrabold text-teal-950">{totalUsers}</p>
            <p className="mt-2 text-xs font-medium text-teal-700">
              {formatNumber(snapshot.users.activeUsers)} active • {formatNumber(snapshot.users.inactiveUsers)} inactive
            </p>
          </div>

          <div className="rounded-3xl border border-indigo-200/60 bg-linear-to-br from-indigo-50/80 to-sky-50/40 p-6 shadow-[0_16px_50px_rgba(99,102,241,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <p className="text-xs font-semibold uppercase tracking-wider text-indigo-800">Active Assignments</p>
              <span className="rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-bold text-indigo-800">Published</span>
            </div>
            <p className="mt-3 text-4xl font-extrabold text-indigo-950">{activeAssignments}</p>
            <p className="mt-2 text-xs font-medium text-indigo-700">
              {formatNumber(snapshot.assignments.dueSoonAssignments)} due in next 14 days
            </p>
          </div>

          <div className="rounded-3xl border border-amber-200/60 bg-linear-to-br from-amber-50/80 to-orange-50/40 p-6 shadow-[0_16px_50px_rgba(245,158,11,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <p className="text-xs font-semibold uppercase tracking-wider text-amber-800">Submission Volume</p>
              <span className="rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-bold text-amber-800">Total Work</span>
            </div>
            <p className="mt-3 text-4xl font-extrabold text-amber-950">{totalSubmissions}</p>
            <p className="mt-2 text-xs font-medium text-amber-700">
              {formatNumber(snapshot.submissions.submittedToday)} turned in today
            </p>
          </div>

          <div className="rounded-3xl border border-purple-200/60 bg-linear-to-br from-purple-50/80 to-fuchsia-50/40 p-6 shadow-[0_16px_50px_rgba(168,85,247,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <p className="text-xs font-semibold uppercase tracking-wider text-purple-800">Pending Review</p>
              <span className="rounded-full bg-purple-100 px-2.5 py-0.5 text-xs font-bold text-purple-800">Ungraded</span>
            </div>
            <p className="mt-3 text-4xl font-extrabold text-purple-950">{formatNumber(snapshot.submissions.pendingReview)}</p>
            <p className="mt-2 text-xs font-medium text-purple-700">
              Awaiting teacher evaluation
            </p>
          </div>
        </section>

        {/* Detailed Insights Row: User Health & Assignment Pipeline */}
        <div className="grid gap-6 xl:grid-cols-2">
          {/* User Health Card */}
          <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between pb-4 border-b border-black/5">
                <div>
                  <h2 className="text-xl font-semibold tracking-tight text-foreground">User Directory Health</h2>
                  <p className="text-xs text-(--color-muted) mt-0.5">Role breakdown and onboarding activity</p>
                </div>
                <Link
                  href="/admin/users"
                  className="rounded-full border border-slate-200 bg-white px-3 py-1 text-xs font-semibold text-slate-700 hover:bg-slate-900 hover:text-white transition shadow-2xs"
                >
                  Manage Users →
                </Link>
              </div>

              <div className="mt-5 grid gap-4 sm:grid-cols-2">
                <div className="rounded-2xl border border-slate-200/80 bg-white/80 p-4 shadow-2xs">
                  <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">New This Month</span>
                  <span className="mt-2 block text-3xl font-bold text-slate-900">{formatNumber(snapshot.users.newUsersThisMonth)}</span>
                  <span className="mt-1 block text-xs text-slate-500">Newly registered user accounts</span>
                </div>
                <div className="rounded-2xl border border-slate-200/80 bg-white/80 p-4 shadow-2xs">
                  <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Activation Rate</span>
                  <span className="mt-2 block text-3xl font-bold text-emerald-700">
                    {formatPercent((snapshot.users.activeUsers / Math.max(snapshot.users.totalUsers, 1)) * 100)}
                  </span>
                  <span className="mt-1 block text-xs text-slate-500">Accounts with active status</span>
                </div>
              </div>

              <div className="mt-5 space-y-2.5">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500">Role Breakdown</h3>
                {snapshot.users.roleBreakdown.map((item) => {
                  const roleBadgeColor =
                    item.role === "Admin"
                      ? "border-rose-200 bg-rose-50 text-rose-700"
                      : item.role === "Teacher"
                      ? "border-purple-200 bg-purple-50 text-purple-700"
                      : "border-teal-200 bg-teal-50 text-teal-700";

                  return (
                    <div key={item.role} className="flex items-center justify-between rounded-2xl border border-slate-200/70 bg-white/90 px-4 py-3 shadow-2xs">
                      <div className="flex items-center gap-2.5">
                        <span className={`rounded-full border px-2.5 py-0.5 text-xs font-bold ${roleBadgeColor}`}>
                          {item.role}
                        </span>
                        <span className="text-xs text-slate-500 font-medium">Registered Accounts</span>
                      </div>
                      <span className="text-base font-bold text-slate-900">{formatNumber(item.count)}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          </section>

          {/* Assignment Pipeline Card */}
          <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between pb-4 border-b border-black/5">
                <div>
                  <h2 className="text-xl font-semibold tracking-tight text-foreground">Assignment Pipeline</h2>
                  <p className="text-xs text-(--color-muted) mt-0.5">Status distribution & overall completion metrics</p>
                </div>
                <Link
                  href="/admin/assignments"
                  className="rounded-full border border-slate-200 bg-white px-3 py-1 text-xs font-semibold text-slate-700 hover:bg-slate-900 hover:text-white transition shadow-2xs"
                >
                  All Assignments →
                </Link>
              </div>

              <div className="mt-5 grid gap-4 sm:grid-cols-2">
                <div className="rounded-2xl border border-slate-200/80 bg-white/80 p-4 shadow-2xs">
                  <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Completion Rate</span>
                  <span className="mt-2 block text-3xl font-bold text-indigo-700">{formatPercent(snapshot.assignments.completionRate)}</span>
                  <div className="mt-2 h-2 w-full rounded-full bg-slate-100 overflow-hidden">
                    <div
                      className="h-full bg-indigo-600 rounded-full"
                      style={{ width: `${Math.min(snapshot.assignments.completionRate, 100)}%` }}
                    />
                  </div>
                </div>
                <div className="rounded-2xl border border-slate-200/80 bg-white/80 p-4 shadow-2xs">
                  <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Draft Assignments</span>
                  <span className="mt-2 block text-3xl font-bold text-slate-700">{formatNumber(snapshot.assignments.draftAssignments)}</span>
                  <span className="mt-1 block text-xs text-slate-500">Unpublished draft coursework</span>
                </div>
              </div>

              <div className="mt-5 space-y-2.5">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500">Status Breakdown</h3>
                {snapshot.assignments.statusBreakdown.map((item) => (
                  <div key={item.status} className="flex items-center justify-between rounded-2xl border border-slate-200/70 bg-white/90 px-4 py-3 shadow-2xs">
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-slate-800 text-sm">{item.status}</span>
                      <span className="text-xs text-slate-500">Coursework Items</span>
                    </div>
                    <span className="text-base font-bold text-slate-900">{formatNumber(item.count)}</span>
                  </div>
                ))}
              </div>
            </div>
          </section>
        </div>

        {/* Submission Volume Chart Section */}
        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex items-center justify-between pb-4 border-b border-black/5 mb-6">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">Weekly Submission Volume</h2>
              <p className="text-xs text-(--color-muted) mt-0.5">Submission activity trend across recent days</p>
            </div>
            <Link
              href="/admin/submissions"
              className="rounded-full border border-slate-200 bg-white px-3.5 py-1.5 text-xs font-semibold text-slate-700 hover:bg-slate-900 hover:text-white transition shadow-2xs"
            >
              Manage All Submissions →
            </Link>
          </div>

          <div className="grid gap-6 xl:grid-cols-12 items-center">
            <div className="xl:col-span-8 rounded-3xl border border-slate-200/80 bg-white/80 p-6 shadow-2xs">
              <BarChart items={snapshot.submissions.weeklyVolumes} />
            </div>

            <div className="xl:col-span-4 flex flex-col gap-4">
              <div className="rounded-3xl border border-amber-200/80 bg-amber-50/60 p-5 shadow-2xs">
                <span className="text-xs font-semibold text-amber-800 uppercase tracking-wider">Ungraded Queue</span>
                <p className="mt-2 text-3xl font-extrabold text-amber-950">{formatNumber(snapshot.submissions.pendingReview)}</p>
                <p className="mt-1 text-xs text-amber-700">Submissions awaiting teacher grading & feedback</p>
              </div>

              <div className="rounded-3xl border border-emerald-200/80 bg-emerald-50/60 p-5 shadow-2xs">
                <span className="text-xs font-semibold text-emerald-800 uppercase tracking-wider">Completed Work</span>
                <p className="mt-2 text-3xl font-extrabold text-emerald-950">{formatNumber(snapshot.submissions.gradedSubmissions)}</p>
                <p className="mt-1 text-xs text-emerald-700">Evaluated student work with final scores</p>
              </div>
            </div>
          </div>
        </section>
      </div>
    </main>
  );
}