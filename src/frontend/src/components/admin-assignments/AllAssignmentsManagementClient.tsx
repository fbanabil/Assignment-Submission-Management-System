"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { AssignmentDetailModal } from "./AssignmentDetailModal";
import {
  getAssignments,
  type AssignmentFilterDto,
  type AssignmentResponseDto,
  type PagedAssignmentResultDto,
} from "@/lib/admin-assignments";
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

function statusBadge(status: string) {
  switch (status) {
    case "Active":
      return "border-emerald-500/15 bg-emerald-500/10 text-emerald-700";
    case "Past Due":
      return "border-rose-500/15 bg-rose-500/10 text-rose-700";
    case "Draft":
      return "border-amber-500/15 bg-amber-500/10 text-amber-700";
    case "Published":
    default:
      return "border-blue-500/15 bg-blue-500/10 text-blue-700";
  }
}

export function AllAssignmentsManagementClient() {
  const [filter, setFilter] = useState<AssignmentFilterDto>({
    title: "",
    className: "",
    subjectName: "",
    subjectCode: "",
    teacherName: "",
    teacherEmail: "",
    status: "",
    sortBy: "duedate",
    sortOrder: "Asc",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedAssignmentResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Inspector Modal State
  const [selectedAssignment, setSelectedAssignment] = useState<AssignmentResponseDto | null>(null);

  const fetchAssignmentsData = useCallback(async (currentFilter: AssignmentFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getAssignments(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load system assignments:", err);
      setError(err instanceof Error ? err.message : "Unable to load system assignments data.");
    } finally {
      setLoading(false);
    }
  }, []);

  // Dynamically refetch whenever filters or pagination parameters change
  useEffect(() => {
    fetchAssignmentsData(filter);
  }, [filter, fetchAssignmentsData]);

  const handleInputChange =
    (field: keyof AssignmentFilterDto) => (e: React.ChangeEvent<HTMLInputElement>) => {
      setFilter((prev) => ({ ...prev, [field]: e.target.value, pageNumber: 1 }));
    };

  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFilter((prev) => ({ ...prev, status: e.target.value, pageNumber: 1 }));
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
      title: "",
      className: "",
      subjectName: "",
      subjectCode: "",
      teacherName: "",
      teacherEmail: "",
      status: "",
      pageNumber: 1,
      pageSize: 10,
    });
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Header section styled matching admin dashboard */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="max-w-2xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/20 bg-teal-500/10 px-3.5 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                <span className="h-2 w-2 rounded-full bg-teal-500 animate-pulse"></span>
                Assignments Directory
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  System-Wide Coursework Directory
                </h1>
                <p className="mt-2 text-sm leading-7 text-(--color-muted) sm:text-base">
                  Inspect and audit every assignment created across all classes, subjects, and teachers in the platform.
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
              className="rounded-full border border-black/10 bg-white px-4 py-2 text-foreground transition hover:border-black/20 hover:bg-black/2 whitespace-nowrap"
              href="/admin/teacher-assignments"
            >
              Teacher Assignments
            </Link>
            <Link
              className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
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
              {/* Title filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Assignment Title</label>
                <input
                  type="text"
                  placeholder="Filter by Title..."
                  value={filter.title || ""}
                  onChange={handleInputChange("title")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
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
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Subject filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Subject</label>
                <input
                  type="text"
                  placeholder="Subject Name..."
                  value={filter.subjectName || ""}
                  onChange={handleInputChange("subjectName")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Teacher Name filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Teacher Name</label>
                <input
                  type="text"
                  placeholder="Filter by Teacher..."
                  value={filter.teacherName || ""}
                  onChange={handleInputChange("teacherName")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Status filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Status</label>
                <select
                  value={filter.status || ""}
                  onChange={handleStatusChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
                >
                  <option value="">All Statuses</option>
                  <option value="Active">Active</option>
                  <option value="Past Due">Past Due</option>
                  <option value="Draft">Draft</option>
                  <option value="Published">Published</option>
                </select>
              </div>

              {/* Sort By selector */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Sort By</label>
                <div className="flex items-center gap-1.5">
                  <select
                    value={filter.sortBy || "duedate"}
                    onChange={(e) => {
                      const newFilter = { ...filter, sortBy: e.target.value, pageNumber: 1 };
                      setFilter(newFilter);
                      fetchAssignmentsData(newFilter);
                    }}
                    className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
                  >
                    <option value="duedate">Due Date</option>
                    <option value="title">Title</option>
                    <option value="classname">Class Name</option>
                    <option value="subjectname">Subject Name</option>
                    <option value="teachername">Teacher Name</option>
                    <option value="status">Status</option>
                    <option value="createdat">Created Date</option>
                  </select>
                  <button
                    type="button"
                    onClick={() => {
                      const newOrder: "Asc" | "Desc" = filter.sortOrder === "Asc" ? "Desc" : "Asc";
                      const newFilter: AssignmentFilterDto = { ...filter, sortOrder: newOrder, pageNumber: 1 };
                      setFilter(newFilter);
                      fetchAssignmentsData(newFilter);
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
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
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
                All Assignments ({pagedData?.totalCount ?? 0})
              </h2>
              <p className="mt-0.5 text-xs text-(--color-muted)">
                Audit system assignments. Click &quot;View Details&quot; for complete assignment specifications.
              </p>
            </div>
          </div>

          <div className="mt-4 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-xs">
            <div className="max-h-[38rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur z-10">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Assignment & Subject</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Class Section</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Assigned Teacher</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Due Date</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Status</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold text-right">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    Array.from({ length: filter.pageSize }).map((_, idx) => (
                      <tr key={`skel-${idx}`} className="animate-pulse">
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-40 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-32 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-36 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-24 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-20 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="ml-auto h-7 w-24 rounded-full bg-slate-200"></div>
                        </td>
                      </tr>
                    ))
                  ) : pagedData && pagedData.items.length > 0 ? (
                    pagedData.items.map((asg) => (
                      <tr
                        key={asg.id}
                        className="odd:bg-white even:bg-slate-50/70 hover:bg-slate-100/50 transition-colors"
                      >
                        <td className="border-b border-black/5 px-4 py-4">
                          <p className="font-semibold text-foreground leading-tight">{asg.title}</p>
                          <div className="flex items-center gap-2 mt-0.5">
                            <span className="text-xs font-medium text-slate-600">{asg.subjectName}</span>
                            <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-2 py-0.2 text-[10px] font-mono font-semibold text-purple-700">
                              {asg.subjectCode}
                            </span>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <p className="font-medium text-foreground">{asg.className}</p>
                          <div className="flex items-center gap-2 mt-0.5">
                            {asg.classSection && (
                              <span className="inline-flex rounded-full bg-teal-100 px-2 py-0.5 text-xs font-semibold text-teal-800">
                                {asg.classSection}
                              </span>
                            )}
                            {asg.academicYear && (
                              <span className="text-xs text-slate-500">({asg.academicYear})</span>
                            )}
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <p className="font-semibold text-foreground leading-tight">{asg.teacherName}</p>
                          <p className="text-xs text-slate-500">{asg.teacherEmail}</p>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-xs font-medium text-rose-600">
                          {formatDateTime(asg.dueDate)}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="flex flex-col gap-1">
                            <span
                              className={`inline-flex w-fit rounded-full border px-2.5 py-0.5 text-xs font-semibold uppercase tracking-[0.16em] ${statusBadge(
                                asg.status
                              )}`}
                            >
                              {asg.status}
                            </span>
                            <span className="text-[11px] font-medium text-slate-500">
                              {asg.totalSubmissions} submissions
                            </span>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <button
                            type="button"
                            onClick={() => setSelectedAssignment(asg)}
                            className="rounded-full border border-slate-300 bg-white px-3.5 py-1 text-xs font-medium text-slate-800 shadow-2xs hover:bg-slate-900 hover:text-white transition cursor-pointer"
                          >
                            View Details
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={6} className="px-4 py-12 text-center text-slate-500">
                        <div className="mx-auto max-w-sm space-y-2">
                          <p className="text-base font-semibold text-slate-700">No assignments match your criteria</p>
                          <p className="text-xs text-slate-500">
                            Try adjusting your Title, Class Name, Subject, Teacher, or Status filter settings.
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

      {/* Inspector Modal */}
      <AssignmentDetailModal
        isOpen={selectedAssignment !== null}
        assignment={selectedAssignment}
        onClose={() => setSelectedAssignment(null)}
      />
    </main>
  );
}
