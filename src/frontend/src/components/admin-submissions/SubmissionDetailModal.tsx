"use client";

import { resolveSubmissionFileUrl, type SubmissionResponseDto } from "@/lib/admin-submissions";

type SubmissionDetailModalProps = {
  isOpen: boolean;
  submission: SubmissionResponseDto | null;
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
    case "Graded":
      return "border-emerald-500/15 bg-emerald-500/10 text-emerald-700";
    case "Late":
      return "border-rose-500/15 bg-rose-500/10 text-rose-700";
    case "Submitted":
    default:
      return "border-blue-500/15 bg-blue-500/10 text-blue-700";
  }
}

function getFileExtension(fileUrl: string): string {
  const parts = fileUrl.split(".");
  if (parts.length > 1) {
    return parts[parts.length - 1].toLowerCase();
  }
  return "";
}

export function SubmissionDetailModal({ isOpen, submission, onClose }: SubmissionDetailModalProps) {
  if (!isOpen || !submission) return null;

  const fullFileUrl = resolveSubmissionFileUrl(submission.fileUrl);
  const ext = getFileExtension(submission.fileUrl);
  const isDirectlyViewableInBrowser = ["pdf", "png", "jpg", "jpeg", "webp", "gif", "txt"].includes(ext);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-2xl overflow-hidden rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span
              className={`inline-flex items-center gap-2 rounded-full border px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] ${statusBadge(
                submission.status
              )}`}
            >
              {submission.status}
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">
              Submission: {submission.assignmentTitle}
            </h2>
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
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Student Info</p>
              <p className="font-semibold text-slate-800 mt-0.5">{submission.studentName}</p>
              <p className="text-xs text-slate-500">{submission.studentEmail}</p>
            </div>

            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Class & Subject</p>
              <p className="font-semibold text-slate-800 mt-0.5">{submission.className}</p>
              <p className="text-xs text-slate-500">
                {submission.subjectName} ({submission.subjectCode})
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Submitted On</p>
              <p className="font-semibold text-slate-800 mt-0.5">{formatDateTime(submission.submittedAt)}</p>
            </div>

            <div>
              <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Grade / Score</p>
              <p className="font-semibold text-slate-800 mt-0.5">
                {submission.grade !== undefined && submission.grade !== null
                  ? `${submission.grade} / ${submission.maxMarks} pts`
                  : "Not Graded Yet"}
              </p>
            </div>
          </div>

          {/* Attached File Section */}
          <div className="rounded-2xl border border-teal-500/20 bg-teal-50/50 p-4">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-wider text-teal-800">
                  Attached Submission File
                </p>
                <p className="text-sm font-medium text-slate-700 mt-0.5 truncate max-w-sm">
                  {submission.fileUrl || "No file attached"}
                </p>
                <p className="text-xs text-slate-500">
                  {isDirectlyViewableInBrowser
                    ? "Clicking below opens file in a new tab browser preview."
                    : "Clicking below downloads the file to your computer."}
                </p>
              </div>

              {submission.fileUrl && (
                <a
                  href={fullFileUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-2 rounded-full bg-teal-600 px-5 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-teal-700 transition"
                >
                  {isDirectlyViewableInBrowser ? "👁 Open / View File" : "⬇ Download File"}
                </a>
              )}
            </div>
          </div>

          {/* Feedback */}
          {submission.feedback && (
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1.5">
                Teacher Feedback & Comments
              </h3>
              <div className="rounded-2xl border border-slate-200 bg-white p-4 text-sm leading-relaxed text-slate-700">
                {submission.feedback}
              </div>
            </div>
          )}
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
