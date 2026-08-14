"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getStudentAssignments,
  type PagedStudentAssignmentResultDto,
  type StudentAssignmentFilterDto,
  type StudentAssignmentResponseDto,
} from "@/lib/student-assignments";
import { logoutUser } from "@/lib/auth";

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

  if (diffDays < 0) return "Past Deadline";
  if (diffDays === 0) return "Due Today";
  if (diffDays === 1) return "Due Tomorrow";
  return `Due in ${diffDays} days`;
}

function renderStatusBadge(status: string, marks?: number, maxMarks?: number) {
  switch (status) {
    case "Graded":
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-emerald-200 bg-emerald-100 px-3 py-1 text-xs font-bold text-emerald-800">
          <svg className="h-3.5 w-3.5 text-emerald-600" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
          </svg>
          Graded: {marks !== null && marks !== undefined ? `${marks}/${maxMarks}` : "Evaluated"}
        </span>
      );
    case "Submitted":
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-sky-200 bg-sky-100 px-3 py-1 text-xs font-bold text-sky-800">
          <svg className="h-3.5 w-3.5 text-sky-600" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
          </svg>
          Submitted
        </span>
      );
    case "Overdue":
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-rose-200 bg-rose-100 px-3 py-1 text-xs font-bold text-rose-800">
          <svg className="h-3.5 w-3.5 text-rose-600" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
          Overdue (Not Submitted)
        </span>
      );
    case "Pending":
    default:
      return (
        <span className="inline-flex items-center gap-1.5 rounded-full border border-amber-200 bg-amber-100 px-3 py-1 text-xs font-bold text-amber-800">
          <svg className="h-3.5 w-3.5 text-amber-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          Pending (Not Submitted)
        </span>
      );
  }
}

import { formatDisplayError } from "@/lib/api-error";

