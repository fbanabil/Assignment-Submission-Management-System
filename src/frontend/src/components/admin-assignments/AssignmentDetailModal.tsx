"use client";

import type { AssignmentResponseDto } from "@/lib/admin-assignments";

type AssignmentDetailModalProps = {
  isOpen: boolean;
  assignment: AssignmentResponseDto | null;
  onClose: () => void;
};

function formatDateTime(value?: string) {
  if (!value) return "N/A";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function statusBadge(status: string) {
  switch (status) {
    case "Active":
      return "border-emerald-500/15 bg-emerald-500/10 text-emerald-700";
    case "Past Due":
      return "border-rose-500/15 bg-rose-500/10 text-rose-700";
    case "Draft":
      return "border-amber-500/15 bg-amber-500/10 text-amber-700";
    case "Published":
    default:
      return "border-blue-500/15 bg-blue-500/10 text-blue-700";
  }
}

export function AssignmentDetailModal({ isOpen, assignment, onClose }: AssignmentDetailModalProps) {
  if (!isOpen || !assignment) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-2xl overflow-hidden rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span
              className={`inline-flex items-center gap-2 rounded-full border px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] ${statusBadge(
                assignment.status
              )}`}
            >
              {assignment.status}
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">{assignment.title}</h2>
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

        <div className="mt-5 space-y-6">
          {/* Metadata Grid */}
          <div className="grid gap-4 sm:grid-cols-2 bg-slate-50/80 p-4 rounded-2xl border border-slate-100 text-sm">
            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Class & Section</p>
              <p className="font-semibold text-slate-800 mt-0.5">{assignment.className}</p>
              <p className="text-xs text-slate-500">
                {assignment.classSection} ({assignment.academicYear})
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Subject</p>
              <p className="font-semibold text-slate-800 mt-0.5">{assignment.subjectName}</p>
              <span className="inline-flex rounded-full bg-purple-100 px-2.5 py-0.5 text-xs font-mono font-semibold text-purple-700">
                {assignment.subjectCode}
              </span>
            </div>

            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Assigned By Teacher</p>
              <p className="font-semibold text-slate-800 mt-0.5">{assignment.teacherName}</p>
              <p className="text-xs text-slate-500">{assignment.teacherEmail}</p>
            </div>

            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Submissions & Marks</p>
              <p className="font-semibold text-slate-800 mt-0.5">
                {assignment.totalSubmissions} Submissions
              </p>
              <p className="text-xs text-slate-500">Max Marks: {assignment.maxMarks} pts</p>
            </div>
          </div>

          {/* Description */}
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
              Assignment Instructions / Description
            </h3>
            <div className="rounded-2xl border border-slate-200 bg-white p-4 text-sm leading-relaxed text-slate-700">
              {assignment.description || "No additional description provided."}
            </div>
          </div>

          {/* Dates & Policies */}
          <div className="grid gap-3 sm:grid-cols-2 pt-2 border-t border-black/5 text-xs">
            <div>
              <span className="text-slate-400 font-medium">Due Date: </span>
              <strong className="text-rose-600 font-semibold">{formatDateTime(assignment.dueDate)}</strong>
            </div>
            <div>
              <span className="text-slate-400 font-medium">Created On: </span>
              <strong className="text-slate-700 font-medium">{formatDateTime(assignment.createdAt)}</strong>
            </div>
            <div>
              <span className="text-slate-400 font-medium">Late Submissions: </span>
              <strong className={assignment.allowLateSubmission ? "text-emerald-600" : "text-slate-500"}>
                {assignment.allowLateSubmission ? "Allowed" : "Not Allowed"}
              </strong>
            </div>
          </div>
        </div>

        <div className="mt-6 flex items-center justify-end pt-4 border-t border-black/5">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full bg-slate-900 px-6 py-2.5 text-sm font-medium text-white shadow-md hover:bg-slate-800 transition"
          >
            Close Inspector
          </button>
        </div>
      </div>
    </div>
  );
}
