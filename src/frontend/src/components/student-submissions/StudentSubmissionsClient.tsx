"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  downloadFileFromServer,
  getStudentSubmissionsHistory,
  resolveServerFileUrl,
  type PagedStudentSubmissionHistoryResultDto,
  type StudentSubmissionHistoryFilterDto,
  type StudentSubmissionHistoryResponseDto,
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

export function StudentSubmissionsClient() {
  const [filter, setFilter] = useState<StudentSubmissionHistoryFilterDto>({
    subjectName: "",
    status: "All",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedStudentSubmissionHistoryResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchHistory = useCallback(async (currentFilter: StudentSubmissionHistoryFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getStudentSubmissionsHistory(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load submission history:", err);
      setError(err instanceof Error ? err.message : "Unable to load submission history.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchHistory(filter);
  }, [filter, fetchHistory]);

  const handleStatusFilterChange = (status: "All" | "Submitted" | "Graded") => {
    setFilter((prev) => ({ ...prev, status, pageNumber: 1 }));
  };

  const handleSubjectChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilter((prev) => ({ ...prev, subjectName: e.target.value, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Header */}
        <header className="overflow-hidden rounded-3xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-indigo-500/15 bg-indigo-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-indigo-700">
                Student History
              </div>
              <div>
                <h1 className="text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
                  My Submissions & Grades
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600 sm:text-base">
                  Track the full record of all your turned-in work, evaluated grades, teacher feedback, and submission attachments.
                </p>
              </div>
            </div>

            {/* Quick Portal Navigation */}
            <div className="flex flex-wrap items-center gap-3">
              <Link
                href="/student"
                className="rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
              >
                Dashboard
              </Link>
              <Link
                href="/student/assignments"
                className="rounded-xl border border-indigo-200 bg-indigo-50 px-4 py-2.5 text-sm font-semibold text-indigo-700 hover:bg-indigo-100"
              >
                My Assignments
              </Link>
              <button
                onClick={() => logoutUser()}
                className="rounded-xl bg-slate-900 px-4 py-2.5 text-sm font-medium text-white shadow-sm hover:bg-slate-800"
              >
                Sign out
              </button>
            </div>
          </div>
        </header>

        {/* Filter Bar */}
        <section className="flex flex-col gap-4 rounded-3xl border border-slate-200 bg-white p-4 shadow-sm sm:p-6 lg:flex-row lg:items-center lg:justify-between">
          <div className="flex flex-wrap items-center gap-2">
            {(["All", "Submitted", "Graded"] as const).map((status) => (
              <button
                key={status}
                onClick={() => handleStatusFilterChange(status)}
                className={`rounded-xl px-4 py-2 text-xs font-semibold transition ${
                  filter.status === status
                    ? "bg-indigo-600 text-white shadow-sm"
                    : "border border-slate-200 bg-slate-50 text-slate-600 hover:bg-slate-100"
                }`}
              >
                {status === "Submitted" ? "Pending Review" : status}
              </button>
            ))}
          </div>

          <div className="flex items-center gap-3">
            <input
              type="text"
              value={filter.subjectName || ""}
              onChange={handleSubjectChange}
              placeholder="Filter by subject..."
              className="w-full sm:w-64 rounded-xl border border-slate-200 px-3.5 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-hidden"
            />
            <button
              onClick={() => fetchHistory(filter)}
              disabled={loading}
              className="rounded-xl border border-slate-200 bg-slate-50 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-50"
            >
              Refresh
            </button>
          </div>
        </section>

        {/* Error Alert */}
        {error && (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 p-4 text-rose-800 text-sm font-medium">
            {error}
          </div>
        )}

        {/* History List */}
        <section className="flex flex-col gap-4">
          {loading ? (
            <div className="flex h-64 items-center justify-center rounded-3xl border border-slate-200 bg-white text-sm text-slate-400">
              Loading submission history...
            </div>
          ) : !pagedData?.items || pagedData.items.length === 0 ? (
            <div className="flex h-64 flex-col items-center justify-center rounded-3xl border border-dashed border-slate-200 bg-white p-6 text-center">
              <svg className="h-10 w-10 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <p className="mt-2 text-sm font-semibold text-slate-700">No submissions found</p>
              <p className="text-xs text-slate-500">You have not submitted any assignments matching the selected criteria.</p>
            </div>
          ) : (
            <div className="space-y-4">
              {pagedData.items.map((item: StudentSubmissionHistoryResponseDto) => (
                <div
                  key={item.submissionId}
                  className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm transition hover:shadow-md"
                >
                  <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                    <div className="space-y-2 max-w-2xl">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="rounded-md bg-indigo-100 px-2.5 py-0.5 text-xs font-semibold text-indigo-700">
                          {item.subjectCode || item.subjectName}
                        </span>
                        <span className="text-xs text-slate-500">{item.className}</span>
                        <span
                          className={`rounded-full border px-2.5 py-0.5 text-[11px] font-bold ${
                            item.status === "Graded"
                              ? "border-emerald-200 bg-emerald-100 text-emerald-800"
                              : "border-sky-200 bg-sky-100 text-sky-800"
                          }`}
                        >
                          {item.status}
                        </span>
                      </div>

                      <h2 className="text-lg font-bold text-slate-900">{item.assignmentTitle}</h2>
                      <p className="text-xs text-slate-500">
                        Teacher: <strong className="text-slate-700">{item.teacherName}</strong> • Submitted:{" "}
                        <strong className="text-slate-700">{formatDateTime(item.submittedAt)}</strong>
                      </p>

                      {/* Submission text if any */}
                      {item.submissionText && (
                        <div className="mt-2 rounded-xl bg-slate-50 p-3 text-xs text-slate-700">
                          <span className="font-semibold text-slate-900 block mb-0.5">Submitted Work:</span>
                          <p className="line-clamp-2 italic">{item.submissionText}</p>
                        </div>
                      )}

                      {/* File attachment link if any */}
                      {item.fileUrl && (
                        <div className="mt-2 inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-xs">
                          <svg className="h-4 w-4 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
                          </svg>
                          <a
                            href={resolveServerFileUrl(item.fileUrl)}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="font-medium text-indigo-600 underline hover:text-indigo-800"
                          >
                            Open Attachment ({item.fileUrl})
                          </a>
                        </div>
                      )}
                    </div>

                    {/* Grade & Action Box */}
                    <div className="flex flex-col items-end gap-3 shrink-0 border-t border-slate-100 pt-3 md:border-t-0 md:pt-0">
                      {item.marks !== null && item.marks !== undefined ? (
                        <div className="text-right">
                          <span className="block text-[11px] font-semibold uppercase tracking-wider text-slate-400">Score</span>
                          <span className="text-2xl font-black text-emerald-700">
                            {item.marks} <span className="text-sm font-semibold text-slate-400">/ {item.maxMarks}</span>
                          </span>
                        </div>
                      ) : (
                        <div className="text-right">
                          <span className="block text-[11px] font-semibold uppercase tracking-wider text-slate-400">Score</span>
                          <span className="text-sm font-medium text-slate-400">Pending Evaluation</span>
                        </div>
                      )}

                      <Link
                        href={`/student/assignments/${item.assignmentId}`}
                        className="rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-1.5 text-xs font-semibold text-slate-700 hover:bg-slate-100"
                      >
                        View Assignment
                      </Link>
                    </div>
                  </div>

                  {/* Feedback Banner if graded */}
                  {item.feedback && (
                    <div className="mt-4 rounded-2xl border border-emerald-100 bg-emerald-50/60 p-3.5 text-xs text-emerald-900">
                      <span className="font-bold">Teacher Feedback: </span>
                      <span className="italic">"{item.feedback}"</span>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}

          {/* Pagination */}
          {pagedData && pagedData.totalPages > 1 && (
            <div className="flex items-center justify-between rounded-3xl border border-slate-200 bg-white px-6 py-4 text-xs text-slate-500 shadow-sm">
              <span>
                Page {pagedData.pageNumber} of {pagedData.totalPages} ({pagedData.totalCount} total submissions)
              </span>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handlePageChange(pagedData.pageNumber - 1)}
                  disabled={!pagedData.hasPreviousPage}
                  className="rounded-lg border border-slate-200 px-3 py-1.5 text-slate-700 hover:bg-slate-50 disabled:opacity-40"
                >
                  Previous
                </button>
                <button
                  onClick={() => handlePageChange(pagedData.pageNumber + 1)}
                  disabled={!pagedData.hasNextPage}
                  className="rounded-lg border border-slate-200 px-3 py-1.5 text-slate-700 hover:bg-slate-50 disabled:opacity-40"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
