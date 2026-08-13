"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  resolveSubmissionFileUrl,
  type SubmissionResponseDto,
} from "@/lib/admin-submissions";
import { getTeacherSubmissions } from "@/lib/teacher-assignments";
import { logoutUser } from "@/lib/auth";
import { GradeSubmissionModal } from "./GradeSubmissionModal";

function formatDate(dateStr?: string) {
  if (!dateStr) return "N/A";
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return dateStr;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(d);
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
        pageNumber,
        pageSize,
      });
      setItems(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages || 1);
    } catch (err) {
      console.error("Failed to load submissions:", err);
      setError("Failed to load student submissions list.");
    } finally {
      setLoading(false);
    }
  }, [assignmentTitleFilter, classFilter, subjectFilter, studentNameFilter, statusFilter, pageNumber, pageSize]);

  useEffect(() => {
    fetchSubmissions();
  }, [fetchSubmissions]);

  const handleFilterReset = () => {
    setAssignmentTitleFilter("");
    setClassFilter("");
    setSubjectFilter("");
    setStudentNameFilter("");
    setStatusFilter("");
    setPageNumber(1);
  };

  const gradedCount = items.filter((i) => i.status === "Graded").length;
  const pendingCount = items.filter((i) => i.status === "Submitted").length;

  return (
    <main className="min-h-screen bg-(--color-background) px-4 py-8 sm:px-8 font-sans">
      <div className="mx-auto max-w-7xl space-y-8">
        {/* Header & Navigation */}
        <header className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-black/5 pb-6">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3.5 py-1 text-xs font-semibold uppercase tracking-[0.2em] text-teal-700">
              Teacher Portal
            </span>
            <h1 className="mt-2 text-3xl font-extrabold tracking-tight text-foreground sm:text-4xl">
              Student Submissions Management
            </h1>
            <p className="mt-1 text-sm text-slate-500">
              Review submitted assignments, grade coursework, and provide feedback to students.
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <Link
              href="/teacher"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-50 transition"
            >
              ← Dashboard
            </Link>
            <Link
              href="/teacher/assignments"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-50 transition"
            >
              Assignments 📚
            </Link>
            <button
              onClick={() => logoutUser()}
              className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-600 hover:text-white transition cursor-pointer"
            >
              Sign Out
            </button>
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
                          <div className="font-semibold text-slate-900">{item.studentName}</div>
                          <div className="text-xs text-slate-500">{item.studentEmail}</div>
                        </td>

                        <td className="px-6 py-4 max-w-xs">
                          <div className="font-medium text-slate-900">{item.assignmentTitle}</div>
                          <div className="text-xs text-slate-500 mt-0.5">
                            {item.className} ({item.subjectCode})
                          </div>
                        </td>

                        <td className="px-6 py-4 text-xs text-slate-600 font-medium">
                          {formatDate(item.submittedAt)}
                        </td>

                        <td className="px-6 py-4">
                          {fileUrl ? (
                            <div className="flex items-center gap-2">
                              <a
                                href={fileUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="inline-flex items-center gap-1 rounded-full border border-teal-600 bg-teal-50 px-3 py-1 text-xs font-semibold text-teal-700 hover:bg-teal-600 hover:text-white transition"
                              >
                                👁️ View File
                              </a>
                              <a
                                href={fileUrl}
                                download
                                className="inline-flex items-center gap-1 rounded-full bg-slate-900 px-3 py-1 text-xs font-semibold text-white hover:bg-slate-800 transition"
                              >
                                ⬇️
                              </a>
                            </div>
                          ) : (
                            <span className="text-xs text-slate-400 italic">No File</span>
                          )}
                        </td>

                        <td className="px-6 py-4">
                          {item.grade !== undefined && item.grade !== null ? (
                            <span className="font-semibold text-slate-900 text-xs">
                              {item.grade} / {item.maxMarks}
                            </span>
                          ) : (
                            <span className="text-xs text-slate-400 font-mono">— / {item.maxMarks}</span>
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

                        <td className="px-6 py-4 text-right">
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
