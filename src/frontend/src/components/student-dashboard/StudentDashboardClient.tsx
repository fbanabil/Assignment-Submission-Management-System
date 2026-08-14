"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getStudentDashboard,
  type StudentAssignmentDueDto,
  type StudentDashboardResponseDto,
  type StudentRecentGradeDto,
} from "@/lib/student-dashboard";
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

  if (diffDays < 0) return "Overdue";
  if (diffDays === 0) return "Due Today";
  if (diffDays === 1) return "Due Tomorrow";
  return `Due in ${diffDays} days`;
}

export function StudentDashboardClient() {
  const [data, setData] = useState<StudentDashboardResponseDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDashboard = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getStudentDashboard();
      setData(res);
    } catch (err) {
      console.error("Failed to load student dashboard:", err);
      setError(formatDisplayError(err, "Unable to load student dashboard data."));
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
            <div className="max-w-2xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Student Portal
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Welcome back, {data?.studentName || "Student"}!
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  Track your pending assignments due soon, view your recent grades and teacher feedback, and manage your academic progress.
                </p>
              </div>
            </div>

            {/* Quick Portal Navigation Links */}
            <nav className="flex flex-wrap items-center gap-1.5 sm:gap-2 text-xs sm:text-sm font-medium shrink-0">
              <Link
                className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
                href="/student"
              >
                Dashboard
              </Link>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/student/assignments"
              >
                My Assignments
              </Link>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/student/submissions"
              >
                Submissions & Grades
              </Link>
              <button
                onClick={() => logoutUser()}
                className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
              >
                Logout 🚪
              </button>
            </nav>
          </div>
        </header>

        {/* Error Alert */}
        {error && (
          <div className="rounded-3xl border border-rose-200 bg-rose-50/90 p-4 shadow-sm text-rose-700">
            <p className="text-sm font-semibold">{error}</p>
          </div>
        )}

        {/* Metrics Grid */}
        <section className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold tracking-wider text-slate-400 uppercase">Enrolled Classes</span>
              <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h6m-6 4h6m-6 4h6" />
                </svg>
              </div>
            </div>
            <div className="mt-3">
              <span className="text-4xl font-extrabold text-foreground">{loading ? "..." : data?.enrolledClassesCount ?? 0}</span>
              <span className="ml-2 text-xs font-medium text-slate-500">Active subjects</span>
            </div>
          </div>

          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold tracking-wider text-slate-400 uppercase">Pending Due Soon</span>
              <div className="flex h-9 w-9 items-center justify-center rounded-full bg-amber-50 text-amber-600">
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
            <div className="mt-3">
              <span className="text-4xl font-extrabold text-foreground">{loading ? "..." : data?.pendingAssignmentsCount ?? 0}</span>
              <span className="ml-2 text-xs font-semibold text-amber-600">Action required</span>
            </div>
          </div>

          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold tracking-wider text-slate-400 uppercase">Completed Tasks</span>
              <div className="flex h-9 w-9 items-center justify-center rounded-full bg-emerald-50 text-emerald-600">
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
            <div className="mt-3">
              <span className="text-4xl font-extrabold text-foreground">{loading ? "..." : data?.completedAssignmentsCount ?? 0}</span>
              <span className="ml-2 text-xs font-semibold text-emerald-600">Submitted</span>
            </div>
          </div>

          <div className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold tracking-wider text-slate-400 uppercase">Average Grade</span>
              <div className="flex h-9 w-9 items-center justify-center rounded-full bg-sky-50 text-sky-600">
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
              </div>
            </div>
            <div className="mt-3">
              <span className="text-4xl font-extrabold text-foreground">{loading ? "..." : `${data?.averageGrade ?? 0}`}</span>
              <span className="ml-2 text-xs font-medium text-slate-500">Points avg</span>
            </div>
          </div>
        </section>

        {/* Dashboard Sections Grid */}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {/* Section 1: Assignments Due Soon */}
          <section className="flex flex-col rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="mb-4 flex items-center justify-between border-b border-black/5 pb-4">
              <div>
                <h2 className="text-xl font-semibold tracking-tight text-foreground">Assignments Due Soon</h2>
                <p className="text-xs text-(--color-muted) mt-0.5">Upcoming tasks needing your submission</p>
              </div>
              <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-800">
                {data?.assignmentsDueSoon.length ?? 0} Pending
              </span>
            </div>

            {loading ? (
              <div className="flex h-48 items-center justify-center text-sm text-slate-400">
                Loading assignments...
              </div>
            ) : !data?.assignmentsDueSoon || data.assignmentsDueSoon.length === 0 ? (
              <div className="flex h-48 flex-col items-center justify-center rounded-2xl border border-dashed border-slate-200 bg-white/60 p-6 text-center">
                <svg className="h-10 w-10 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p className="mt-2 text-sm font-semibold text-slate-700">No pending assignments due!</p>
                <p className="text-xs text-slate-500">You are all caught up with your tasks.</p>
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {data.assignmentsDueSoon.map((item: StudentAssignmentDueDto) => {
                  const daysRemainingLabel = getDaysRemaining(item.dueDate);
                  const isOverdue = item.status === "Overdue" || daysRemainingLabel === "Overdue";

                  return (
                    <div
                      key={item.assignmentId}
                      className="flex flex-col gap-3 rounded-2xl border border-slate-200/80 bg-white/90 p-4 shadow-2xs hover:border-teal-500/40 hover:shadow-md transition sm:flex-row sm:items-center sm:justify-between"
                    >
                      <div className="space-y-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-2.5 py-0.5 text-xs font-mono font-bold text-purple-700">
                            {item.subjectCode || item.subjectName}
                          </span>
                          <span className="text-xs text-slate-500">{item.className}</span>
                        </div>
                        <h3 className="text-base font-semibold text-slate-900">{item.title}</h3>
                        <p className="text-xs text-slate-500">
                          Due: <span className="font-medium text-slate-700">{formatDateTime(item.dueDate)}</span>
                        </p>
                      </div>

                      <div className="flex items-center justify-between gap-3 sm:flex-col sm:items-end">
                        <span
                          className={`rounded-full px-2.5 py-1 text-xs font-semibold ${
                            isOverdue
                              ? "bg-rose-100 text-rose-700 border border-rose-200"
                              : "bg-amber-100 text-amber-800 border border-amber-200"
                          }`}
                        >
                          {daysRemainingLabel}
                        </span>
                        <span className="text-xs text-slate-500">Max Marks: {item.maxMarks}</span>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </section>

          {/* Section 2: Recent Grades & Feedback */}
          <section className="flex flex-col rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="mb-4 flex items-center justify-between border-b border-black/5 pb-4">
              <div>
                <h2 className="text-xl font-semibold tracking-tight text-foreground">Recent Grades & Feedback</h2>
                <p className="text-xs text-(--color-muted) mt-0.5">Evaluated assignments and teacher comments</p>
              </div>
              <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-800">
                {data?.recentGradesFeedback.length ?? 0} Evaluated
              </span>
            </div>

            {loading ? (
              <div className="flex h-48 items-center justify-center text-sm text-slate-400">
                Loading grades...
              </div>
            ) : !data?.recentGradesFeedback || data.recentGradesFeedback.length === 0 ? (
              <div className="flex h-48 flex-col items-center justify-center rounded-2xl border border-dashed border-slate-200 bg-slate-50 p-6 text-center">
                <svg className="h-10 w-10 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                <p className="mt-2 text-sm font-semibold text-slate-700">No grades recorded yet!</p>
                <p className="text-xs text-slate-500">Your graded assignments will appear here once teachers evaluate your work.</p>
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {data.recentGradesFeedback.map((item: StudentRecentGradeDto) => (
                  <div
                    key={item.submissionId}
                    className="flex flex-col gap-3 rounded-2xl border border-slate-100 bg-slate-50/70 p-4 transition hover:bg-slate-50 hover:shadow-xs"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <span className="rounded-md bg-teal-100 px-2 py-0.5 text-xs font-semibold text-teal-800">
                            {item.subjectCode || item.subjectName}
                          </span>
                          <span className="text-xs text-slate-500">
                            Evaluated: {formatDateTime(item.gradedAt || item.submittedAt)}
                          </span>
                        </div>
                        <h3 className="text-base font-semibold text-slate-900">{item.assignmentTitle}</h3>
                      </div>

                      <div className="flex flex-col items-end">
                        <span className="rounded-xl border border-emerald-200 bg-emerald-100 px-3 py-1 text-sm font-bold text-emerald-800">
                          {item.grade !== null && item.grade !== undefined ? `${item.grade} / ${item.maxMarks}` : "Graded"}
                        </span>
                      </div>
                    </div>

                    {/* Teacher Feedback Box */}
                    <div className="rounded-xl border border-slate-200 bg-white p-3 text-xs leading-relaxed text-slate-700">
                      <div className="mb-1 flex items-center justify-between text-slate-400">
                        <span className="font-medium text-slate-500">Teacher Feedback</span>
                        <span>{item.gradedByTeacherName}</span>
                      </div>
                      <p className="text-slate-800 italic">"{item.feedback || "No specific feedback comment left."}"</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </section>
        </div>
      </div>
    </main>
  );
}
