"use client";

import { useState } from "react";
import { createUser, type UserCreateDto, type UserCreateResponseDto, type UserRole } from "@/lib/admin-users";

type CreateUserModalProps = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (createdUser: UserCreateResponseDto) => void;
};

export function CreateUserModal({ isOpen, onClose, onSuccess }: CreateUserModalProps) {
  const [formData, setFormData] = useState<UserCreateDto>({
    fullName: "",
    email: "",
    phoneNumber: "",
    password: "",
    confirmPassword: "",
    role: "Student",
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (error) setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Client-side validation checks matching backend UserCreateDtoValidator
    if (!formData.fullName.trim()) {
      setError("Full name is required.");
      return;
    }
    if (!formData.email.trim() || !formData.email.includes("@")) {
      setError("A valid email address is required.");
      return;
    }
    if (!formData.phoneNumber.trim()) {
      setError("Phone number is required.");
      return;
    }
    if (formData.password.length < 8) {
      setError("Password must be at least 8 characters long.");
      return;
    }
    if (formData.password !== formData.confirmPassword) {
      setError("Confirm password must match the password.");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const created = await createUser(formData);
      onSuccess(created);
      onClose();
      // Reset form
      setFormData({
        fullName: "",
        email: "",
        phoneNumber: "",
        password: "",
        confirmPassword: "",
        role: "Student",
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create user.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-lg overflow-hidden rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] text-teal-700">
              Admin Action
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">Create New User</h2>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-2 text-muted-foreground hover:bg-slate-100 transition"
            aria-label="Close modal"
          >
            ✕
          </button>
        </div>

        {error && (
          <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50/90 p-4 text-xs font-medium text-rose-700">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="mt-5 space-y-4">
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
              Full Name *
            </label>
            <input
              type="text"
              name="fullName"
              required
              value={formData.fullName}
              onChange={handleChange}
              placeholder="e.g. Sarah Jenkins"
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Email Address *
              </label>
              <input
                type="email"
                name="email"
                required
                value={formData.email}
                onChange={handleChange}
                placeholder="sarah@school.edu"
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Phone Number *
              </label>
              <input
                type="text"
                name="phoneNumber"
                required
                value={formData.phoneNumber}
                onChange={handleChange}
                placeholder="+15550192834"
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
              User Role *
            </label>
            <select
              name="role"
              value={formData.role}
              onChange={handleChange}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            >
              <option value="Student">Student</option>
              <option value="Teacher">Teacher</option>
              <option value="Admin">Admin</option>
            </select>
          </div>

          {formData.role === "Student" && (
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Roll Number (Student Only)
              </label>
              <input
                type="text"
                name="rollNo"
                value={formData.rollNo || ""}
                onChange={handleChange}
                placeholder="e.g. STU-1001"
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Password *
              </label>
              <input
                type="password"
                name="password"
                required
                value={formData.password}
                onChange={handleChange}
                placeholder="Min 8 characters"
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Confirm Password *
              </label>
              <input
                type="password"
                name="confirmPassword"
                required
                value={formData.confirmPassword}
                onChange={handleChange}
                placeholder="Re-enter password"
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>
          </div>

          <div className="mt-6 flex items-center justify-end gap-3 pt-4 border-t border-black/5">
            <button
              type="button"
              onClick={onClose}
              className="rounded-full border border-slate-200 bg-white px-5 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 transition"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-full bg-slate-900 px-6 py-2.5 text-sm font-medium text-white shadow-md hover:bg-slate-800 transition disabled:opacity-50"
            >
              {loading ? (
                <>
                  <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                  Creating...
                </>
              ) : (
                "Create User"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
