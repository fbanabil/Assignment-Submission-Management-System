"use client";

import { useEffect, useState } from "react";
import {
  updateAssignment,
  type AssignmentStatus,
  type TeacherAssignmentItemDto,
} from "@/lib/teacher-assignments";

interface EditAssignmentModalProps {
  isOpen: boolean;
  assignment: TeacherAssignmentItemDto | null;
  onClose: () => void;
  onSuccess: (updated: TeacherAssignmentItemDto) => void;
}

export function EditAssignmentModal({
  isOpen,
  assignment,
  onClose,
  onSuccess,
}: EditAssignmentModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [maxMarks, setMaxMarks] = useState(100);
  const [status, setStatus] = useState<AssignmentStatus>("Active");
  const [allowLateSubmission, setAllowLateSubmission] = useState(true);
  const [allowResubmission, setAllowResubmission] = useState(true);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (assignment) {
      setTitle(assignment.title || "");
      setDescription(assignment.description || "");
      if (assignment.dueDate) {
        const d = new Date(assignment.dueDate);
        if (!Number.isNaN(d.getTime())) {
          const isoStr = d.toISOString().slice(0, 16);
          setDueDate(isoStr);
        }
      }
      setMaxMarks(assignment.maxMarks || 100);
      setStatus(assignment.status || "Active");
      setAllowLateSubmission(typeof assignment.allowLateSubmission === "boolean" ? assignment.allowLateSubmission : true);
      setAllowResubmission(typeof assignment.allowResubmission === "boolean" ? assignment.allowResubmission : true);
    }
  }, [assignment]);

  if (!isOpen || !assignment) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError("Title cannot be empty.");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const updated = await updateAssignment({
        id: assignment.id,
        title: title.trim(),
        description: description.trim(),
        dueDate: dueDate ? new Date(dueDate).toISOString() : assignment.dueDate,
        maxMarks: Number(maxMarks) || 100,
        status,
        allowLateSubmission,
        allowResubmission,
      });

      onSuccess(updated);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update assignment.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-xs">
      <div className="w-full max-w-xl max-h-[90vh] overflow-y-auto rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-slate-100">
          <div>
            <h2 className="text-xl font-bold text-slate-900">Edit Assignment</h2>
            <p className="text-xs text-slate-500 mt-0.5">Modify deadline, marks, or instructions for this assignment</p>
          </div>
          <button
            onClick={onClose}
            className="rounded-full p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700 transition"
          >
            ✕
          </button>
        </div>

        {error && (
          <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 p-3.5 text-xs font-semibold text-rose-700">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="mt-5 space-y-4 text-xs font-medium">
          <div>
            <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
              Title *
            </label>
            <input
              type="text"
              required
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
            />
          </div>

          <div>
            <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
              Description & Instructions
            </label>
            <textarea
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
                Due Date & Time
              </label>
              <input
                type="datetime-local"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
                className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
                Maximum Marks
              </label>
              <input
                type="number"
                min={1}
                max={1000}
                value={maxMarks}
                onChange={(e) => setMaxMarks(Number(e.target.value))}
                className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
              />
            </div>
          </div>

          <div>
            <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
              Status
            </label>
            <select
              value={status}
              onChange={(e) => setStatus(e.target.value as AssignmentStatus)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
            >
              <option value="Active">Active (Published)</option>
              <option value="Draft">Draft (Hidden from students)</option>
              <option value="Past Due">Past Due (Closed)</option>
              <option value="Archived">Archived</option>
            </select>
          </div>

          <div className="flex flex-col gap-2 pt-2">
            <label className="inline-flex items-center gap-2 cursor-pointer text-slate-700">
              <input
                type="checkbox"
                checked={allowLateSubmission}
                onChange={(e) => setAllowLateSubmission(e.target.checked)}
                className="h-4 w-4 rounded border-slate-300 text-teal-600 focus:ring-teal-500"
              />
              <span>Allow Late Submissions</span>
            </label>

            <label className="inline-flex items-center gap-2 cursor-pointer text-slate-700">
              <input
                type="checkbox"
                checked={allowResubmission}
                onChange={(e) => setAllowResubmission(e.target.checked)}
                className="h-4 w-4 rounded border-slate-300 text-teal-600 focus:ring-teal-500"
              />
              <span>Allow Student Resubmissions</span>
            </label>
          </div>

          <div className="mt-6 flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="rounded-full border border-slate-200 px-5 py-2.5 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="rounded-full bg-slate-900 px-6 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-slate-800 disabled:opacity-50 transition"
            >
              {loading ? "Saving..." : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
