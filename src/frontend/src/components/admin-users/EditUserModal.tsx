"use client";

import { useEffect, useState } from "react";
import { updateUser, type UserResponseDto, type UserUpdateDto, type UserRole } from "@/lib/admin-users";

type EditUserModalProps = {
  isOpen: boolean;
  user: UserResponseDto | null;
  onClose: () => void;
  onSuccess: (updatedUser: UserResponseDto) => void;
};

export function EditUserModal({ isOpen, user, onClose, onSuccess }: EditUserModalProps) {
  const [formData, setFormData] = useState<UserUpdateDto>({
    id: "",
    fullName: "",
    email: "",
    phoneNumber: "",
    role: "Student",
    isActive: true,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (user) {
      setFormData({
        id: user.id,
        fullName: user.fullName,
        email: user.email,
        phoneNumber: user.phoneNumber,
        rollNo: user.rollNo || "",
        role: user.role,
        isActive: user.isActive,
      });
      setError(null);
    }
  }, [user]);

  if (!isOpen || !user) return null;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    if (type === "checkbox") {
      const checked = (e.target as HTMLInputElement).checked;
      setFormData((prev) => ({ ...prev, [name]: checked }));
    } else {
      setFormData((prev) => ({ ...prev, [name]: value }));
    }
    if (error) setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const updated = await updateUser(formData);
      onSuccess(updated);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update user.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-lg overflow-hidden rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-blue-500/15 bg-blue-500/10 px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] text-blue-700">
              User Management
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">Edit & Account Status</h2>
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
              Full Name
            </label>
            <input
              type="text"
              name="fullName"
              required
              value={formData.fullName || ""}
              onChange={handleChange}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Email Address
              </label>
              <input
                type="email"
                name="email"
                required
                value={formData.email || ""}
                onChange={handleChange}
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Phone Number
              </label>
              <input
                type="text"
                name="phoneNumber"
                required
                value={formData.phoneNumber || ""}
                onChange={handleChange}
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
              User Role
            </label>
            <select
              name="role"
              value={formData.role || "Student"}
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

          {/* Account Activation Status Toggle */}
          <div className="rounded-2xl border border-slate-200 bg-slate-50/70 p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-semibold text-slate-900">Account Status</p>
                <p className="text-xs text-slate-500 mt-0.5">
                  Toggle whether this user can log into the platform.
                </p>
              </div>

              <label className="relative inline-flex items-center cursor-pointer">
                <input
                  type="checkbox"
                  name="isActive"
                  checked={formData.isActive ?? true}
                  onChange={handleChange}
                  className="sr-only peer"
                />
                <div className="w-11 h-6 bg-slate-300 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-emerald-600"></div>
              </label>
            </div>
            <div className="mt-2 text-xs font-medium">
              Status:{" "}
              <span className={formData.isActive ? "text-emerald-700 font-semibold" : "text-rose-600 font-semibold"}>
                {formData.isActive ? "Active (Enabled)" : "Deactive (Disabled)"}
              </span>
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
                  Saving...
                </>
              ) : (
                "Save Changes"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
