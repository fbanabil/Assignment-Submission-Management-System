"use client";

import { useState } from "react";
import { createClass, type ClassCreateDto, type ClassResponseDto } from "@/lib/admin-classes";

type CreateClassModalProps = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (createdClass: ClassResponseDto) => void;
};

export function CreateClassModal({ isOpen, onClose, onSuccess }: CreateClassModalProps) {
  const [formData, setFormData] = useState<ClassCreateDto>({
    name: "",
    section: "",
    academicYear: `${new Date().getFullYear()}-${new Date().getFullYear() + 1}`,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (error) setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name.trim()) {
      setError("Class name is required.");
      return;
    }
    if (!formData.section.trim()) {
      setError("Section is required.");
      return;
    }
    if (!formData.academicYear.trim()) {
      setError("Academic year is required.");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const created = await createClass(formData);
      onSuccess(created);
      onClose();
      setFormData({
        name: "",
        section: "",
        academicYear: `${new Date().getFullYear()}-${new Date().getFullYear() + 1}`,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create class.");
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
              Class Management
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">Create Class & Section</h2>
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
              Class Name *
            </label>
            <input
              type="text"
              name="name"
              required
              value={formData.name}
              onChange={handleChange}
              placeholder="e.g. Grade 10 - Mathematics"
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Section *
              </label>
              <input
                type="text"
                name="section"
                required
                value={formData.section}
                onChange={handleChange}
                placeholder="e.g. Section A"
                className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                Academic Year *
              </label>
              <input
                type="text"
                name="academicYear"
                required
                value={formData.academicYear}
                onChange={handleChange}
                placeholder="e.g. 2024-2025"
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
                "Create Class"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
