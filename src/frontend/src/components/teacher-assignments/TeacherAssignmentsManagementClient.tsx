"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getTeacherAssignments,
  type TeacherAssignmentItemDto,
} from "@/lib/teacher-assignments";
import { logoutUser } from "@/lib/auth";
import { formatDisplayError } from "@/lib/api-error";
import { CreateAssignmentModal } from "./CreateAssignmentModal";
import { EditAssignmentModal } from "./EditAssignmentModal";
import { DeleteAssignmentModal } from "./DeleteAssignmentModal";
import { AssignmentDetailsModal } from "./AssignmentDetailsModal";

function formatDateParts(dateStr?: string) {
  if (!dateStr) return { date: "N/A", time: "" };
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return { date: dateStr, time: "" };
  const date = new Intl.DateTimeFormat("en-US", { dateStyle: "medium" }).format(d);
  const time = new Intl.DateTimeFormat("en-US", { timeStyle: "short" }).format(d);
  return { date, time };
}

export function TeacherAssignmentsManagementClient() {
  const [items, setItems] = useState<TeacherAssignmentItemDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  // Filters
  const [titleFilter, setTitleFilter] = useState("");
  const [classFilter, setClassFilter] = useState("");
  const [subjectFilter, setSubjectFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [sortBy, setSortBy] = useState("duedate");
  const [sortOrder, setSortOrder] = useState<"Asc" | "Desc">("Asc");

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals state
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingAssignment, setEditingAssignment] = useState<TeacherAssignmentItemDto | null>(null);
  const [deletingAssignment, setDeletingAssignment] = useState<TeacherAssignmentItemDto | null>(null);
  const [detailsAssignment, setDetailsAssignment] = useState<TeacherAssignmentItemDto | null>(null);

  const fetchAssignments = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getTeacherAssignments({
        title: titleFilter.trim() || undefined,
        className: classFilter.trim() || undefined,
        subjectCode: subjectFilter.trim() || undefined,
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
      console.error("Failed to load teacher assignments:", err);
      setError(formatDisplayError(err, "Failed to load assignments list."));
    } finally {
      setLoading(false);
    }
  }, [titleFilter, classFilter, subjectFilter, statusFilter, sortBy, sortOrder, pageNumber, pageSize]);

  useEffect(() => {
    fetchAssignments();
  }, [fetchAssignments]);

  const handleFilterReset = () => {
    setTitleFilter("");
    setClassFilter("");
    setSubjectFilter("");
    setStatusFilter("");
    setSortBy("duedate");
    setSortOrder("Asc");
    setPageNumber(1);
  };

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Navigation & Header */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Teacher Portal
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  My Class Assignments
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  Manage your created assignments, track student submissions, and review coursework.
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
                className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
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
                onClick={() => setIsCreateOpen(true)}
                className="inline-flex items-center gap-1.5 rounded-full bg-teal-600 px-4 py-2 text-xs font-semibold text-white shadow-md transition hover:bg-teal-700 active:scale-98 cursor-pointer whitespace-nowrap"
              >
                + Create Assignment
              </button>
              <button
                onClick={() => logoutUser()}
                className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
              >
                Logout 🚪
              </button>
            </nav>
          </div>
        </header>

        {/* Filter Controls Section */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.06)] backdrop-blur">
          <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-4">
            Search & Filter Assignments
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-5 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Assignment Title
              </label>
              <input
                type="text"
                placeholder="Search by title..."
                value={titleFilter}
                onChange={(e) => {
                  setTitleFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Class Name / Grade
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
                <option value="Active">Active</option>
                <option value="Draft">Draft</option>
                <option value="Past Due">Past Due</option>
                <option value="Archived">Archived</option>
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
                  <option value="duedate">Due Date</option>
                  <option value="title">Title</option>
                  <option value="classname">Class Name</option>
                  <option value="subjectname">Subject Name</option>
                  <option value="status">Status</option>
                  <option value="createdat">Created Date</option>
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
              Showing <strong>{items.length}</strong> of <strong>{totalCount}</strong> assignments
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

        {/* Assignments Table / List */}
        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-black/5">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">
                All Assignments ({totalCount})
              </h2>
              <p className="mt-0.5 text-xs text-(--color-muted)">
                View details, edit configuration, or monitor student submission progress.
              </p>
            </div>
            <button
              type="button"
              onClick={() => setIsCreateOpen(true)}
              className="inline-flex items-center gap-1.5 rounded-full bg-teal-600 px-4 py-2 text-xs font-semibold text-white shadow-md transition hover:bg-teal-700 active:scale-98 cursor-pointer whitespace-nowrap"
            >
              + Create Assignment
            </button>
          </div>

          <div className="mt-4 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-xs">
            <div className="max-h-[38rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur z-10">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Title & Description</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Class & Subject</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Due Date</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold whitespace-nowrap">Max Marks</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold whitespace-nowrap">Submissions</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold whitespace-nowrap">Status</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    Array.from({ length: pageSize }).map((_, idx) => (
                      <tr key={`skel-tr-${idx}`} className="animate-pulse">
                        <td className="border-b border-black/5 px-4 py-4"><div className="h-4 w-44 bg-slate-200 rounded-full"></div></td>
                        <td className="border-b border-black/5 px-4 py-4"><div className="h-4 w-32 bg-slate-200 rounded-full"></div></td>
                        <td className="border-b border-black/5 px-4 py-4"><div className="h-4 w-20 bg-slate-200 rounded-full"></div></td>
                        <td className="border-b border-black/5 px-4 py-4"><div className="h-4 w-16 bg-slate-200 rounded-full"></div></td>
                        <td className="border-b border-black/5 px-4 py-4"><div className="h-4 w-20 bg-slate-200 rounded-full"></div></td>
                        <td className="border-b border-black/5 px-4 py-4"><div className="h-4 w-16 bg-slate-200 rounded-full"></div></td>
                        <td className="border-b border-black/5 px-4 py-4 text-right"><div className="h-16 w-20 bg-slate-200 rounded-xl ml-auto"></div></td>
                      </tr>
                    ))
                  ) : items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="odd:bg-white even:bg-slate-50/70 hover:bg-slate-100/50 transition-colors">
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="flex items-start gap-3">
                            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-2xl bg-teal-50 border border-teal-200/80 text-teal-700 text-sm font-bold mt-0.5 shadow-2xs">
                              📝
                            </div>
                            <div>
                              <p className="font-semibold text-foreground leading-tight text-sm">{item.title}</p>
                              <p className="text-xs text-slate-500 mt-1 leading-relaxed" title={item.description}>
                                {item.description || "No description provided."}
                              </p>
                            </div>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="flex flex-col gap-1 items-start">
                            <div className="flex items-center gap-1.5">
                              <span className="font-semibold text-foreground text-sm leading-tight">{item.className}</span>
                              {item.classSection && (
                                <span className="inline-flex rounded-md bg-teal-500/10 border border-teal-500/20 px-1.5 py-0.5 text-[10px] font-semibold text-teal-700">
                                  Sec {item.classSection}
                                </span>
                              )}
                            </div>
                            <div className="mt-0.5">
                              <span className="inline-flex rounded-md border border-purple-500/20 bg-purple-500/10 px-2 py-0.5 text-[10px] font-mono font-bold text-purple-700">
                                {item.subjectCode}
                              </span>
                            </div>
                            <p className="text-xs font-medium text-slate-600 leading-tight mt-0.5" title={item.subjectName}>
                              {item.subjectName}
                            </p>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-xs font-medium">
                          {(() => {
                            const parts = formatDateParts(item.dueDate);
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
                        <td className="border-b border-black/5 px-4 py-4 whitespace-nowrap">
                          <div className="inline-flex items-center gap-1 rounded-xl border border-amber-200/80 bg-amber-50/80 px-2.5 py-1 text-xs font-bold text-amber-800 shadow-2xs">
                            🎯 {item.maxMarks} pts
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 whitespace-nowrap">
                          <div className="inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50/90 px-3 py-1 text-xs font-bold text-blue-800 shadow-2xs">
                            <span>📥</span>
                            <span>{item.totalSubmissions} submitted</span>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 whitespace-nowrap">
                          <span
                            className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.14em] ${
                              item.status === "Active" || item.status === "Published"
                                ? "border-emerald-500/20 bg-emerald-500/10 text-emerald-800"
                                : item.status === "Draft"
                                ? "border-slate-300 bg-slate-100 text-slate-700"
                                : item.status === "Past Due"
                                ? "border-rose-500/20 bg-rose-500/10 text-rose-800"
                                : "border-amber-500/20 bg-amber-500/10 text-amber-800"
                            }`}
                          >
                            {item.status}
                          </span>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="flex flex-col gap-1 items-end ml-auto w-24">
                            <button
                              type="button"
                              onClick={() => setDetailsAssignment(item)}
                              className="w-full inline-flex items-center justify-center gap-1 rounded-full border border-teal-300 bg-teal-50 px-2 py-0.5 text-xs font-medium text-teal-700 shadow-2xs hover:bg-teal-600 hover:text-white transition cursor-pointer"
                            >
                              👁️ Details
                            </button>
                            <button
                              type="button"
                              onClick={() => setEditingAssignment(item)}
                              className="w-full inline-flex items-center justify-center gap-1 rounded-full border border-slate-300 bg-white px-2 py-0.5 text-xs font-medium text-slate-800 shadow-2xs hover:bg-slate-900 hover:text-white transition cursor-pointer"
                            >
                              ✏️ Edit
                            </button>
                            <button
                              type="button"
                              onClick={() => setDeletingAssignment(item)}
                              className="w-full inline-flex items-center justify-center gap-1 rounded-full border border-rose-200 bg-rose-50 px-2 py-0.5 text-xs font-medium text-rose-700 shadow-2xs hover:bg-rose-600 hover:text-white transition cursor-pointer"
                            >
                              🗑️ Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={7} className="px-6 py-12 text-center text-slate-500">
                        No assignments found matching your filters.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination bar */}
          <div className="mt-4 flex items-center justify-between border-t border-black/5 pt-4 text-xs font-medium text-slate-600">
            <button
              disabled={pageNumber <= 1}
              onClick={() => setPageNumber((p) => p - 1)}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 hover:bg-slate-100 disabled:opacity-40 transition cursor-pointer shadow-2xs"
            >
              ← Previous
            </button>
            <span className="font-semibold text-slate-700">
              Page {pageNumber} of {totalPages}
            </span>
            <button
              disabled={pageNumber >= totalPages}
              onClick={() => setPageNumber((p) => p + 1)}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 hover:bg-slate-100 disabled:opacity-40 transition cursor-pointer shadow-2xs"
            >
              Next →
            </button>
          </div>
        </section>

        {/* Modal Dialogs */}
        <CreateAssignmentModal
          isOpen={isCreateOpen}
          onClose={() => setIsCreateOpen(false)}
          onSuccess={() => fetchAssignments()}
        />

        <EditAssignmentModal
          isOpen={!!editingAssignment}
          assignment={editingAssignment}
          onClose={() => setEditingAssignment(null)}
          onSuccess={() => fetchAssignments()}
        />

        <DeleteAssignmentModal
          isOpen={!!deletingAssignment}
          assignment={deletingAssignment}
          onClose={() => setDeletingAssignment(null)}
          onSuccess={() => fetchAssignments()}
        />

        <AssignmentDetailsModal
          isOpen={!!detailsAssignment}
          assignment={detailsAssignment}
          onClose={() => setDetailsAssignment(null)}
        />
      </div>
    </main>
  );
}
