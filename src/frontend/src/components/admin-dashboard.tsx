"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import type { ReactNode } from "react";

import { getDashboardSummarySnapshot, type DashboardSummaryDto } from "@/lib/admin-dashboard";

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
        <div className="mx-auto flex w-full max-w-3xl flex-col gap-4 rounded-4xl border border-rose-200 bg-white/90 p-8 shadow-[0_16px_50px_rgba(15,23,42,0.08)]">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-rose-600">Dashboard error</p>
          <h1 className="text-2xl font-semibold text-foreground">Unable to load admin summary</h1>
          <p className="text-sm leading-6 text-(--color-muted)">{error}</p>
          <p className="text-sm leading-6 text-(--color-muted)">
            Check that <span className="font-semibold text-foreground">NEXT_PUBLIC_API_BASE_URL</span> points to your backend and that the endpoint responds from the browser.
          </p>
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
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Admin dashboard
              </div>
              <h1 className="mt-4 text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                System-wide control center for the assignment platform.
              </h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                Monitor user growth, keep assignment throughput healthy, and track submission volume from a single
                operational view.
              </p>
            </div>

            <div className="flex flex-wrap items-center gap-3 text-sm">
              <span className="rounded-full border border-black/5 bg-white px-4 py-2 font-medium text-foreground shadow-sm">
                Source: {snapshot.dataSource}
              </span>
              <span className="rounded-full border border-black/5 bg-white px-4 py-2 font-medium text-foreground shadow-sm">
                Refreshed {new Intl.DateTimeFormat("en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(snapshot.fetchedAt))}
              </span>
            </div>
          </div>

          <nav className="mt-6 flex flex-wrap gap-3 border-t border-black/5 pt-5 text-sm font-medium">
            <Link className="rounded-full bg-foreground px-4 py-2 text-background transition hover:opacity-90" href="#overview">
              Overview
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2" href="/admin/users">
              User Management
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2" href="/admin/classes">
              Class Management
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2" href="/admin/subjects">
              Subject Management
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2" href="/admin/teacher-assignments">
              Teacher Assignments
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2" href="/admin/assignments">
              Assignments
            </Link>
            <Link className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2" href="/admin/submissions">
              Submissions
            </Link>
          </nav>
        </header>

        <Section
          id="overview"
          title="Overview"
          description="Three high-signal metrics that show how the system is moving right now."
        >
          <div className="grid gap-4 lg:grid-cols-3">
            <StatCard
              label="Total users"
              value={totalUsers}
              helper={`${formatNumber(snapshot.users.activeUsers)} active, ${formatNumber(snapshot.users.inactiveUsers)} inactive`}
              tone="teal"
            />
            <StatCard
              label="Active assignments"
              value={activeAssignments}
              helper={`${formatNumber(snapshot.assignments.dueSoonAssignments)} are due soon`}
              tone="blue"
            />
            <StatCard
              label="Submission volume"
              value={totalSubmissions}
              helper={`${formatNumber(snapshot.submissions.submittedToday)} submitted today`}
              tone="amber"
            />
          </div>
        </Section>

        <div className="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
          <Section
            id="users"
            title="User health"
            description="Role distribution and recent onboarding activity."
          >
            <div className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="rounded-2xl border border-black/5 bg-white/80 p-4 shadow-sm">
                  <p className="text-sm text-(--color-muted)">New users this month</p>
                  <p className="mt-2 text-3xl font-semibold text-foreground">{formatNumber(snapshot.users.newUsersThisMonth)}</p>
                </div>
                <div className="rounded-2xl border border-black/5 bg-white/80 p-4 shadow-sm">
                  <p className="text-sm text-(--color-muted)">Activation rate</p>
                  <p className="mt-2 text-3xl font-semibold text-foreground">
                    {formatPercent((snapshot.users.activeUsers / Math.max(snapshot.users.totalUsers, 1)) * 100)}
                  </p>
                </div>
              </div>

              <div className="mt-6 space-y-3">
                {snapshot.users.roleBreakdown.map((item) => (
                  <div key={item.role} className="flex items-center justify-between rounded-2xl border border-black/5 bg-white/70 px-4 py-3">
                    <div>
                      <p className="font-medium text-foreground">{item.role}</p>
                      <p className="text-sm text-(--color-muted)">{formatNumber(item.count)} accounts</p>
                    </div>
                    <span className="text-lg font-semibold text-foreground">{formatNumber(item.count)}</span>
                  </div>
                ))}
              </div>
            </div>
          </Section>

          <Section
            id="assignments"
            title="Assignment pipeline"
            description="Assignment status mix and the completion trend at a glance."
          >
            <div className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
              <div className="flex items-center justify-between gap-4 rounded-2xl border border-black/5 bg-white/80 p-4 shadow-sm">
                <div>
                  <p className="text-sm text-(--color-muted)">Completion rate</p>
                  <p className="mt-1 text-3xl font-semibold text-foreground">{formatPercent(snapshot.assignments.completionRate)}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm text-(--color-muted)">Drafts</p>
                  <p className="mt-1 text-2xl font-semibold text-foreground">{formatNumber(snapshot.assignments.draftAssignments)}</p>
                </div>
              </div>

              <div className="mt-6 space-y-3">
                {snapshot.assignments.statusBreakdown.map((item) => (
                  <div key={item.status} className="rounded-2xl border border-black/5 bg-white/70 px-4 py-3 shadow-sm">
                    <div className="flex items-center justify-between gap-4">
                      <div>
                        <p className="font-medium text-foreground">{item.status}</p>
                        <p className="text-sm text-(--color-muted)">Assignments in this state</p>
                      </div>
                      <span className="text-lg font-semibold text-foreground">{formatNumber(item.count)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Section>
        </div>

        <Section
          id="submissions"
          title="Submission volume"
          description="A simple week view for submission throughput and grading workload."
        >
          <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
            <div className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
              <BarChart items={snapshot.submissions.weeklyVolumes} />
            </div>

            <div className="grid gap-4">
              <div className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
                <p className="text-sm text-(--color-muted)">Pending review</p>
                <p className="mt-2 text-4xl font-semibold text-foreground">{formatNumber(snapshot.submissions.pendingReview)}</p>
                <p className="mt-2 text-sm leading-6 text-(--color-muted)">
                  Submissions that still need a teacher response.
                </p>
              </div>

              <div className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
                <p className="text-sm text-(--color-muted)">Graded submissions</p>
                <p className="mt-2 text-4xl font-semibold text-foreground">{formatNumber(snapshot.submissions.gradedSubmissions)}</p>
                <p className="mt-2 text-sm leading-6 text-(--color-muted)">
                  Reviewed work already closed out by teachers.
                </p>
              </div>
            </div>
          </div>
        </Section>
      </div>
    </main>
  );
}