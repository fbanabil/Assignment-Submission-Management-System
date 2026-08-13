"use client";

import { useState } from "react";
import { deleteAssignment, type TeacherAssignmentItemDto } from "@/lib/teacher-assignments";

interface DeleteAssignmentModalProps {
  isOpen: boolean;
  assignment: TeacherAssignmentItemDto | null;
  onClose: () => void;
  onSuccess: (deletedId: string) => void;
}

export function DeleteAssignmentModal({
  isOpen,
  assignment,
  onClose,
  onSuccess,
}: DeleteAssignmentModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !assignment) return null;

  const handleDelete = async () => {
    setLoading(true);
    setError(null);
    try {
      await deleteAssignment(assignment.id);
      onSuccess(assignment.id);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete assignment.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-xs">
      <div className="w-full max-w-md rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="text-center space-y-3">
          <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-rose-100 text-rose-600 text-xl font-bold">
            🗑️
          </div>
          <h2 className="text-xl font-bold text-slate-900">Delete Assignment</h2>
          <p className="text-xs text-slate-500">
            Are you sure you want to delete <strong className="text-slate-900">&quot;{assignment.title}&quot;</strong>? This action cannot be undone and will remove all student submission records.
          </p>
        </div>

        {error && (
          <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 p-3.5 text-xs font-semibold text-rose-700">
            {error}
          </div>
        )}

        <div className="mt-6 flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-slate-200 px-5 py-2.5 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition"
          >
            Cancel
          </button>
          <button
            type="button"
            disabled={loading}
            onClick={handleDelete}
            className="rounded-full bg-rose-600 px-6 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-rose-700 disabled:opacity-50 transition"
          >
            {loading ? "Deleting..." : "Confirm Delete"}
          </button>
        </div>
      </div>
    </div>
  );
}
