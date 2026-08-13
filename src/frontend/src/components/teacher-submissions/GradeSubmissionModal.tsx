"use client";

import { useEffect, useState } from "react";
import { type SubmissionResponseDto } from "@/lib/admin-submissions";
import { gradeTeacherSubmission } from "@/lib/teacher-assignments";

interface GradeSubmissionModalProps {
  isOpen: boolean;
  submission: SubmissionResponseDto | null;
  onClose: () => void;
  onSuccess: () => void;
}

export function GradeSubmissionModal({
  isOpen,
  submission,
  onClose,
  onSuccess,
}: GradeSubmissionModalProps) {
  const [marks, setMarks] = useState<number>(0);
  const [feedback, setFeedback] = useState("");
  const [loadingSubmit, setLoadingSubmit] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (submission) {
      setMarks(submission.grade || 0);
      setFeedback(submission.feedback || "");
      setError(null);
    }
  }, [submission]);

  if (!isOpen || !submission) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoadingSubmit(true);
    setError(null);

    try {
      await gradeTeacherSubmission({
        submissionId: submission.id,
        marks,
        feedback: feedback.trim(),
      });

      onSuccess();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to grade submission.");
    } finally {
      setLoadingSubmit(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-xs animate-in fade-in duration-200">
      <div className="w-full max-w-lg overflow-hidden rounded-3xl border border-white/80 bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-100 p-6 bg-slate-50/50">
          <div>
            <h3 className="text-lg font-bold text-slate-900">Grade Student Submission</h3>
            <p className="text-xs text-slate-500 mt-0.5">
              Student: <span className="font-semibold text-slate-700">{submission.studentName}</span> ({submission.studentEmail})
            </p>
          </div>
          <button
            onClick={onClose}
            className="rounded-full p-2 text-slate-400 hover:bg-slate-200 hover:text-slate-700 transition"
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4 text-xs">
          {error && (
            <div className="rounded-2xl border border-rose-200 bg-rose-50 p-3 font-semibold text-rose-700">
              {error}
            </div>
          )}

          <div className="rounded-2xl border border-slate-200 bg-slate-50 p-3.5 space-y-1">
            <span className="font-semibold text-slate-800 text-xs">Assignment Details:</span>
            <p className="text-slate-600">{submission.assignmentTitle} ({submission.subjectCode})</p>
            <p className="text-slate-500 font-mono">Max Marks: {submission.maxMarks} pts</p>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Score / Marks (out of {submission.maxMarks})
            </label>
            <input
              type="number"
              min={0}
              max={submission.maxMarks || 100}
              required
              value={marks}
              onChange={(e) => setMarks(Number(e.target.value))}
              className="w-full rounded-2xl border border-slate-200 bg-white px-3.5 py-2 text-sm font-semibold text-slate-900 focus:border-teal-500 focus:outline-none"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Teacher Feedback & Comments
            </label>
            <textarea
              rows={4}
              placeholder="Provide constructive feedback for the student..."
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
              className="w-full rounded-2xl border border-slate-200 bg-white p-3 text-xs text-slate-900 focus:border-teal-500 focus:outline-none"
            />
          </div>

          <div className="flex items-center justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-full border border-slate-200 px-5 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 transition"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loadingSubmit}
              className="rounded-full bg-teal-600 px-6 py-2 text-xs font-semibold text-white hover:bg-teal-700 disabled:opacity-50 transition shadow-sm"
            >
              {loadingSubmit ? "Saving Grade..." : "Submit Grade ✅"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
