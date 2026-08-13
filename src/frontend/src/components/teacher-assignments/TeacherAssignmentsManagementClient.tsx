"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getTeacherAssignments,
  type TeacherAssignmentItemDto,
} from "@/lib/teacher-assignments";
import { logoutUser } from "@/lib/auth";
import { CreateAssignmentModal } from "./CreateAssignmentModal";
import { EditAssignmentModal } from "./EditAssignmentModal";
import { DeleteAssignmentModal } from "./DeleteAssignmentModal";
import { AssignmentDetailsModal } from "./AssignmentDetailsModal";

function formatDate(dateStr?: string) {
  if (!dateStr) return "N/A";
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return dateStr;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(d);
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
        pageNumber,
        pageSize,
      });
      setItems(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages || 1);
    } catch (err) {
      console.error("Failed to load teacher assignments:", err);
      setError("Failed to load assignments list.");
    } finally {
      setLoading(false);
    }
  }, [titleFilter, classFilter, subjectFilter, statusFilter, pageNumber, pageSize]);

  useEffect(() => {
    fetchAssignments();
  }, [fetchAssignments]);

  const handleFilterReset = () => {
    setTitleFilter("");
    setClassFilter("");
    setSubjectFilter("");
    setStatusFilter("");
    setPageNumber(1);
  };

  return (
    <main className="min-h-screen bg-(--color-background) px-4 py-8 sm:px-8 font-sans">
      <div className="mx-auto max-w-7xl space-y-8">
        {/* Navigation & Header */}
        <header className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-black/5 pb-6">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3.5 py-1 text-xs font-semibold uppercase tracking-[0.2em] text-teal-700">
              Teacher Portal
            </span>
            <h1 className="mt-2 text-3xl font-extrabold tracking-tight text-foreground sm:text-4xl">
              My Class Assignments
            </h1>
            <p className="mt-1 text-sm text-slate-500">
              Manage your created assignments, track student submissions, and review coursework.
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <Link
              href="/teacher"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-50 transition"
            >
              ← Back to Dashboard
            </Link>
            <button
              onClick={() => setIsCreateOpen(true)}
              className="rounded-full bg-teal-600 px-5 py-2 text-xs font-semibold text-white shadow-md hover:bg-teal-700 transition cursor-pointer"
            >
              + Create Assignment
            </button>
            <button
              onClick={() => logoutUser()}
              className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-600 hover:text-white transition cursor-pointer"
            >
              Sign Out
            </button>
          </div>
        </header>

        {/* Filter Controls Section */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.06)] backdrop-blur">
          <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-4">
            Search & Filter Assignments
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
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
        <section className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50/80 text-xs font-semibold uppercase tracking-wider text-slate-500 border-b border-black/5">
                <tr>
                  <th className="px-6 py-4">Title & Description</th>
                  <th className="px-6 py-4">Class & Subject</th>
                  <th className="px-6 py-4">Due Date</th>
                  <th className="px-6 py-4">Max Marks</th>
                  <th className="px-6 py-4">Submissions</th>
                  <th className="px-6 py-4">Status</th>
                  <th className="px-6 py-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-black/5">
                {loading ? (
                  Array.from({ length: 4 }).map((_, idx) => (
                    <tr key={`skel-tr-${idx}`} className="animate-pulse">
                      <td className="px-6 py-4"><div className="h-4 w-40 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-28 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-24 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-12 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-12 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-16 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4 text-right"><div className="h-4 w-20 bg-slate-200 rounded-full ml-auto"></div></td>
                    </tr>
                  ))
                ) : items.length > 0 ? (
                  items.map((item) => (
                    <tr key={item.id} className="hover:bg-slate-50/60 transition">
                      <td className="px-6 py-4 max-w-xs">
                        <div className="font-semibold text-slate-900">{item.title}</div>
                        <p className="text-xs text-slate-500 truncate mt-0.5">{item.description}</p>
                      </td>
                      <td className="px-6 py-4">
                        <div className="font-medium text-slate-900">{item.className}</div>
                        <div className="inline-flex rounded-full bg-purple-100 px-2 py-0.5 text-[11px] font-mono font-semibold text-purple-800 mt-1">
                          {item.subjectCode} - {item.subjectName}
                        </div>
                      </td>
                      <td className="px-6 py-4 text-xs font-medium text-slate-600">
                        {formatDate(item.dueDate)}
                      </td>
                      <td className="px-6 py-4 font-semibold text-slate-800">
                        {item.maxMarks} pts
                      </td>
                      <td className="px-6 py-4">
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-bold text-slate-700">
                          {item.totalSubmissions}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <span
                          className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-bold ${
                            item.status === "Active" || item.status === "Published"
                              ? "bg-emerald-100 text-emerald-800"
                              : item.status === "Draft"
                              ? "bg-slate-200 text-slate-800"
                              : item.status === "Past Due"
                              ? "bg-rose-100 text-rose-800"
                              : "bg-amber-100 text-amber-800"
                          }`}
                        >
                          {item.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right space-x-2">
                        <button
                          onClick={() => setDetailsAssignment(item)}
                          className="rounded-full border border-teal-200 bg-teal-50 px-3 py-1 text-xs font-semibold text-teal-700 hover:bg-teal-600 hover:text-white transition cursor-pointer"
                        >
                          Details 👁️
                        </button>
                        <button
                          onClick={() => setEditingAssignment(item)}
                          className="rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold text-slate-700 hover:bg-slate-900 hover:text-white transition cursor-pointer"
                        >
                          Edit ✏️
                        </button>
                        <button
                          onClick={() => setDeletingAssignment(item)}
                          className="rounded-full border border-rose-200 bg-rose-50 px-3 py-1 text-xs font-semibold text-rose-700 hover:bg-rose-600 hover:text-white transition cursor-pointer"
                        >
                          Delete
                        </button>
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
