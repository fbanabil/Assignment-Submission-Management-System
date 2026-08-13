"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { CreateSubjectModal } from "./CreateSubjectModal";
import { EditSubjectModal } from "./EditSubjectModal";
import { DeleteSubjectModal } from "./DeleteSubjectModal";
import { LinkSubjectClassModal } from "./LinkSubjectClassModal";
import { getClasses, type ClassResponseDto } from "@/lib/admin-classes";
import {
  getSubjects,
  type PagedSubjectResultDto,
  type SubjectFilterDto,
  type SubjectResponseDto,
} from "@/lib/admin-subjects";

function formatDateTime(value?: string) {
  if (!value) return "Just now";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

export function SubjectManagementClient() {
  const [filter, setFilter] = useState<SubjectFilterDto>({
    name: "",
    code: "",
    classId: "",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedSubjectResultDto | null>(null);
  const [classList, setClassList] = useState<ClassResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals state
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [selectedSubjectToEdit, setSelectedSubjectToEdit] = useState<SubjectResponseDto | null>(null);
  const [selectedSubjectToDelete, setSelectedSubjectToDelete] = useState<SubjectResponseDto | null>(null);
  const [selectedSubjectToLink, setSelectedSubjectToLink] = useState<SubjectResponseDto | null>(null);

  // Toast notification
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const fetchSubjectsData = useCallback(async (currentFilter: SubjectFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getSubjects(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load subjects:", err);
      setError(err instanceof Error ? err.message : "Unable to load subjects data.");
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch classes for filter dropdown
  useEffect(() => {
    getClasses({ pageNumber: 1, pageSize: 100 })
      .then((res) => setClassList(res.items))
      .catch((err) => console.error("Failed to load class dropdown list:", err));
  }, []);

  // Dynamically refetch whenever filters or pagination parameters change
  useEffect(() => {
    fetchSubjectsData(filter);
  }, [filter, fetchSubjectsData]);

  const handleInputChange = (field: keyof SubjectFilterDto) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilter((prev) => ({ ...prev, [field]: e.target.value, pageNumber: 1 }));
  };

  const handleClassFilterChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFilter((prev) => ({ ...prev, classId: e.target.value, pageNumber: 1 }));
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
      code: "",
      classId: "",
      pageNumber: 1,
      pageSize: 10,
    });
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  const handleSubjectCreated = (newSubject: SubjectResponseDto) => {
    setToastMessage(`Subject "${newSubject.name}" (${newSubject.code}) created successfully!`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      const filtered = prev.items.filter((s) => s.id !== newSubject.id);
      return {
        ...prev,
        items: [newSubject, ...filtered],
        totalCount: prev.totalCount + 1,
      };
    });

    const resetFilterState: SubjectFilterDto = {
      name: "",
      code: "",
      classId: "",
      pageNumber: 1,
      pageSize: filter.pageSize,
    };
    setFilter(resetFilterState);
    fetchSubjectsData(resetFilterState);
  };

  const handleSubjectUpdated = (updatedSubject: SubjectResponseDto) => {
    setToastMessage(`Subject "${updatedSubject.name}" updated successfully.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.map((s) => (s.id === updatedSubject.id ? { ...s, ...updatedSubject } : s)),
      };
    });

    fetchSubjectsData(filter);
  };

  const handleSubjectDeleted = (deletedId: string) => {
    setToastMessage(`Subject deleted successfully.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic local update
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.filter((s) => s.id !== deletedId),
        totalCount: Math.max(0, prev.totalCount - 1),
      };
    });

    fetchSubjectsData(filter);
  };

  const handleSubjectLinksUpdated = (updatedSubject: SubjectResponseDto) => {
    setToastMessage(`Class links for "${updatedSubject.name}" updated.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Update in-memory page items and active modal reference
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.map((s) => (s.id === updatedSubject.id ? updatedSubject : s)),
      };
    });
    setSelectedSubjectToLink(updatedSubject);
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
              <div className="inline-flex items-center gap-2 rounded-full border border-purple-500/15 bg-purple-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-purple-700">
                Subject Management
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  Subjects & Class Mappings
                </h1>
                <p className="mt-3 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  Manage academic subjects, course codes, and map subjects to assigned class sections.
                </p>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-3 text-sm">
              <button
                type="button"
                onClick={() => setIsCreateModalOpen(true)}
                className="inline-flex items-center gap-2 rounded-full bg-foreground px-5 py-2.5 text-sm font-medium text-background shadow-md transition hover:opacity-90 active:scale-98 cursor-pointer"
              >
                <span className="text-base font-bold">+</span> Create Subject
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
              Refreshed: <strong className="text-foreground">{formatDateTime(pagedData?.fetchedAt)}</strong>
            </span>
          </div>
        </header>

        {/* Dynamic Filter Bar (Filtering using Name, Code, Linked Class) */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-slate-700">Filter Subjects</h2>
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
                <label className="block text-xs font-medium text-slate-500 mb-1">Subject Name</label>
                <input
                  type="text"
                  placeholder="Filter by Subject Name..."
                  value={filter.name || ""}
                  onChange={handleInputChange("name")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-purple-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Code filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Subject Code</label>
                <input
                  type="text"
                  placeholder="Filter by Code..."
                  value={filter.code || ""}
                  onChange={handleInputChange("code")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-purple-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Class filter dropdown */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Linked Class</label>
                <select
                  value={filter.classId || ""}
                  onChange={handleClassFilterChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground focus:border-purple-500 focus:outline-none transition shadow-2xs"
                >
                  <option value="">All Classes</option>
                  {classList.map((cls) => (
                    <option key={cls.id} value={cls.id}>
                      {cls.name} ({cls.section})
                    </option>
                  ))}
                </select>
              </div>

              {/* Page size selector */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Page Size</label>
                <select
                  value={filter.pageSize}
                  onChange={handlePageSizeChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground focus:border-purple-500 focus:outline-none transition shadow-2xs"
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
            <h3 className="font-semibold text-rose-800">Error loading subjects</h3>
            <p className="text-sm mt-1">{error}</p>
          </div>
        )}

        {/* Subject Table Section */}
        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-black/5">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">
                All Subjects ({pagedData?.totalCount ?? 0})
              </h2>
              <p className="mt-0.5 text-xs text-(--color-muted)">
                Filtered subject catalog. Click &quot;Link Classes&quot; to assign subjects to class sections.
              </p>
            </div>
          </div>

          <div className="mt-4 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-xs">
            <div className="max-h-[38rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur z-10">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Subject Name</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Code</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Linked Classes</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold text-right">Actions</th>
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
                          <div className="h-4 w-20 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-48 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="ml-auto h-7 w-32 rounded-full bg-slate-200"></div>
                        </td>
                      </tr>
                    ))
                  ) : pagedData && pagedData.items.length > 0 ? (
                    pagedData.items.map((sbj) => (
                      <tr
                        key={sbj.id}
                        className="odd:bg-white even:bg-slate-50/70 hover:bg-slate-100/50 transition-colors"
                      >
                        <td className="border-b border-black/5 px-4 py-4 font-semibold text-foreground">
                          {sbj.name}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <span className="inline-flex rounded-full border border-purple-500/15 bg-purple-500/10 px-3 py-0.5 text-xs font-mono font-semibold text-purple-700">
                            {sbj.code}
                          </span>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="flex flex-wrap items-center gap-1.5 max-w-md">
                            {sbj.linkedClasses && sbj.linkedClasses.length > 0 ? (
                              sbj.linkedClasses.map((cls) => (
                                <span
                                  key={cls.id}
                                  className="inline-flex items-center gap-1 rounded-full border border-slate-200 bg-white px-2.5 py-0.5 text-xs font-medium text-slate-700 shadow-2xs"
                                >
                                  <span>{cls.name}</span>
                                  {cls.section && (
                                    <span className="font-semibold text-teal-700">({cls.section})</span>
                                  )}
                                </span>
                              ))
                            ) : (
                              <span className="text-xs italic text-slate-400">No classes linked yet</span>
                            )}
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="inline-flex items-center justify-end gap-2">
                            <button
                              type="button"
                              onClick={() => setSelectedSubjectToLink(sbj)}
                              className="rounded-full border border-purple-200 bg-purple-50 px-3.5 py-1 text-xs font-semibold text-purple-700 shadow-2xs hover:bg-purple-600 hover:text-white transition cursor-pointer"
                            >
                              Link Classes ({sbj.linkedClasses?.length || 0})
                            </button>
                            <button
                              type="button"
                              onClick={() => setSelectedSubjectToEdit(sbj)}
                              className="rounded-full border border-slate-300 bg-white px-3.5 py-1 text-xs font-medium text-slate-800 shadow-2xs hover:bg-slate-900 hover:text-white transition cursor-pointer"
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              onClick={() => setSelectedSubjectToDelete(sbj)}
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
                      <td colSpan={4} className="px-4 py-12 text-center text-slate-500">
                        <div className="mx-auto max-w-sm space-y-2">
                          <p className="text-base font-semibold text-slate-700">No subjects match your criteria</p>
                          <p className="text-xs text-slate-500">
                            Try adjusting your Name, Code, or Linked Class filter settings.
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

      {/* Create Subject Modal */}
      <CreateSubjectModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSuccess={handleSubjectCreated}
      />

      {/* Edit Subject Modal */}
      <EditSubjectModal
        isOpen={selectedSubjectToEdit !== null}
        subjectData={selectedSubjectToEdit}
        onClose={() => setSelectedSubjectToEdit(null)}
        onSuccess={handleSubjectUpdated}
      />

      {/* Delete Subject Modal */}
      <DeleteSubjectModal
        isOpen={selectedSubjectToDelete !== null}
        subjectData={selectedSubjectToDelete}
        onClose={() => setSelectedSubjectToDelete(null)}
        onSuccess={handleSubjectDeleted}
      />

      {/* Link Subject to Class Modal */}
      <LinkSubjectClassModal
        isOpen={selectedSubjectToLink !== null}
        subjectData={selectedSubjectToLink}
        onClose={() => setSelectedSubjectToLink(null)}
        onUpdated={handleSubjectLinksUpdated}
      />
    </main>
  );
}
