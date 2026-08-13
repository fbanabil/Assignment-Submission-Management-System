"use client";

import { useEffect, useState } from "react";
import {
  getTeacherDashboard,
  type TeacherAssignedClassSubjectDto,
} from "@/lib/teacher-dashboard";
import {
  createAssignment,
  type AssignmentStatus,
  type TeacherAssignmentCreateDto,
  type TeacherAssignmentItemDto,
} from "@/lib/teacher-assignments";

interface CreateAssignmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (newAssignment: TeacherAssignmentItemDto) => void;
}

export function CreateAssignmentModal({
  isOpen,
  onClose,
  onSuccess,
}: CreateAssignmentModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [selectedPairIndex, setSelectedPairIndex] = useState("0");
  const [dueDate, setDueDate] = useState("");
  const [maxMarks, setMaxMarks] = useState(100);
  const [status, setStatus] = useState<AssignmentStatus>("Active");
  const [allowLateSubmission, setAllowLateSubmission] = useState(true);
  const [allowResubmission, setAllowResubmission] = useState(true);

  const [assignedPairs, setAssignedPairs] = useState<TeacherAssignedClassSubjectDto[]>([]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      // Call teacher dashboard API to fetch real assigned class + subject GUID pairs
      getTeacherDashboard()
        .then((res) => {
          if (res.assignedClasses && res.assignedClasses.length > 0) {
            setAssignedPairs(res.assignedClasses);
            setSelectedPairIndex("0");
          } else {
            setAssignedPairs([]);
          }
        })
        .catch(() => {
          setAssignedPairs([]);
        });
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError("Assignment title is required.");
      return;
    }
    if (!dueDate) {
      setError("Due date & time is required.");
      return;
    }
    if (assignedPairs.length === 0) {
      setError("No assigned classes available. Please ensure classes are assigned to your account.");
      return;
    }

    setLoading(true);
    setError(null);

    const pair = assignedPairs[Number(selectedPairIndex)];
    if (!pair || !pair.classId || !pair.subjectId) {
      setError("Selected class or subject GUID is missing.");
      setLoading(false);
      return;
    }

    const formattedIsoDate = new Date(dueDate).toISOString();

    const dto: TeacherAssignmentCreateDto = {
      title: title.trim(),
      description: description.trim(),
      classId: pair.classId,
      subjectId: pair.subjectId,
      deadline: formattedIsoDate,
      dueDate: formattedIsoDate,
      maxMarks: Number(maxMarks) || 100,
      status: status === "Active" || status === "Published" ? 1 : 0,
      allowLateSubmission,
      allowResubmission,
    };

    try {
      const created = await createAssignment(dto, {
        className: `${pair.className} (${pair.classSection})`,
        subjectName: pair.subjectName,
        subjectCode: pair.subjectCode,
      });
      onSuccess(created);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create assignment.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-xs">
      <div className="w-full max-w-xl max-h-[90vh] overflow-y-auto rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-slate-100">
          <div>
            <h2 className="text-xl font-bold text-slate-900">Create New Assignment</h2>
            <p className="text-xs text-slate-500 mt-0.5">Assign a new task, homework, or quiz to your class</p>
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
              placeholder="e.g. Quadratic Equations Problem Set"
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
              placeholder="Provide assignment guidelines, pages to solve, or criteria..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
            />
          </div>

          <div>
            <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
              Target Assigned Class & Subject *
            </label>
            <select
              value={selectedPairIndex}
              onChange={(e) => setSelectedPairIndex(e.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
            >
              {assignedPairs.length > 0 ? (
                assignedPairs.map((pair, idx) => (
                  <option key={pair.classSubjectId || idx} value={String(idx)}>
                    {pair.className} ({pair.classSection}) — {pair.subjectCode} ({pair.subjectName})
                  </option>
                ))
              ) : (
                <option value="">No assigned classes found</option>
              )}
            </select>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block uppercase tracking-wider text-slate-600 mb-1 font-semibold">
                Due Date & Time *
              </label>
              <input
                type="datetime-local"
                required
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
              Initial Status
            </label>
            <select
              value={status}
              onChange={(e) => setStatus(e.target.value as AssignmentStatus)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-2.5 text-sm font-medium text-slate-900 focus:border-teal-500 focus:outline-none"
            >
              <option value="Active">Active (Published immediately)</option>
              <option value="Draft">Draft (Save for later editing)</option>
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
              {loading ? "Creating..." : "Publish Assignment"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