export function StudentAssignmentsClient() {
  const [filter, setFilter] = useState<StudentAssignmentFilterDto>({
    statusFilter: "All",
    search: "",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedStudentAssignmentResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAssignments = useCallback(async (currentFilter: StudentAssignmentFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getStudentAssignments(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load student assignments:", err);
      setError(formatDisplayError(err, "Unable to load assignments."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAssignments(filter);
  }, [filter, fetchAssignments]);

  const handleStatusFilterChange = (status: "All" | "Pending" | "Submitted" | "Graded") => {
    setFilter((prev) => ({ ...prev, statusFilter: status, pageNumber: 1 }));
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilter((prev) => ({ ...prev, search: e.target.value, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Header */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="max-w-2xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Student Portal
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  My Class Assignments
                </h1>
                <p className="mt-3 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  View published assignments for your enrolled subjects, check submission status, track upcoming deadlines, and turn in your work.
                </p>
              </div>
            </div>

            {/* Quick Portal Navigation */}
            <nav className="flex flex-wrap items-center gap-1.5 sm:gap-2 text-xs sm:text-sm font-medium shrink-0">
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
                href="/student"
              >
                Dashboard
              </Link>
              <Link
                className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
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

        {/* Filter & Search Bar */}
        <section className="flex flex-col gap-4 rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur lg:flex-row lg:items-center lg:justify-between">
          {/* Status Filter Tabs */}
          <div className="flex flex-wrap items-center gap-2">
            {(["All", "Pending", "Submitted", "Graded"] as const).map((status) => (
              <button
                key={status}
                onClick={() => handleStatusFilterChange(status)}
                className={`rounded-full px-4 py-2 text-xs font-semibold transition cursor-pointer ${
                  filter.statusFilter === status
                    ? "bg-foreground text-background shadow-md"
                    : "border border-slate-200 bg-white/80 text-slate-700 hover:bg-slate-100"
                }`}
              >
                {status === "Pending" ? "Pending / Not Submitted" : `${status} Assignments`}
              </button>
            ))}
          </div>

          {/* Search Box */}
          <div className="flex items-center gap-3">
            <input
              type="text"
              value={filter.search || ""}
              onChange={handleSearchChange}
              placeholder="Search title, subject, or class..."
              className="w-full sm:w-72 rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
            />
            <button
              onClick={() => fetchAssignments(filter)}
              disabled={loading}
              className="rounded-full border border-slate-200 bg-white/90 px-4 py-2 text-xs font-semibold uppercase tracking-wider text-slate-600 hover:bg-slate-100 transition shadow-2xs cursor-pointer disabled:opacity-50"
            >
              Refresh
            </button>
          </div>
        </section>

        {/* Error Alert */}
        {error && (
          <div className="rounded-3xl border border-rose-200 bg-rose-50/90 p-4 shadow-sm text-rose-700 text-sm font-semibold">
            {error}
          </div>
        )}

        {/* Assignments Grid */}
        <section className="flex flex-col gap-4">
          {loading ? (
            <div className="flex h-64 items-center justify-center rounded-3xl border border-white/70 bg-(--color-surface) text-sm text-slate-400 backdrop-blur">
              Loading assignments...
            </div>
          ) : !pagedData?.items || pagedData.items.length === 0 ? (
            <div className="flex h-64 flex-col items-center justify-center rounded-3xl border border-dashed border-slate-200 bg-white/60 p-6 text-center">
              <svg className="h-10 w-10 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <p className="mt-2 text-sm font-semibold text-slate-700">No assignments found</p>
              <p className="text-xs text-slate-500">Try changing your search keywords or status filter.</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              {pagedData.items.map((item: StudentAssignmentResponseDto) => {
                const daysRemaining = getDaysRemaining(item.deadline);
                const isSubmitted = item.status === "Submitted" || item.status === "Graded";

                return (
                  <div
                    key={item.id}
                    className="flex flex-col justify-between rounded-3xl border border-slate-200/80 bg-white/90 p-6 shadow-2xs hover:border-teal-500/40 hover:shadow-md transition group"
                  >
                    <div className="space-y-3">
                      {/* Top Badges */}
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        <div className="flex items-center gap-2">
                          <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-2.5 py-0.5 text-xs font-mono font-bold text-purple-700">
                            {item.subjectCode || item.subjectName}
                          </span>
                          <span className="text-xs text-slate-500">{item.className}</span>
                        </div>
                        {renderStatusBadge(item.status, item.marks, item.maxMarks)}
                      </div>

                      {/* Title & Description */}
                      <div>
                        <h2 className="text-lg font-semibold text-foreground group-hover:text-teal-700 transition">{item.title}</h2>
                        <p className="mt-1 line-clamp-2 text-xs text-(--color-muted) leading-relaxed">
                          {item.description || "No description provided."}
                        </p>
                      </div>

                      {/* Teacher & Submission Status Bar */}
                      <div className="flex flex-col gap-1 border-t border-black/5 pt-3 text-xs">
                        <div className="flex items-center justify-between text-slate-500">
                          <span>Teacher: <strong className="text-slate-700">{item.teacherName}</strong></span>
                          <span className="font-semibold text-slate-800">
                            Max Marks: {item.maxMarks}
                          </span>
                        </div>

                        {/* Explicit Submission Indicator */}
                        {isSubmitted ? (
                          <div className="mt-1 flex items-center justify-between rounded-xl bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-800">
                            <span>✓ Turned In</span>
                            {item.submittedAt && <span>{formatDateTime(item.submittedAt)}</span>}
                          </div>
                        ) : (
                          <div className="mt-1 flex items-center justify-between rounded-xl bg-amber-50 px-3 py-1.5 text-xs font-medium text-amber-800">
                            <span>⏳ Work Not Submitted Yet</span>
                            <span className="font-semibold">{daysRemaining}</span>
                          </div>
                        )}
                      </div>
                    </div>

                    {/* Footer / Actions */}
                    <div className="mt-4 flex items-center justify-between border-t border-black/5 pt-4">
                      <div className="text-xs">
                        <span className="text-slate-400">Deadline: </span>
                        <span className="font-medium text-slate-700">{formatDateTime(item.deadline)}</span>
                      </div>

                      <Link
                        href={`/student/assignments/${item.id}`}
                        className={`inline-flex items-center gap-1.5 rounded-full px-4 py-1.5 text-xs font-medium shadow-2xs transition cursor-pointer ${
                          isSubmitted
                            ? "border border-slate-300 bg-white text-slate-800 hover:bg-slate-900 hover:text-white"
                            : "bg-foreground text-background hover:opacity-90"
                        }`}
                      >
                        {isSubmitted ? "View / Edit Submission" : "Turn In Work"} →
                      </Link>
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          {/* Pagination Footer */}
          {pagedData && pagedData.totalPages > 1 && (
            <div className="flex items-center justify-between rounded-3xl border border-white/70 bg-(--color-surface) px-6 py-4 text-xs text-slate-500 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
              <span>
                Page {pagedData.pageNumber} of {pagedData.totalPages} ({pagedData.totalCount} total assignments)
              </span>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handlePageChange(pagedData.pageNumber - 1)}
                  disabled={!pagedData.hasPreviousPage}
                  className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition"
                >
                  ← Previous
                </button>
                <button
                  onClick={() => handlePageChange(pagedData.pageNumber + 1)}
                  disabled={!pagedData.hasNextPage}
                  className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition"
                >
                  Next →
                </button>
              </div>
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
