"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { AssignTeacherModal } from "./AssignTeacherModal";
import { DeleteTeacherAssignmentModal } from "./DeleteTeacherAssignmentModal";
import {
  getTeacherAssignments,
  type PagedTeacherAssignmentResultDto,
  type TeacherAssignmentFilterDto,
  type TeacherAssignmentResponseDto,
} from "@/lib/admin-teacher-assignments";
import { logoutUser } from "@/lib/auth";

function formatDateTime(value?: string) {
  if (!value) return "Just now";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

export function TeacherAssignmentManagementClient() {
  const [filter, setFilter] = useState<TeacherAssignmentFilterDto>({
    teacherName: "",
    teacherEmail: "",
    className: "",
    subjectCode: "",
    sortBy: "teachername",
    sortOrder: "Asc",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedTeacherAssignmentResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals state
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [selectedAssignmentToDelete, setSelectedAssignmentToDelete] =
    useState<TeacherAssignmentResponseDto | null>(null);

  // Toast notification
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const fetchAssignmentsData = useCallback(async (currentFilter: TeacherAssignmentFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getTeacherAssignments(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load teacher assignments:", err);
      setError(err instanceof Error ? err.message : "Unable to load teacher assignments data.");
    } finally {
      setLoading(false);
    }
  }, []);

  // Dynamically refetch whenever filters or pagination parameters change
  useEffect(() => {
    fetchAssignmentsData(filter);
  }, [filter, fetchAssignmentsData]);

  const handleInputChange =
    (field: keyof TeacherAssignmentFilterDto) => (e: React.ChangeEvent<HTMLInputElement>) => {
      setFilter((prev) => ({ ...prev, [field]: e.target.value, pageNumber: 1 }));
    };

  const handlePageSizeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFilter((prev) => ({
      ...prev,
      pageSize: Number(e.target.value),
      pageNumber: 1,
    }));
  };

  const handleResetFilters = () => {
    setFilter({
      teacherName: "",
      teacherEmail: "",
      className: "",
      subjectCode: "",
      pageNumber: 1,
      pageSize: 10,
    });
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  const handleAssignmentCreated = (newAssignment: TeacherAssignmentResponseDto) => {
    setToastMessage(
      `Assigned ${newAssignment.teacherName} to teach ${newAssignment.subjectName} (${newAssignment.className})!`
    );
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      const filtered = prev.items.filter((a) => a.id !== newAssignment.id);
      return {
        ...prev,
        items: [newAssignment, ...filtered],
        totalCount: prev.totalCount + 1,
      };
    });

    const resetFilterState: TeacherAssignmentFilterDto = {
      teacherName: "",
      teacherEmail: "",
      className: "",
      subjectCode: "",
      pageNumber: 1,
      pageSize: filter.pageSize,
    };
    setFilter(resetFilterState);
    fetchAssignmentsData(resetFilterState);
  };

  const handleAssignmentDeleted = (deletedId: string) => {
    setToastMessage(`Teacher assignment removed successfully.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.filter((a) => a.id !== deletedId),
        totalCount: Math.max(0, prev.totalCount - 1),
      };
    });

    fetchAssignmentsData(filter);
  };

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Toast Notification */}
        {toastMessage && (
          <div className="fixed bottom-6 right-6 z-50 flex items-center gap-3 rounded-2xl border border-emerald-300 bg-emerald-900/90 text-white px-5 py-3 shadow-xl backdrop-blur animate-in slide-in-from-bottom duration-300">
            <span className="text-lg">✓</span>
            <span className="text-sm font-medium">{toastMessage}</span>
            <button
              onClick={() => setToastMessage(null)}
              className="ml-2 text-xs opacity-70 hover:opacity-100"
            >
              ✕
            </button>
          </div>
        )}

        {/* Header section styled matching admin dashboard */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="max-w-2xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-blue-500/20 bg-blue-500/10 px-3.5 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-blue-700">
                <span className="h-2 w-2 rounded-full bg-blue-500 animate-pulse"></span>
                Teacher Assignments
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Teacher & Course Allocations
                </h1>
                <p className="mt-2 text-sm leading-7 text-(--color-muted) sm:text-base">
                  Assign qualified teachers to specific class section and subject pairs across academic terms.
                </p>
              </div>
            </div>

            {/* Refreshed info */}
            <div className="flex flex-wrap items-center gap-3 text-xs font-medium text-slate-500 shrink-0">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-black/5 bg-white px-3.5 py-1.5 font-medium text-foreground shadow-2xs">
                Refreshed: <strong className="text-foreground">{formatDateTime(pagedData?.fetchedAt)}</strong>
              </span>
            </div>
          </div>

          <nav className="mt-6 flex flex-wrap items-center gap-1.5 sm:gap-2 border-t border-black/5 pt-5 text-xs sm:text-sm font-medium shrink-0">
            <Link
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin"
            >
              Dashboard
            </Link>
            <Link
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin/users"
            >
              User Management
            </Link>
            <Link
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin/classes"
            >
              Class Management
            </Link>
            <Link
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin/subjects"
            >
              Subject Management
            </Link>
            <Link
              className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
              href="/admin/teacher-assignments"
            >
              Teacher Assignments
            </Link>
            <Link
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin/assignments"
            >
              All Assignments
            </Link>
            <Link
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin/submissions"
            >
              All Submissions
            </Link>
            <button
              type="button"
              onClick={() => setIsAssignModalOpen(true)}
              className="inline-flex items-center gap-1.5 rounded-full bg-teal-600 px-4 py-2 text-xs font-semibold text-white shadow-md transition hover:bg-teal-700 active:scale-98 cursor-pointer whitespace-nowrap"
            >
              + Assign Teacher
            </button>
            <button
              onClick={() => logoutUser()}
              className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
            >
              Logout 🚪
            </button>
          </nav>
        </header>

        {/* Dynamic Filter Bar */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-slate-700">Filter Assignments</h2>
              <button
                onClick={handleResetFilters}
                className="rounded-full border border-slate-200 bg-white/90 px-4 py-1.5 text-xs font-semibold uppercase tracking-wider text-slate-600 hover:bg-slate-100 transition shadow-2xs"
              >
                Reset All Filters
              </button>
            </div>

            {/* Field Filters Grid */}
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-6">
              {/* Teacher Name filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Teacher Name</label>
                <input
                  type="text"
                  placeholder="Filter by Name..."
                  value={filter.teacherName || ""}
                  onChange={handleInputChange("teacherName")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-blue-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Teacher Email filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Teacher Email</label>
                <input
                  type="text"
                  placeholder="Filter by Email..."
                  value={filter.teacherEmail || ""}
                  onChange={handleInputChange("teacherEmail")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-blue-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Class Name filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Class Name</label>
                <input
                  type="text"
                  placeholder="Filter by Class..."
                  value={filter.className || ""}
                  onChange={handleInputChange("className")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-blue-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Subject Code filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Subject Code</label>
                <input
                  type="text"
                  placeholder="Filter by Code..."
                  value={filter.subjectCode || ""}
                  onChange={handleInputChange("subjectCode")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-blue-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Sort By selector */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Sort By</label>
                <div className="flex items-center gap-1.5">
                  <select
                    value={filter.sortBy || "teachername"}
                    onChange={(e) => {
                      setFilter((prev) => ({ ...prev, sortBy: e.target.value, pageNumber: 1 }));
                    }}
                    className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground focus:border-blue-500 focus:outline-none transition shadow-2xs"
                  >
                    <option value="teachername">Teacher Name</option>
                    <option value="classname">Class Name</option>
                    <option value="subjectcode">Subject Code</option>
                    <option value="subjectname">Subject Name</option>
                    <option value="teacheremail">Teacher Email</option>
                  </select>
                  <button
                    type="button"
                    onClick={() => {
                      setFilter((prev) => ({ ...prev, sortOrder: prev.sortOrder === "Asc" ? "Desc" : "Asc", pageNumber: 1 }));
                    }}
                    className="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-2xl border border-slate-200 bg-white text-xs font-bold text-slate-700 shadow-2xs hover:bg-slate-100 transition cursor-pointer"
                    title={`Sort Order: ${filter.sortOrder === "Asc" ? "Ascending" : "Descending"}`}
                  >
                    {filter.sortOrder === "Asc" ? "⬆️" : "⬇️"}
                  </button>
                </div>
              </div>

              {/* Page size selector */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Page Size</label>
                <select
                  value={filter.pageSize}
                  onChange={handlePageSizeChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground focus:border-blue-500 focus:outline-none transition shadow-2xs"
                >
                  <option value={5}>5 per page</option>
                  <option value={10}>10 per page</option>
                  <option value={20}>20 per page</option>
                  <option value={50}>50 per page</option>
                </select>
              </div>
            </div>
          </div>
        </section>

        {/* Error banner */}
        {error && (
          <div className="rounded-3xl border border-rose-200 bg-rose-50/90 p-4 shadow-sm text-rose-700">
            <p className="text-sm font-semibold">{error}</p>
          </div>
        )}

        {/* Assignments Table Section */}
        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-black/5">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">
                All Teacher Assignments ({pagedData?.totalCount ?? 0})
              </h2>
              <p className="mt-0.5 text-xs text-(--color-muted)">
                Active teacher allocations mapped to class section & subject pairs.
              </p>
            </div>
          </div>

          <div className="mt-4 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-xs">
            <div className="max-h-[38rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur z-10">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Teacher</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Subject</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Class Section</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Assigned Date</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold text-right">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    Array.from({ length: filter.pageSize }).map((_, idx) => (
                      <tr key={`skel-${idx}`} className="animate-pulse">
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-36 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-28 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-40 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-24 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="ml-auto h-7 w-24 rounded-full bg-slate-200"></div>
                        </td>
                      </tr>
                    ))
                  ) : pagedData && pagedData.items.length > 0 ? (
                    pagedData.items.map((item) => (
                      <tr
                        key={item.id}
                        className="odd:bg-white even:bg-slate-50/70 hover:bg-slate-100/50 transition-colors"
                      >
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="flex items-center gap-3">
                            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-blue-100 text-xs font-bold text-blue-700">
                              {item.teacherName.slice(0, 2).toUpperCase()}
                            </div>
                            <div>
                              <p className="font-semibold text-foreground leading-tight">{item.teacherName}</p>
                              <p className="text-xs text-slate-500">{item.teacherEmail}</p>
                            </div>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <p className="font-semibold text-foreground">{item.subjectName}</p>
                          <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-2.5 py-0.5 text-xs font-mono font-semibold text-purple-700">
                            {item.subjectCode}
                          </span>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <p className="font-medium text-foreground">{item.className}</p>
                          <div className="flex items-center gap-2 mt-0.5">
                            {item.classSection && (
                              <span className="inline-flex rounded-full bg-teal-100 px-2.5 py-0.5 text-xs font-semibold text-teal-800">
                                {item.classSection}
                              </span>
                            )}
                            {item.academicYear && (
                              <span className="text-xs text-slate-500">({item.academicYear})</span>
                            )}
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-slate-500 text-xs">
                          {formatDateTime(item.assignedAt)}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <button
                            type="button"
                            onClick={() => setSelectedAssignmentToDelete(item)}
                            className="rounded-full border border-rose-200 bg-rose-50 px-3.5 py-1 text-xs font-medium text-rose-700 shadow-2xs hover:bg-rose-600 hover:text-white transition cursor-pointer"
                          >
                            Remove Assignment
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="px-4 py-12 text-center text-slate-500">
                        <div className="mx-auto max-w-sm space-y-2">
                          <p className="text-base font-semibold text-slate-700">No teacher assignments match your criteria</p>
                          <p className="text-xs text-slate-500">
                            Try adjusting your Teacher Name, Teacher Email, Class Name, or Subject Code filters.
                          </p>
                          <button
                            onClick={handleResetFilters}
                            className="mt-3 rounded-full bg-slate-900 px-4 py-1.5 text-xs font-medium text-white hover:bg-slate-800 transition"
                          >
                            Clear All Filters
                          </button>
                        </div>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination Controls */}
          {pagedData && pagedData.totalPages > 0 && (
            <div className="mt-5 flex flex-col sm:flex-row items-center justify-between gap-4 border-t border-black/5 pt-4">
              <div className="text-xs font-medium text-slate-500">
                Showing{" "}
                <span className="font-semibold text-foreground">
                  {pagedData.totalCount === 0 ? 0 : (pagedData.pageNumber - 1) * pagedData.pageSize + 1}
                </span>{" "}
                to{" "}
                <span className="font-semibold text-foreground">
                  {Math.min(pagedData.pageNumber * pagedData.pageSize, pagedData.totalCount)}
                </span>{" "}
                of <span className="font-semibold text-foreground">{pagedData.totalCount}</span> entries
              </div>

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  disabled={!pagedData.hasPreviousPage || loading}
                  onClick={() => handlePageChange(pagedData.pageNumber - 1)}
                  className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition"
                >
                  ← Previous
                </button>

                <span className="px-3 text-xs font-semibold text-slate-600">
                  Page {pagedData.pageNumber} of {pagedData.totalPages}
                </span>

                <button
                  type="button"
                  disabled={!pagedData.hasNextPage || loading}
                  onClick={() => handlePageChange(pagedData.pageNumber + 1)}
                  className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition"
                >
                  Next →
                </button>
              </div>
            </div>
          )}
        </section>
      </div>

      {/* Assign Teacher Modal */}
      <AssignTeacherModal
        isOpen={isAssignModalOpen}
        onClose={() => setIsAssignModalOpen(false)}
        onSuccess={handleAssignmentCreated}
      />

      {/* Delete Teacher Assignment Modal */}
      <DeleteTeacherAssignmentModal
        isOpen={selectedAssignmentToDelete !== null}
        assignmentData={selectedAssignmentToDelete}
        onClose={() => setSelectedAssignmentToDelete(null)}
        onSuccess={handleAssignmentDeleted}
      />
    </main>
  );
}
