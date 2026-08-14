"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { CreateEnrollmentModal } from "./CreateEnrollmentModal";
import {
  getTeacherEnrollments,
  removeEnrollment,
  type PagedStudentEnrollmentResultDto,
  type StudentEnrollmentFilterDto,
  type StudentEnrollmentResponseDto,
} from "@/lib/teacher-enrollments";

function formatDateTime(value?: string) {
  if (!value) return "N/A";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
  }).format(date);
}

export function TeacherEnrollmentsClient() {
  const [filter, setFilter] = useState<StudentEnrollmentFilterDto>({
    studentName: "",
    className: "",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedStudentEnrollmentResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals & Actions
  const [isEnrollModalOpen, setIsEnrollModalOpen] = useState(false);
  const [selectedEnrollmentToDelete, setSelectedEnrollmentToDelete] = useState<StudentEnrollmentResponseDto | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const fetchEnrollments = useCallback(async (currentFilter: StudentEnrollmentFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getTeacherEnrollments(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load enrollments:", err);
      setError(err instanceof Error ? err.message : "Unable to load student enrollments.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchEnrollments(filter);
  }, [filter, fetchEnrollments]);

  const handleSearchChange = (field: "studentName" | "className") => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilter((prev) => ({ ...prev, [field]: e.target.value, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  const handleDeleteConfirm = async () => {
    if (!selectedEnrollmentToDelete) return;
    setDeleting(true);
    try {
      await removeEnrollment(selectedEnrollmentToDelete.id);
      setToastMessage(`Removed ${selectedEnrollmentToDelete.studentName} from ${selectedEnrollmentToDelete.className}`);
      setSelectedEnrollmentToDelete(null);
      fetchEnrollments(filter);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove enrollment.");
    } finally {
      setDeleting(false);
    }
  };

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
        {/* Header */}
        <header className="overflow-hidden rounded-3xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl space-y-3">
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-teal-700">
                Teacher Management
              </div>
              <div>
                <h1 className="text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">
                  Student Enrollments
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600 sm:text-base">
                  Enroll students into your assigned classes and manage your class rosters.
                </p>
              </div>
            </div>

            {/* Actions */}
            <div className="flex flex-wrap items-center gap-3">
              <Link
                href="/teacher"
                className="rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-100"
              >
                Dashboard
              </Link>
              <button
                onClick={() => setIsEnrollModalOpen(true)}
                className="inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-indigo-700"
              >
                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Enroll Student
              </button>
            </div>
          </div>
        </header>

        {/* Toast Notification */}
        {toastMessage && (
          <div className="flex items-center justify-between rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-emerald-800">
            <span className="text-sm font-medium">{toastMessage}</span>
            <button onClick={() => setToastMessage(null)} className="text-xs font-bold text-emerald-600 hover:underline">
              Dismiss
            </button>
          </div>
        )}

        {/* Filter Bar */}
        <section className="rounded-3xl border border-slate-200 bg-white p-4 shadow-sm sm:p-6">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1">
                Filter by Student Name
              </label>
              <input
                type="text"
                value={filter.studentName || ""}
                onChange={handleSearchChange("studentName")}
                placeholder="Search student name..."
                className="w-full rounded-xl border border-slate-200 px-3.5 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-hidden"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1">
                Filter by Class Name
              </label>
              <input
                type="text"
                value={filter.className || ""}
                onChange={handleSearchChange("className")}
                placeholder="Search class name..."
                className="w-full rounded-xl border border-slate-200 px-3.5 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-hidden"
              />
            </div>

            <div className="flex items-end justify-end">
              <button
                onClick={() => fetchEnrollments(filter)}
                disabled={loading}
                className="w-full sm:w-auto rounded-xl border border-slate-200 bg-slate-50 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
              >
                Refresh List
              </button>
            </div>
          </div>
        </section>

        {/* Enrollments Table */}
        <section className="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
          {loading ? (
            <div className="flex h-64 items-center justify-center text-sm text-slate-400">
              Loading student enrollments...
            </div>
          ) : !pagedData?.items || pagedData.items.length === 0 ? (
            <div className="flex h-64 flex-col items-center justify-center p-6 text-center">
              <svg className="h-10 w-10 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <p className="mt-2 text-sm font-semibold text-slate-700">No student enrollments found</p>
              <p className="text-xs text-slate-500">Click "Enroll Student" to add students to your class roster.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-slate-700">
                <thead className="border-b border-slate-100 bg-slate-50/80 text-xs uppercase tracking-wider text-slate-500">
                  <tr>
                    <th className="px-6 py-4 font-semibold">Student</th>
                    <th className="px-6 py-4 font-semibold">Class Name</th>
                    <th className="px-6 py-4 font-semibold">Section</th>
                    <th className="px-6 py-4 font-semibold">Academic Year</th>
                    <th className="px-6 py-4 font-semibold">Enrolled Date</th>
                    <th className="px-6 py-4 text-right font-semibold">Action</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {pagedData.items.map((item: StudentEnrollmentResponseDto) => (
                    <tr key={item.id} className="hover:bg-slate-50/60">
                      <td className="px-6 py-4 font-medium text-slate-900">
                        <div>{item.studentName || "Student"}</div>
                        <div className="text-xs text-slate-400">{item.studentEmail}</div>
                      </td>
                      <td className="px-6 py-4">
                        <span className="rounded-lg bg-indigo-50 px-2.5 py-1 text-xs font-semibold text-indigo-700">
                          {item.className}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-slate-600">{item.classSection || "-"}</td>
                      <td className="px-6 py-4 text-slate-600">{item.academicYear || "Current"}</td>
                      <td className="px-6 py-4 text-slate-500">{formatDateTime(item.enrolledAt)}</td>
                      <td className="px-6 py-4 text-right">
                        <button
                          onClick={() => setSelectedEnrollmentToDelete(item)}
                          className="rounded-lg border border-rose-200 bg-rose-50 px-3 py-1.5 text-xs font-semibold text-rose-700 hover:bg-rose-100"
                        >
                          Disenroll
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Pagination Footer */}
          {pagedData && pagedData.totalPages > 1 && (
            <div className="flex items-center justify-between border-t border-slate-100 px-6 py-4 text-xs text-slate-500">
              <span>
                Page {pagedData.pageNumber} of {pagedData.totalPages} ({pagedData.totalCount} total students)
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

        {/* Modal Components */}
        <CreateEnrollmentModal
          isOpen={isEnrollModalOpen}
          onClose={() => setIsEnrollModalOpen(false)}
          onSuccess={() => {
            setToastMessage("Student successfully enrolled in class.");
            fetchEnrollments(filter);
          }}
        />

        {/* Disenroll Confirmation Modal */}
        {selectedEnrollmentToDelete && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4 backdrop-blur-xs">
            <div className="w-full max-w-sm rounded-3xl border border-slate-200 bg-white p-6 shadow-xl text-center">
              <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-rose-100 text-rose-600">
                <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
              <h3 className="text-lg font-bold text-slate-900">Remove Student Enrollment?</h3>
              <p className="mt-2 text-xs text-slate-500">
                Are you sure you want to remove <span className="font-semibold text-slate-800">{selectedEnrollmentToDelete.studentName}</span> from class <span className="font-semibold text-slate-800">{selectedEnrollmentToDelete.className}</span>?
              </p>
              <div className="mt-6 flex items-center justify-center gap-3">
                <button
                  onClick={() => setSelectedEnrollmentToDelete(null)}
                  disabled={deleting}
                  className="rounded-xl border border-slate-200 px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDeleteConfirm}
                  disabled={deleting}
                  className="rounded-xl bg-rose-600 px-4 py-2 text-xs font-semibold text-white hover:bg-rose-700 disabled:opacity-50"
                >
                  {deleting ? "Removing..." : "Yes, Disenroll"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </main>
  );
}
