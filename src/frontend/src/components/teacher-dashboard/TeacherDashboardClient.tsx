"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getTeacherDashboard,
  type TeacherDashboardResponseDto,
} from "@/lib/teacher-dashboard";
import { logoutUser } from "@/lib/auth";
import { formatDisplayError } from "@/lib/api-error";

function formatDateTime(value?: string) {
  if (!value) return "N/A";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function getDaysRemaining(value?: string) {
  if (!value) return "";
  const date = new Date(value);
  const now = new Date();
  const diffMs = date.getTime() - now.getTime();
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));

  if (diffDays < 0) return "Past Due";
  if (diffDays === 0) return "Due Today";
  if (diffDays === 1) return "Due Tomorrow";
  return `Due in ${diffDays} days`;
}

export function TeacherDashboardClient() {
  const [data, setData] = useState<TeacherDashboardResponseDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDashboard = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getTeacherDashboard();
      setData(res);
    } catch (err) {
      console.error("Failed to load teacher dashboard:", err);
      setError(formatDisplayError(err, "Unable to load teacher dashboard data."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Header section styled matching modern dashboard design */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="max-w-xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Teacher Portal
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Welcome back, {data?.teacherName || "Teacher"}!
                </h1>
                <p className="mt-2 text-sm leading-7 text-(--color-muted) sm:text-base">
                  Track your classes, assignments, student submissions, and manage academic rosters.
                </p>
              </div>
            </div>

            {/* Quick Portal Navigation Links */}
            <nav className="flex flex-wrap items-center gap-1.5 sm:gap-2 text-xs sm:text-sm font-medium shrink-0">
              <Link
                className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
                href="/teacher"
              >
                Dashboard
              </Link>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/teacher/classes"
              >
                My Classes
              </Link>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/teacher/assignments"
              >
                Assignments
              </Link>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/teacher/submissions"
              >
                Submissions
              </Link>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/teacher/enrollments"
              >
                Enrollments
              </Link>
              <button
                onClick={() => logoutUser()}
                className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
              >
                Logout 🚪
              </button>
            </nav>
          </div>

          <div className="mt-6 flex flex-wrap items-center justify-between border-t border-black/5 pt-4 text-xs font-medium text-slate-500 gap-2">
            <span>
              Teacher Account: <strong className="text-foreground">{data?.teacherEmail || "teacher@school.edu"}</strong>
            </span>
            <span>
              Refreshed: <strong className="text-foreground">{formatDateTime(data?.fetchedAt)}</strong>
            </span>
          </div>
        </header>

        {/* Error Notification */}
        {error && (
          <div className="rounded-3xl border border-rose-200 bg-rose-50/90 p-4 shadow-sm text-rose-700">
            <p className="text-sm font-semibold">{error}</p>
          </div>
        )}

        {/* Stats Overview Grid */}
        <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {/* Assigned Classes */}
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-400">Assigned Classes</p>
            <div className="mt-3 flex items-baseline justify-between">
              <span className="text-4xl font-extrabold text-foreground">
                {loading ? "..." : data?.totalAssignedClasses ?? 0}
              </span>
              <span className="rounded-full bg-teal-100 px-3 py-1 text-xs font-semibold text-teal-800">
                Active Courses
              </span>
            </div>
            <p className="mt-2 text-xs text-slate-500">Classes & subject pairs assigned to you</p>
          </div>

          {/* Active Assignments */}
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-400">Active Assignments</p>
            <div className="mt-3 flex items-baseline justify-between">
              <span className="text-4xl font-extrabold text-foreground">
                {loading ? "..." : data?.activeAssignmentsCount ?? 0}
              </span>
              <span className="rounded-full bg-purple-100 px-3 py-1 text-xs font-semibold text-purple-800">
                Published
              </span>
            </div>
            <p className="mt-2 text-xs text-slate-500">Currently open for student submissions</p>
          </div>

          {/* Ungraded Count Highlight Card */}
          <div className="rounded-3xl border border-amber-300/60 bg-amber-50/70 p-6 shadow-[0_16px_50px_rgba(245,158,11,0.12)] backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-wider text-amber-800">Ungraded Submissions</p>
            <div className="mt-3 flex items-baseline justify-between">
              <span className="text-4xl font-extrabold text-amber-900">
                {loading ? "..." : data?.ungradedSubmissionsCount ?? 0}
              </span>
              <span className="rounded-full bg-amber-200/80 px-3 py-1 text-xs font-bold text-amber-900">
                Action Needed
              </span>
            </div>
            <p className="mt-2 text-xs font-medium text-amber-700">Submissions awaiting score & feedback</p>
          </div>

          {/* Upcoming Deadlines */}
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-400">Upcoming Deadlines</p>
            <div className="mt-3 flex items-baseline justify-between">
              <span className="text-4xl font-extrabold text-foreground">
                {loading ? "..." : data?.upcomingDeadlinesCount ?? 0}
              </span>
              <span className="rounded-full bg-blue-100 px-3 py-1 text-xs font-semibold text-blue-800">
                Next 14 Days
              </span>
            </div>
            <p className="mt-2 text-xs text-slate-500">Assignments due soon</p>
          </div>
        </section>

        {/* Two-Column Main Content: Assigned Classes & Upcoming Deadlines */}
        <div className="grid gap-6 lg:grid-cols-12">
          {/* Assigned Classes / Subjects Section */}
          <section className="lg:col-span-5 rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between pb-4 border-b border-black/5">
                <div>
                  <h2 className="text-xl font-semibold tracking-tight text-foreground">Assigned Classes & Subjects</h2>
                  <p className="text-xs text-(--color-muted) mt-0.5">Classes and subjects currently taught by you</p>
                </div>
                <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700">
                  {data?.assignedClasses.length ?? 0} Courses
                </span>
              </div>

              <div className="mt-4 space-y-3 max-h-[32rem] overflow-auto pr-1">
                {loading ? (
                  Array.from({ length: 3 }).map((_, idx) => (
                    <div key={`skel-cls-${idx}`} className="animate-pulse rounded-2xl border border-slate-200 p-4 bg-white/60">
                      <div className="h-4 w-3/4 rounded-full bg-slate-200"></div>
                      <div className="mt-2 h-3 w-1/2 rounded-full bg-slate-200"></div>
                    </div>
                  ))
                ) : data && data.assignedClasses.length > 0 ? (
                  data.assignedClasses.map((cls) => (
                    <div
                      key={cls.classSubjectId}
                      className="rounded-2xl border border-slate-200/80 bg-white/90 p-4 shadow-2xs hover:border-teal-500/40 hover:shadow-md transition group"
                    >
                      <div className="flex items-start justify-between">
                        <div>
                          <h3 className="font-semibold text-foreground group-hover:text-teal-700 transition">
                            {cls.className}
                          </h3>
                          <p className="text-xs text-slate-500 mt-0.5">
                            {cls.classSection} ({cls.academicYear})
                          </p>
                        </div>
                        <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-2.5 py-0.5 text-xs font-mono font-bold text-purple-700">
                          {cls.subjectCode}
                        </span>
                      </div>

                      <div className="mt-3 flex items-center justify-between border-t border-black/5 pt-3 text-xs">
                        <span className="text-slate-600 font-medium">{cls.subjectName}</span>
                        <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-slate-700 font-semibold">
                          👥 {cls.studentCount} Students
                        </span>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="p-8 text-center text-slate-500 text-sm">
                    No classes or subjects assigned yet. Contact your administrator.
                  </div>
                )}
              </div>
            </div>

            <div className="mt-4 pt-4 border-t border-black/5 text-right">
              <Link
                href="/teacher/classes"
                className="inline-flex items-center gap-1.5 text-xs font-semibold text-teal-700 hover:text-teal-900 transition"
              >
                View All Assigned Classes →
              </Link>
            </div>
          </section>

          {/* Upcoming Deadlines & Ungraded Submissions Section */}
          <section className="lg:col-span-7 rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between pb-4 border-b border-black/5">
                <div>
                  <h2 className="text-xl font-semibold tracking-tight text-foreground">Upcoming Deadlines & Grading</h2>
                  <p className="text-xs text-(--color-muted) mt-0.5">Assignments due soon with pending ungraded submissions</p>
                </div>
                <span className="rounded-full bg-rose-100 px-3 py-1 text-xs font-semibold text-rose-800">
                  {data?.upcomingDeadlines.length ?? 0} Upcoming
                </span>
              </div>

              <div className="mt-4 space-y-3 max-h-[32rem] overflow-auto pr-1">
                {loading ? (
                  Array.from({ length: 3 }).map((_, idx) => (
                    <div key={`skel-dl-${idx}`} className="animate-pulse rounded-2xl border border-slate-200 p-4 bg-white/60">
                      <div className="h-4 w-3/4 rounded-full bg-slate-200"></div>
                      <div className="mt-2 h-3 w-1/3 rounded-full bg-slate-200"></div>
                    </div>
                  ))
                ) : data && data.upcomingDeadlines.length > 0 ? (
                  data.upcomingDeadlines.map((dl) => (
                    <div
                      key={dl.assignmentId}
                      className="rounded-2xl border border-slate-200/80 bg-white/90 p-4 shadow-2xs hover:border-slate-400 transition flex flex-col sm:flex-row sm:items-center justify-between gap-4"
                    >
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <h3 className="font-semibold text-foreground leading-tight">{dl.title}</h3>
                          <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-2 py-0.2 text-[10px] font-mono font-semibold text-purple-700">
                            {dl.subjectCode}
                          </span>
                        </div>
                        <p className="text-xs text-slate-500">{dl.className}</p>
                        <p className="text-xs font-semibold text-rose-600">
                          📅 {formatDateTime(dl.dueDate)} ({getDaysRemaining(dl.dueDate)})
                        </p>
                      </div>

                      <div className="flex sm:flex-col items-end justify-between gap-2 border-t sm:border-t-0 border-black/5 pt-2 sm:pt-0">
                        <div className="text-right">
                          <span className="text-xs text-slate-500 block">Submissions: {dl.totalSubmissions}</span>
                          {dl.ungradedSubmissions > 0 ? (
                            <span className="inline-flex rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-bold text-amber-800 mt-0.5">
                              {dl.ungradedSubmissions} Ungraded
                            </span>
                          ) : (
                            <span className="inline-flex rounded-full bg-emerald-100 px-2.5 py-0.5 text-xs font-semibold text-emerald-800 mt-0.5">
                              All Graded ✓
                            </span>
                          )}
                        </div>

                        <Link
                          href={`/teacher/submissions?assignmentId=${dl.assignmentId}`}
                          className="rounded-full border border-slate-300 bg-white px-3.5 py-1 text-xs font-medium text-slate-800 hover:bg-slate-900 hover:text-white transition shadow-2xs"
                        >
                          Grade Submissions →
                        </Link>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="p-8 text-center text-slate-500 text-sm">
                    No upcoming deadlines found.
                  </div>
                )}
              </div>
            </div>

            <div className="mt-4 pt-4 border-t border-black/5 flex items-center justify-between text-xs">
              <span className="text-slate-500">
                Total Ungraded Items: <strong className="text-amber-800 font-semibold">{data?.ungradedSubmissionsCount ?? 0}</strong>
              </span>
              <Link
                href="/teacher/submissions"
                className="font-semibold text-teal-700 hover:text-teal-900 transition"
              >
                Go to Submissions & Grading →
              </Link>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}
