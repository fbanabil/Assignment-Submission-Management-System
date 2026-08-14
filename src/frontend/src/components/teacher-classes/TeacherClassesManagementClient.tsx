"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getTeacherClasses,
  type TeacherClassFilterDto,
} from "@/lib/teacher-classes";
import { type TeacherAssignedClassSubjectDto } from "@/lib/teacher-dashboard";
import { logoutUser } from "@/lib/auth";
import { formatDisplayError } from "@/lib/api-error";

export function TeacherClassesManagementClient() {
  const [items, setItems] = useState<TeacherAssignedClassSubjectDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  // Filters
  const [classNameFilter, setClassNameFilter] = useState("");
  const [sectionFilter, setSectionFilter] = useState("");
  const [academicYearFilter, setAcademicYearFilter] = useState("");
  const [subjectCodeFilter, setSubjectCodeFilter] = useState("");
  const [sortBy, setSortBy] = useState("classname");
  const [sortOrder, setSortOrder] = useState<"Asc" | "Desc">("Asc");

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchClasses = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getTeacherClasses({
        className: classNameFilter.trim() || undefined,
        classSection: sectionFilter.trim() || undefined,
        academicYear: academicYearFilter.trim() || undefined,
        subjectCode: subjectCodeFilter.trim() || undefined,
        sortBy,
        sortOrder,
        pageNumber,
        pageSize,
      });
      setItems(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages || 1);
    } catch (err) {
      console.error("Failed to load teacher assigned classes:", err);
      setError(formatDisplayError(err, "Failed to load assigned classes."));
    } finally {
      setLoading(false);
    }
  }, [classNameFilter, sectionFilter, academicYearFilter, subjectCodeFilter, sortBy, sortOrder, pageNumber, pageSize]);

  useEffect(() => {
    fetchClasses();
  }, [fetchClasses]);

  const handleFilterReset = () => {
    setClassNameFilter("");
    setSectionFilter("");
    setAcademicYearFilter("");
    setSubjectCodeFilter("");
    setSortBy("classname");
    setSortOrder("Asc");
    setPageNumber(1);
  };

  const totalEnrolledStudents = items.reduce((sum, item) => sum + (item.studentCount || 0), 0);

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
                  My Assigned Classes & Subjects
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-7 text-(--color-muted) sm:text-base">
                  View your course loads, assigned class sections, and student enrollment metrics.
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
                className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
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
                onClick={() => logoutUser()}
                className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
              >
                Logout 🚪
              </button>
            </nav>
          </div>
        </header>

        {/* Overview Stats */}
        <section className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div className="rounded-3xl border border-white/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
              Assigned Classes
            </span>
            <span className="text-3xl font-black text-slate-900 block mt-1">{totalCount}</span>
          </div>

          <div className="rounded-3xl border border-white/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
              Total Enrolled Students
            </span>
            <span className="text-3xl font-black text-teal-700 block mt-1">{totalEnrolledStudents}</span>
          </div>

          <div className="rounded-3xl border border-white/70 bg-white/80 p-6 shadow-sm backdrop-blur">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
              Active Academic Years
            </span>
            <span className="text-3xl font-black text-purple-700 block mt-1">
              {Array.from(new Set(items.map((i) => i.academicYear))).length || 1}
            </span>
          </div>
        </section>

        {/* Filter Controls Section */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.06)] backdrop-blur">
          <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-4">
            Search & Filter Classes
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-5 gap-4">
            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Class Name
              </label>
              <input
                type="text"
                placeholder="Search class name..."
                value={classNameFilter}
                onChange={(e) => {
                  setClassNameFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Section
              </label>
              <input
                type="text"
                placeholder="e.g. Section A"
                value={sectionFilter}
                onChange={(e) => {
                  setSectionFilter(e.target.value);
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
                value={subjectCodeFilter}
                onChange={(e) => {
                  setSubjectCodeFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-600 mb-1">
                Academic Year
              </label>
              <input
                type="text"
                placeholder="e.g. 2024-2025"
                value={academicYearFilter}
                onChange={(e) => {
                  setAcademicYearFilter(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
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
                  <option value="classname">Class Name</option>
                  <option value="classsection">Section</option>
                  <option value="subjectcode">Subject Code</option>
                  <option value="subjectname">Subject Name</option>
                  <option value="academicyear">Academic Year</option>
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
              Showing <strong>{items.length}</strong> of <strong>{totalCount}</strong> assigned classes
            </span>
            <button
              onClick={handleFilterReset}
              className="text-teal-700 hover:text-teal-900 font-semibold cursor-pointer"
            >
              Reset Filters ↺
            </button>
          </div>
        </section>

        {/* Error Message */}
        {error && (
          <div className="rounded-3xl border border-rose-200 bg-rose-50 p-4 text-rose-700 text-sm font-semibold">
            {error}
          </div>
        )}

        {/* Classes Table */}
        <section className="overflow-hidden rounded-4xl border border-white/70 bg-(--color-surface) shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50/80 text-xs font-semibold uppercase tracking-wider text-slate-500 border-b border-black/5">
                <tr>
                  <th className="px-6 py-4">Class & Section</th>
                  <th className="px-6 py-4">Subject Name & Code</th>
                  <th className="px-6 py-4">Academic Year</th>
                  <th className="px-6 py-4">Students Enrolled</th>
                  <th className="px-6 py-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-black/5">
                {loading ? (
                  Array.from({ length: 3 }).map((_, idx) => (
                    <tr key={`skel-${idx}`} className="animate-pulse">
                      <td className="px-6 py-4"><div className="h-4 w-32 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-40 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-24 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4"><div className="h-4 w-16 bg-slate-200 rounded-full"></div></td>
                      <td className="px-6 py-4 text-right"><div className="h-4 w-20 bg-slate-200 rounded-full ml-auto"></div></td>
                    </tr>
                  ))
                ) : items.length > 0 ? (
                  items.map((item) => (
                    <tr key={item.classSubjectId} className="hover:bg-slate-50/60 transition">
                      <td className="px-6 py-4">
                        <div className="font-semibold text-slate-900">{item.className}</div>
                        <span className="inline-flex rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-600 mt-1">
                          {item.classSection}
                        </span>
                      </td>

                      <td className="px-6 py-4">
                        <div className="font-medium text-slate-900">{item.subjectName}</div>
                        <div className="inline-flex rounded-full bg-purple-100 px-2 py-0.5 text-[11px] font-mono font-semibold text-purple-800 mt-1">
                          {item.subjectCode}
                        </div>
                      </td>

                      <td className="px-6 py-4 text-xs font-semibold text-slate-600">
                        {item.academicYear}
                      </td>

                      <td className="px-6 py-4">
                        <span className="inline-flex items-center gap-1 rounded-full bg-teal-50 border border-teal-200 px-3 py-1 text-xs font-bold text-teal-800">
                          👤 {item.studentCount} Students
                        </span>
                      </td>

                      <td className="px-6 py-4 text-right space-x-2">
                        <Link
                          href={`/teacher/assignments?className=${encodeURIComponent(item.className)}`}
                          className="inline-block rounded-full border border-teal-600 bg-teal-50 px-3.5 py-1.5 text-xs font-semibold text-teal-700 hover:bg-teal-600 hover:text-white transition"
                        >
                          Assignments 📚
                        </Link>
                        <Link
                          href={`/teacher/submissions?className=${encodeURIComponent(item.className)}`}
                          className="inline-block rounded-full border border-slate-300 bg-white px-3.5 py-1.5 text-xs font-semibold text-slate-700 hover:bg-slate-900 hover:text-white transition"
                        >
                          Submissions 📥
                        </Link>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-slate-500">
                      No assigned classes found matching your filters.
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
      </div>
    </main>
  );
}
