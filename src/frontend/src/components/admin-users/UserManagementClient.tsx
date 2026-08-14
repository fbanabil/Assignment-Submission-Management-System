"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { CreateUserModal } from "./CreateUserModal";
import { EditUserModal } from "./EditUserModal";
import {
  getUsers,
  type PagedUserResultDto,
  type UserCreateResponseDto,
  type UserFilterDto,
  type UserResponseDto,
  type UserRole,
} from "@/lib/admin-users";
import { logoutUser } from "@/lib/auth";

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function roleBadge(role: UserRole) {
  switch (role) {
    case "Admin":
      return "border-purple-500/15 bg-purple-500/10 text-purple-700";
    case "Teacher":
      return "border-blue-500/15 bg-blue-500/10 text-blue-700";
    case "Student":
    default:
      return "border-teal-500/15 bg-teal-500/10 text-teal-700";
  }
}

function statusBadge(isActive: boolean) {
  return isActive
    ? "border-emerald-500/15 bg-emerald-500/10 text-emerald-700"
    : "border-rose-500/15 bg-rose-500/10 text-rose-700";
}

export function UserManagementClient() {
  // Default filter state with specific fields provided on load
  const [filter, setFilter] = useState<UserFilterDto>({
    name: "",
    email: "",
    phoneNumber: "",
    rollNo: "",
    role: "",
    isActive: "",
    sortBy: "createdat",
    sortOrder: "Asc",
    pageNumber: 1,
    pageSize: 10,
  });

  const [pagedData, setPagedData] = useState<PagedUserResultDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modals state
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [selectedUserToEdit, setSelectedUserToEdit] = useState<UserResponseDto | null>(null);

  // Notification message
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const fetchUsersData = useCallback(async (currentFilter: UserFilterDto) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getUsers(currentFilter);
      setPagedData(data);
    } catch (err) {
      console.error("Failed to load users:", err);
      setError(err instanceof Error ? err.message : "Unable to load users data.");
    } finally {
      setLoading(false);
    }
  }, []);

  // Dynamically refetch whenever filters or pagination parameters change
  useEffect(() => {
    fetchUsersData(filter);
  }, [filter, fetchUsersData]);

  const handleInputChange = (field: keyof UserFilterDto) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilter((prev) => ({ ...prev, [field]: e.target.value, pageNumber: 1 }));
  };

  const handleRoleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFilter((prev) => ({
      ...prev,
      role: e.target.value as UserRole | "",
      pageNumber: 1,
    }));
  };

  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setFilter((prev) => ({
      ...prev,
      isActive: e.target.value as "" | "true" | "false",
      pageNumber: 1,
    }));
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
      email: "",
      phoneNumber: "",
      role: "",
      isActive: "",
      pageNumber: 1,
      pageSize: 10,
    });
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  const handleUserCreated = (newUser: UserCreateResponseDto) => {
    setToastMessage(`User "${newUser.fullName}" created successfully as ${newUser.role}!`);
    setTimeout(() => setToastMessage(null), 5000);

    // 1. Instantly update table view with newly created user
    const createdRecord: UserResponseDto = {
      id: newUser.id,
      fullName: newUser.fullName,
      email: newUser.email,
      phoneNumber: "",
      role: newUser.role,
      isActive: true,
      createdAt: new Date().toISOString(),
    };

    setPagedData((prev) => {
      if (!prev) return prev;
      const filteredItems = prev.items.filter((u) => u.id !== createdRecord.id);
      return {
        ...prev,
        items: [createdRecord, ...filteredItems],
        totalCount: prev.totalCount + 1,
      };
    });

    // 2. Reset filter to Page 1 and refetch from backend to ensure complete synchronization
    const resetFilterState: UserFilterDto = {
      name: "",
      email: "",
      phoneNumber: "",
      role: "",
      isActive: "",
      pageNumber: 1,
      pageSize: filter.pageSize,
    };
    setFilter(resetFilterState);
    fetchUsersData(resetFilterState);
  };

  const handleUserUpdated = (updatedUser: UserResponseDto) => {
    setToastMessage(`User "${updatedUser.fullName}" updated successfully.`);
    setTimeout(() => setToastMessage(null), 5000);

    // Instant optimistic update in table state
    setPagedData((prev) => {
      if (!prev) return prev;
      return {
        ...prev,
        items: prev.items.map((u) => (u.id === updatedUser.id ? updatedUser : u)),
      };
    });

    fetchUsersData(filter);
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
              <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/20 bg-teal-500/10 px-3.5 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-teal-700">
                <span className="h-2 w-2 rounded-full bg-teal-500 animate-pulse"></span>
                User Management
              </div>
              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                  User Directory & Access Controls
                </h1>
                <p className="mt-2 text-sm leading-7 text-(--color-muted) sm:text-base">
                  Manage platform users, assign system roles (Admin, Teacher, Student), toggle account active status, and onboard accounts.
                </p>
              </div>
            </div>

            {/* Refreshed info */}
            <div className="flex flex-wrap items-center gap-3 text-xs font-medium text-slate-500 shrink-0">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-black/5 bg-white px-3.5 py-1.5 font-medium text-foreground shadow-2xs">
                Refreshed: <strong className="text-foreground">{pagedData?.fetchedAt ? formatDateTime(pagedData.fetchedAt) : "Just now"}</strong>
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
              className="rounded-full bg-slate-900 px-4 py-2 text-white shadow-md transition hover:bg-slate-800 whitespace-nowrap"
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
              onClick={() => setIsCreateModalOpen(true)}
              className="inline-flex items-center gap-1.5 rounded-full bg-teal-600 px-4 py-2 text-xs font-semibold text-white shadow-md transition hover:bg-teal-700 active:scale-98 cursor-pointer whitespace-nowrap"
            >
              + Create User
            </button>
            <button
              onClick={() => logoutUser()}
              className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-rose-700 font-semibold transition hover:bg-rose-600 hover:text-white cursor-pointer whitespace-nowrap"
            >
              Logout 🚪
            </button>
          </nav>
        </header>

        {/* Dynamic Filter Bar with Separate Fields for Name, Email, PhoneNumber */}
        <section className="rounded-3xl border border-white/70 bg-(--color-surface) p-5 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-slate-700">Filter Users</h2>
              <button
                onClick={handleResetFilters}
                className="rounded-full border border-slate-200 bg-white/90 px-4 py-1.5 text-xs font-semibold uppercase tracking-wider text-slate-600 hover:bg-slate-100 transition shadow-2xs"
              >
                Reset All Filters
              </button>
            </div>

            {/* Field Filters Grid (Name, Email, Phone, Roll No) */}
            <div className="grid gap-3 sm:grid-cols-4">
              {/* Name filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Name</label>
                <input
                  type="text"
                  placeholder="Filter by Name..."
                  value={filter.name || ""}
                  onChange={handleInputChange("name")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Email filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Email</label>
                <input
                  type="text"
                  placeholder="Filter by Email..."
                  value={filter.email || ""}
                  onChange={handleInputChange("email")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Phone number filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Phone Number</label>
                <input
                  type="text"
                  placeholder="Filter by Phone..."
                  value={filter.phoneNumber || ""}
                  onChange={handleInputChange("phoneNumber")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>

              {/* Roll number filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Roll No</label>
                <input
                  type="text"
                  placeholder="Filter by Roll No..."
                  value={filter.rollNo || ""}
                  onChange={handleInputChange("rollNo")}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground placeholder:text-slate-400 focus:border-teal-500 focus:outline-none transition shadow-2xs"
                />
              </div>
            </div>

            {/* Dropdowns Grid */}
            <div className="grid gap-3 sm:grid-cols-4 pt-2 border-t border-black/5">
              {/* Role filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Role</label>
                <select
                  value={filter.role || ""}
                  onChange={handleRoleChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
                >
                  <option value="">All Roles</option>
                  <option value="Admin">Admin</option>
                  <option value="Teacher">Teacher</option>
                  <option value="Student">Student</option>
                </select>
              </div>

              {/* Status filter */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Status</label>
                <select
                  value={filter.isActive === undefined ? "" : String(filter.isActive)}
                  onChange={handleStatusChange}
                  className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3.5 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
                >
                  <option value="">All Statuses</option>
                  <option value="true">Active Only</option>
                  <option value="false">Deactive Only</option>
                </select>
              </div>

              {/* Sort By selector */}
              <div>
                <label className="block text-xs font-medium text-slate-500 mb-1">Sort By</label>
                <div className="flex items-center gap-1.5">
                  <select
                    value={filter.sortBy || "createdat"}
                    onChange={(e) => {
                      const newFilter = { ...filter, sortBy: e.target.value, pageNumber: 1 };
                      setFilter(newFilter);
                      fetchUsersData(newFilter);
                    }}
                    className="w-full rounded-2xl border border-slate-200 bg-white/80 px-3 py-2 text-sm font-medium text-foreground focus:border-teal-500 focus:outline-none transition shadow-2xs"
                  >
                    <option value="createdat">Created Date</option>
                    <option value="name">Name</option>
                    <option value="email">Email</option>
                    <option value="rollno">Roll No</option>
                    <option value="role">Role</option>
                    <option value="isactive">Status</option>
                  </select>
                  <button
                    type="button"
                    onClick={() => {
                      const newOrder: "Asc" | "Desc" = filter.sortOrder === "Asc" ? "Desc" : "Asc";
                      const newFilter: UserFilterDto = { ...filter, sortOrder: newOrder, pageNumber: 1 };
                      setFilter(newFilter);
                      fetchUsersData(newFilter);
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
          <div className="rounded-3xl border border-rose-200 bg-rose-50/90 p-4 shadow-sm text-rose-700">
            <p className="text-sm font-semibold">{error}</p>
          </div>
        )}

        {/* User Table Section */}
        <section className="rounded-4xl border border-white/70 bg-(--color-surface) p-6 shadow-[0_16px_50px_rgba(15,23,42,0.08)] backdrop-blur">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-black/5">
            <div>
              <h2 className="text-xl font-semibold tracking-tight text-foreground">
                All Users ({pagedData?.totalCount ?? 0})
              </h2>
              <p className="mt-0.5 text-xs text-(--color-muted)">
                Filtered by specific fields. Click &quot;Edit User&quot; to edit details or toggle active status.
              </p>
            </div>
          </div>

          <div className="mt-4 overflow-hidden rounded-3xl border border-black/5 bg-white/80 shadow-xs">
            <div className="max-h-[38rem] overflow-auto">
              <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                <thead className="sticky top-0 bg-white/95 backdrop-blur z-10">
                  <tr className="text-xs uppercase tracking-[0.18em] text-(--color-muted)">
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">User</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Contact Info</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Roll No</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Role</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Status</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold">Joined Date</th>
                    <th className="border-b border-black/5 px-4 py-3.5 font-semibold text-right">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    Array.from({ length: filter.pageSize }).map((_, idx) => (
                      <tr key={`skel-${idx}`} className="animate-pulse">
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-32 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-40 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-16 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-16 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <div className="h-4 w-24 rounded-full bg-slate-200"></div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <div className="ml-auto h-7 w-20 rounded-full bg-slate-200"></div>
                        </td>
                      </tr>
                    ))
                  ) : pagedData && pagedData.items.length > 0 ? (
                    pagedData.items.map((user) => (
                      <tr
                        key={user.id}
                        className="odd:bg-white even:bg-slate-50/70 hover:bg-slate-100/50 transition-colors"
                      >
                        <td className="border-b border-black/5 px-4 py-4 font-semibold text-foreground">
                          <div className="flex items-center gap-3">
                            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-slate-200 text-xs font-bold text-slate-700">
                              {user.fullName.slice(0, 2).toUpperCase()}
                            </div>
                            <div>
                              <p className="font-medium text-foreground leading-tight">{user.fullName}</p>
                            </div>
                          </div>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <p className="text-sm font-medium text-foreground">{user.email}</p>
                          <p className="text-xs text-slate-500">{user.phoneNumber || "—"}</p>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          {user.rollNo ? (
                            <span className="inline-flex rounded-full border border-purple-500/20 bg-purple-500/10 px-2.5 py-0.5 text-xs font-mono font-bold text-purple-700">
                              {user.rollNo}
                            </span>
                          ) : (
                            <span className="text-xs text-slate-400">—</span>
                          )}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <span
                            className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] ${roleBadge(
                              user.role
                            )}`}
                          >
                            {user.role}
                          </span>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4">
                          <span
                            className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] ${statusBadge(
                              user.isActive
                            )}`}
                          >
                            {user.isActive ? "Active" : "Deactive"}
                          </span>
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-slate-500 text-xs">
                          {formatDateTime(user.createdAt)}
                        </td>
                        <td className="border-b border-black/5 px-4 py-4 text-right">
                          <button
                            type="button"
                            onClick={() => setSelectedUserToEdit(user)}
                            className="inline-flex items-center gap-1.5 rounded-full border border-slate-300 bg-white px-4 py-1.5 text-xs font-medium text-slate-800 shadow-2xs hover:bg-slate-900 hover:text-white transition cursor-pointer"
                          >
                            Edit User
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={6} className="px-4 py-12 text-center text-slate-500">
                        <div className="mx-auto max-w-sm space-y-2">
                          <p className="text-base font-semibold text-slate-700">No users match your filter criteria</p>
                          <p className="text-xs text-slate-500">
                            Try adjusting your Name, Email, Phone, or Role filter settings.
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

      {/* Create User Modal */}
      <CreateUserModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSuccess={handleUserCreated}
      />

      {/* Edit User Modal */}
      <EditUserModal
        isOpen={selectedUserToEdit !== null}
        user={selectedUserToEdit}
        onClose={() => setSelectedUserToEdit(null)}
        onSuccess={handleUserUpdated}
      />
    </main>
  );
}
