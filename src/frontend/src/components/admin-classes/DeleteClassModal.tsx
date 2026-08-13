"use client";

import { useState } from "react";
import { deleteClass, type ClassResponseDto } from "@/lib/admin-classes";

type DeleteClassModalProps = {
  isOpen: boolean;
  classData: ClassResponseDto | null;
  onClose: () => void;
  onSuccess: (deletedId: string) => void;
};

export function DeleteClassModal({ isOpen, classData, onClose, onSuccess }: DeleteClassModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !classData) return null;

  const handleDelete = async () => {
    setLoading(true);
    setError(null);

    try {
      await deleteClass(classData.id);
      onSuccess(classData.id);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete class.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-md overflow-hidden rounded-3xl border border-rose-200 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-rose-500/15 bg-rose-500/10 px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] text-rose-700">
              Confirm Delete
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">Delete Class Section</h2>
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

        <div className="mt-5 space-y-3">
          <p className="text-sm leading-6 text-slate-600">
            Are you sure you want to delete <strong className="text-foreground">{classData.name}</strong> ({classData.section}, {classData.academicYear})?
          </p>
          <p className="text-xs leading-5 text-rose-600 font-medium bg-rose-50 p-3 rounded-2xl border border-rose-100">
            Warning: This action will permanently remove this class section and its associations.
          </p>
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
            type="button"
            onClick={handleDelete}
            disabled={loading}
            className="inline-flex items-center gap-2 rounded-full bg-rose-600 px-6 py-2.5 text-sm font-medium text-white shadow-md hover:bg-rose-700 transition disabled:opacity-50 cursor-pointer"
          >
            {loading ? (
              <>
                <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                Deleting...
              </>
            ) : (
              "Yes, Delete Class"
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
