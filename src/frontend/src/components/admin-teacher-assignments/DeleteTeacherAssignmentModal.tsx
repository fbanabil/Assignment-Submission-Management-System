"use client";

import { useState } from "react";
import { deleteTeacherAssignment, type TeacherAssignmentResponseDto } from "@/lib/admin-teacher-assignments";

type DeleteTeacherAssignmentModalProps = {
  isOpen: boolean;
  assignmentData: TeacherAssignmentResponseDto | null;
  onClose: () => void;
  onSuccess: (deletedId: string) => void;
};

export function DeleteTeacherAssignmentModal({
  isOpen,
  assignmentData,
  onClose,
  onSuccess,
}: DeleteTeacherAssignmentModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !assignmentData) return null;

  const handleDelete = async () => {
    setLoading(true);
    setError(null);

    try {
      await deleteTeacherAssignment(assignmentData.id);
      onSuccess(assignmentData.id);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove teacher assignment.");
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
              Confirm Removal
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">Remove Teacher Assignment</h2>
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
            Are you sure you want to remove <strong className="text-foreground">{assignmentData.teacherName}</strong> from teaching{" "}
            <strong className="text-foreground">{assignmentData.subjectName}</strong> in{" "}
            <strong className="text-foreground">{assignmentData.className}</strong>?
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
                Removing...
              </>
            ) : (
              "Yes, Remove Assignment"
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
