"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { CreateClassModal } from "./CreateClassModal";
import { EditClassModal } from "./EditClassModal";
import { DeleteClassModal } from "./DeleteClassModal";
import {
  getClasses,
  type ClassFilterDto,
  type ClassResponseDto,
  type PagedClassResultDto,
} from "@/lib/admin-classes";

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

export function ClassManagementClient() {
  const [filter, setFilter] = useState<ClassFilterDto>({
    name: "",
    section: "",
    academicYear: "",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedClassResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals state
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [selectedClassToEdit, setSelectedClassToEdit] = useState<ClassResponseDto | null>(null);
  const [selectedClassToDelete, setSelectedClassToDelete] = useState<ClassResponseDto | null>(null);

  // Notification message
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const fetchClassesData = useCallback(async (currentFilter: ClassFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getClasses(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load classes:", err);
      setError(err instanceof Error ? err.message : "Unable to load classes data.");
    } finally {
      setLoading(false);
    }
  }, []);

  // Dynamically refetch whenever filters or pagination parameters change
  useEffect(() => {
    fetchClassesData(filter);
  }, [filter, fetchClassesData]);

  const handleInputChange = (field: keyof ClassFilterDto) => (e: React.ChangeEvent<HTMLInputElement>) => {
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
      name: "",
      section: "",
      academicYear: "",
      pageNumber: 1,
      pageSize: 10,
    });
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  const handleClassCreated = (newClass: ClassResponseDto) => {
    setToastMessage(`Class "${newClass.name}" (${newClass.section}) created successfully!`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      const filtered = prev.items.filter((c) => c.id !== newClass.id);
      return {
        ...prev,
        items: [newClass, ...filtered],
        totalCount: prev.totalCount + 1,
      };
    });

    // Reset filters to Page 1 and refetch
    const resetFilterState: ClassFilterDto = {
      name: "",
      section: "",
      academicYear: "",
      pageNumber: 1,
      pageSize: filter.pageSize,
    };
    setFilter(resetFilterState);
    fetchClassesData(resetFilterState);
  };

  const handleClassUpdated = (updatedClass: ClassResponseDto) => {
    setToastMessage(`Class "${updatedClass.name}" updated successfully.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.map((c) => (c.id === updatedClass.id ? updatedClass : c)),
      };
    });

    fetchClassesData(filter);
  };

  const handleClassDeleted = (deletedId: string) => {
    setToastMessage(`Class section deleted successfully.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.filter((c) => c.id !== deletedId),
        totalCount: Math.max(0, prev.totalCount - 1),
      };
    });

    fetchClassesData(filter);
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

        {/* Header section styled like admin-dashboard */}
        <header className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) px-6 py-6 shadow-[0_24px_80px_rgba(15,23,42,0.12)] backdrop-blur sm:px-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                Class Management
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Classes & Sections Directory
                </h1>
                <p className="mt-3 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  Manage academic classes, sections, and academic year assignments across your institution.
                </p>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-3 text-sm">
              <button
                type="button"
                onClick={() => setIsCreateModalOpen(true)}
                className="inline-flex items-center gap-2 rounded-full bg-foreground px-5 py-2.5 text-sm font-medium text-background shadow-md transition hover:opacity-90 active:scale-98 cursor-pointer"
              >
                <span className="text-base font-bold">+</span> Create Class
              </button>
              <Link
                className="rounded-full border border-black/10 bg-white px-4 py-2.5 font-medium text-foreground shadow-sm transition hover:border-black/20 hover:bg-black/2"
                href="/admin"
              >
                Back to dashboard
              </Link>
            </div>
          </div>

          <div className="mt-6 flex flex-wrap items-center justify-between border-t border-black/5 pt-4 text-xs font-medium text-slate-500 gap-2">
            <span>
              Source: <strong className="text-foreground">{pagedData?.dataSource || "Server API"}</strong>
            </span>
            <span>
              Refreshed:{" "}
              <strong className="text-foreground">
                {pagedData?.fetchedAt ? formatDateTime(pagedData.fetchedAt) : "Just now"}
              </strong>
            </span>
          </div>
        </header>

        {/* Dynamic Filter Bar (Filtering using Name, Section, Academic Year) */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-slate-700">Filter Classes</h2>
              <button
                onClick={handleResetFilters}
                className="rounded-full border border-slate-200 bg-white/90 px-4 py-1.5 text-xs font-semibold uppercase tracking-wider text-slate-600 hover:bg-slate-100 transition shadow-2xs"
              >
                Reset All Filters
              </button>
            </div>

            {/* Field Filters Grid */}
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {/* Name filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Class Name</label>
                <input
                  type="text"
                  placeholder="Filter by Class Name..."
                  value={filter.name || ""}
                  onChange={handleInputChange("name")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Section filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Section</label>
                <input
                  type="text"
                  placeholder="Filter by Section..."
                  value={filter.section || ""}
                  onChange={handleInputChange("section")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Academic Year filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Academic Year</label>
                <input
                  type="text"
                  placeholder="Filter by Academic Year..."
                  value={filter.academicYear || ""}
                  onChange={handleInputChange("academicYear")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Page size selector */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Page Size</label>
                <select
                  value={filter.pageSize}
                  onChange={handlePageSizeChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
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
          <div className="rounded-3xl border border-rose-200 bg-rose-50/90 p-5 shadow-sm text-rose-700">
            <h3 className="font-semibold text-rose-800">Error loading classes</h3>
            <p className="text-sm mt-1">{error}</p>
          </div>
        )}

        {/* Classes Table Section */}
        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-black/5">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">
                All Classes ({pagedData?.totalCount ?? 0})
              </h2>
              <p className="mt-0.5 text-xs text-(--color-muted)">
                Dynamically filtered class records. Click &quot;Edit&quot; or &quot;Delete&quot; to manage sections.
              </p>
            </div>
          </div>

          <div className="mt-4 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-xs">
            <div className="max-h-[38rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur z-10">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Class Name</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Section</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Academic Year</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Created Date</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold text-right">Actions</th>
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
                          <div className="h-4 w-20 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-24 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-28 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="ml-auto h-7 w-28 rounded-full bg-slate-200"></div>
                        </td>
                      </tr>
                    ))
                  ) : pagedData && pagedData.items.length > 0 ? (
                    pagedData.items.map((cls) => (
                      <tr
                        key={cls.id}
                        className="odd:bg-white even:bg-slate-50/70 hover:bg-slate-100/50 transition-colors"
                      >
                        <td className="border-b border-black/5 px-4 py-4 font-semibold text-foreground">
                          {cls.name}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <span className="inline-flex rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] text-teal-700">
                            {cls.section}
                          </span>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 font-medium text-slate-700">
                          {cls.academicYear}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-slate-500 text-xs">
                          {formatDateTime(cls.createdAt)}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="inline-flex items-center justify-end gap-2">
                            <button
                              type="button"
                              onClick={() => setSelectedClassToEdit(cls)}
                              className="rounded-full border border-slate-300 bg-white px-3.5 py-1 text-xs font-medium text-slate-800 shadow-2xs hover:bg-slate-900 hover:text-white transition cursor-pointer"
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              onClick={() => setSelectedClassToDelete(cls)}
                              className="rounded-full border border-rose-200 bg-rose-50 px-3.5 py-1 text-xs font-medium text-rose-700 shadow-2xs hover:bg-rose-600 hover:text-white transition cursor-pointer"
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="px-4 py-12 text-center text-slate-500">
                        <div className="mx-auto max-w-sm space-y-2">
                          <p className="text-base font-semibold text-slate-700">No classes match your criteria</p>
                          <p className="text-xs text-slate-500">
                            Try adjusting your Name, Section, or Academic Year filter settings.
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

      {/* Create Class Modal */}
      <CreateClassModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSuccess={handleClassCreated}
      />

      {/* Edit Class Modal */}
      <EditClassModal
        isOpen={selectedClassToEdit !== null}
        classData={selectedClassToEdit}
        onClose={() => setSelectedClassToEdit(null)}
        onSuccess={handleClassUpdated}
      />

      {/* Delete Class Modal */}
      <DeleteClassModal
        isOpen={selectedClassToDelete !== null}
        classData={selectedClassToDelete}
        onClose={() => setSelectedClassToDelete(null)}
        onSuccess={handleClassDeleted}
      />
    </main>
  );
}
