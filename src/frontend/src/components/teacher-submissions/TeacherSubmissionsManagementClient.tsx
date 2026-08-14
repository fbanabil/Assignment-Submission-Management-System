"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  resolveSubmissionFileUrl,
  type SubmissionResponseDto,
} from "@/lib/admin-submissions";
import { getTeacherSubmissions } from "@/lib/teacher-assignments";
import { logoutUser } from "@/lib/auth";
import { formatDisplayError } from "@/lib/api-error";
import { GradeSubmissionModal } from "./GradeSubmissionModal";

function formatDateParts(dateStr?: string) {
  if (!dateStr) return { date: "N/A", time: "" };
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return { date: dateStr, time: "" };
  const date = new Intl.DateTimeFormat("en-US", { dateStyle: "medium" }).format(d);
  const time = new Intl.DateTimeFormat("en-US", { timeStyle: "short" }).format(d);
  return { date, time };
}

export function TeacherSubmissionsManagementClient() {
  const [items, setItems] = useState<SubmissionResponseDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  // Filters
  const [assignmentTitleFilter, setAssignmentTitleFilter] = useState("");
  const [classFilter, setClassFilter] = useState("");
  const [subjectFilter, setSubjectFilter] = useState("");
  const [studentNameFilter, setStudentNameFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [sortBy, setSortBy] = useState("submittedat");
  const [sortOrder, setSortOrder] = useState<"Asc" | "Desc">("Desc");

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Grade Modal State
  const [gradingSubmission, setGradingSubmission] = useState<SubmissionResponseDto | null>(null);

  const fetchSubmissions = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getTeacherSubmissions({
        assignmentTitle: assignmentTitleFilter.trim() || undefined,
        className: classFilter.trim() || undefined,
        subjectCode: subjectFilter.trim() || undefined,
        studentName: studentNameFilter.trim() || undefined,
        status: statusFilter.trim() || undefined,
        sortBy,
        sortOrder,
        pageNumber,
        pageSize,
      });
      setItems(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages || 1);
    } catch (err) {
      console.error("Failed to load submissions:", err);
      setError(formatDisplayError(err, "Failed to load student submissions list."));
    } finally {
      setLoading(false);
    }
  }, [assignmentTitleFilter, classFilter, subjectFilter, studentNameFilter, statusFilter, sortBy, sortOrder, pageNumber, pageSize]);

  useEffect(() => {
    fetchSubmissions();
  }, [fetchSubmissions]);

  const handleFilterReset = () => {
    setAssignmentTitleFilter("");
    setClassFilter("");
    setSubjectFilter("");
    setStudentNameFilter("");
    setStatusFilter("");
    setSortBy("submittedat");
    setSortOrder("Desc");
    setPageNumber(1);
  };

  const gradedCount = items.filter((i) => i.status === "Graded").length;
  const pendingCount = items.filter((i) => i.status === "Submitted").length;

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Header & Navigation */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Teacher Portal
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Student Submissions Management
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  Review submitted assignments, grade coursework, and provide feedback to students.
                </p>
              </div>
            </div>

            <nav className="flex flex-wrap items-center gap-1.5 sm:gap-2 text-xs sm:text-sm font-medium shrink-0">
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
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
                className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
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
        </header>

        {/* Submissions Summary Stats */}
        <section className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div className="rounded-3xl border border-white/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
              Total Submissions
            </span>
            <span className="text-3xl font-black text-slate-900 block mt-1">{totalCount}</span>
          </div>

          <div className="rounded-3xl border border-white/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
              Pending Review / Ungraded
            </span>
            <span className="text-3xl font-black text-amber-600 block mt-1">{pendingCount}</span>
          </div>

          <div className="rounded-3xl border border-white/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
              Graded Submissions
            </span>
            <span className="text-3xl font-black text-emerald-600 block mt-1">{gradedCount}</span>
          </div>
        </section>

        {/* Search & Filter Controls */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.06)] backdrop-blur">
          <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-4">
            Filter & Search Submissions
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-5 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Assignment Title
              </label>
              <input
                type="text"
                placeholder="Search title..."
                value={assignmentTitleFilter}
                onChange={(e) => {
                  setAssignmentTitleFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Student Name
              </label>
              <input
                type="text"
                placeholder="Search student..."
                value={studentNameFilter}
                onChange={(e) => {
                  setStudentNameFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Class Name
              </label>
              <input
                type="text"
                placeholder="e.g. Grade 10"
                value={classFilter}
                onChange={(e) => {
                  setClassFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Subject Code
              </label>
              <input
                type="text"
                placeholder="e.g. MATH101"
                value={subjectFilter}
                onChange={(e) => {
                  setSubjectFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Status
              </label>
              <select
                value={statusFilter}
                onChange={(e) => {
                  setStatusFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              >
                <option value="">All Statuses</option>
                <option value="Submitted">Submitted (Ungraded)</option>
                <option value="Graded">Graded</option>
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Sort By
              </label>
              <div className="flex items-center gap-1.5">
                <select
                  value={sortBy}
                  onChange={(e) => {
                    setSortBy(e.target.value);
                    setPageNumber(1);
                  }}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
                >
                  <option value="submittedat">Submitted Date</option>
                  <option value="studentname">Student Name</option>
                  <option value="assignmenttitle">Assignment Title</option>
                  <option value="classname">Class Name</option>
                  <option value="status">Status</option>
                  <option value="marks">Grade / Score</option>
                </select>
                <button
                  type="button"
                  onClick={() => {
                    setSortOrder((prev) => (prev === "Asc" ? "Desc" : "Asc"));
                    setPageNumber(1);
                  }}
                  className="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-2xl border border-slate-200 bg-white text-xs font-bold text-slate-700 shadow-2xs hover:bg-slate-100 transition cursor-pointer"
                  title={`Sort Order: ${sortOrder === "Asc" ? "Ascending" : "Descending"}`}
                >
                  {sortOrder === "Asc" ? "⬆️" : "⬇️"}
                </button>
              </div>
            </div>
          </div>

          <div className="mt-4 flex items-center justify-between border-t border-black/5 pt-3 text-xs">
            <span className="text-slate-500">
              Showing <strong>{items.length}</strong> of <strong>{totalCount}</strong> submissions
            </span>
            <button
              onClick={handleFilterReset}
              className="text-teal-700 hover:text-teal-900 font-semibold cursor-pointer"
            >
              Reset Filters ↺
            </button>
          </div>
        </section>

        {/* Error Notification */}
        {error && (
          <div className="rounded-3xl border border-rose-200 bg-rose-50 p-4 text-rose-700 text-sm font-semibold">
            {error}
          </div>
        )}

        {/* Submissions Table */}
        <section className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50/80 text-xs font-semibold uppercase tracking-wider text-slate-500 border-b border-black/5">
                <tr>
                  <th className="px-6 py-4">Student</th>
                  <th className="px-6 py-4">Assignment & Class</th>
                  <th className="px-6 py-4">Submitted Date</th>
                  <th className="px-6 py-4">Attached File</th>
                  <th className="px-6 py-4">Grade & Score</th>
                  <th className="px-6 py-4">Status</th>
                  <th className="px-6 py-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-black/5">
                {loading ? (
                  Array.from({ length: 4 }).map((_, idx) => (
                    <tr key={`skel-sub-${idx}`} className="animate-pulse">
                      <td className="px-6 py-4"><div className="h-4 w-32 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-40 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-24 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-20 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-16 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-16 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4 text-right"><div className="h-4 w-20 bg-slate-200 rounded-full ml-auto"></div></td>
                    </tr>
                  ))
                ) : items.length > 0 ? (
                  items.map((item) => {
                    const fileUrl = item.fileUrl ? resolveSubmissionFileUrl(item.fileUrl) : null;
                    return (
                      <tr key={item.id} className="hover:bg-slate-50/60 transition">
                        <td className="px-6 py-4">
                          <div className="flex flex-col">
                            <div className="font-semibold text-slate-900 leading-tight">{item.studentName}</div>
                            {item.studentRollNo && (
                              <span className="inline-flex w-fit items-center rounded-md border border-purple-500/20 bg-purple-500/10 px-2 py-0.5 text-[11px] font-mono font-bold text-purple-700 mt-1">
                                Roll: {item.studentRollNo}
                              </span>
                            )}
                            <div className="text-xs text-slate-500 mt-1">{item.studentEmail}</div>
                          </div>
                        </td>

                        <td className="px-6 py-4 max-w-xs">
                          <div className="font-medium text-slate-900">{item.assignmentTitle}</div>
                          <div className="text-xs text-slate-500 mt-0.5">
                            {item.className} ({item.subjectCode})
                          </div>
                        </td>

                        <td className="px-6 py-4 text-xs font-medium">
                          {(() => {
                            const parts = formatDateParts(item.submittedAt);
                            return (
                              <div className="flex flex-col gap-0.5">
                                <span className="font-semibold text-slate-800 text-xs">{parts.date}</span>
                                {parts.time && (
                                  <span className="text-[11px] font-mono text-slate-500">{parts.time}</span>
                                )}
                              </div>
                            );
                          })()}
                        </td>

                        <td className="px-6 py-4">
                          {fileUrl ? (
                            <div className="flex flex-col gap-1 items-start min-w-[115px]">
                              <a
                                href={fileUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="w-full inline-flex items-center justify-center gap-1.5 whitespace-nowrap rounded-full border border-teal-600 bg-teal-50 px-3 py-1 text-xs font-semibold text-teal-700 hover:bg-teal-600 hover:text-white transition"
                              >
                                👁️ Open File
                              </a>
                              <a
                                href={fileUrl}
                                download
                                className="w-full inline-flex items-center justify-center gap-1.5 whitespace-nowrap rounded-full border border-slate-300 bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-800 hover:bg-slate-900 hover:text-white transition"
                              >
                                ⬇️ Download
                              </a>
                            </div>
                          ) : (
                            <span className="text-xs text-slate-400 italic">No File</span>
                          )}
                        </td>

                        <td className="px-6 py-4">
                          {item.grade !== undefined && item.grade !== null ? (
                            <div className="inline-flex items-center gap-1 rounded-xl border border-emerald-200 bg-emerald-50/80 px-2.5 py-1 text-xs shadow-2xs">
                              <span className="text-emerald-700 font-medium">Grade:</span>
                              <span className="font-mono font-bold text-emerald-900 text-xs">{item.grade}</span>
                              <span className="text-emerald-700 font-medium">/ {item.maxMarks}</span>
                            </div>
                          ) : (
                            <div className="inline-flex items-center gap-1 rounded-xl border border-amber-200/80 bg-amber-50/70 px-2.5 py-1 text-xs font-medium text-amber-800 shadow-2xs">
                              <span>⏳ Ungraded ({item.maxMarks} max)</span>
                            </div>
                          )}
                        </td>

                        <td className="px-6 py-4">
                          <span
                            className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-bold ${
                              item.status === "Graded"
                                ? "bg-emerald-100 text-emerald-800"
                                : "bg-amber-100 text-amber-800"
                            }`}
                          >
                            {item.status}
                          </span>
                        </td>

                        <td className="px-6 py-4 text-right whitespace-nowrap">
                          <button
                            onClick={() => setGradingSubmission(item)}
                            className="rounded-full bg-teal-600 px-4 py-1.5 text-xs font-semibold text-white shadow-xs hover:bg-teal-700 transition cursor-pointer"
                          >
                            {item.status === "Graded" ? "Edit Grade ✏️" : "Grade Submission 📝"}
                          </button>
                        </td>
                      </tr>
                    );
                  })
                ) : (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-slate-500">
                      No student submissions found matching your filters.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination bar */}
          <div className="flex items-center justify-between border-t border-black/5 px-6 py-4 text-xs font-semibold text-slate-600">
            <button
              disabled={pageNumber <= 1}
              onClick={() => setPageNumber((p) => p - 1)}
              className="rounded-full border border-slate-200 px-4 py-2 hover:bg-slate-100 disabled:opacity-40 transition"
            >
              ← Previous
            </button>
            <span>
              Page {pageNumber} of {totalPages}
            </span>
            <button
              disabled={pageNumber >= totalPages}
              onClick={() => setPageNumber((p) => p + 1)}
              className="rounded-full border border-slate-200 px-4 py-2 hover:bg-slate-100 disabled:opacity-40 transition"
            >
              Next →
            </button>
          </div>
        </section>

        {/* Modal for Grading */}
        <GradeSubmissionModal
          isOpen={!!gradingSubmission}
          submission={gradingSubmission}
          onClose={() => setGradingSubmission(null)}
          onSuccess={() => fetchSubmissions()}
        />
      </div>
    </main>
  );
}
