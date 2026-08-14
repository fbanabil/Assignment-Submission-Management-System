"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  getStudentAssignmentDetail,
  resolveServerFileUrl,
  submitStudentAssignment,
  unsubmitStudentAssignment,
  uploadAssignmentFile,
  type StudentAssignmentDetailDto,
} from "@/lib/student-assignments";

type StudentAssignmentDetailClientProps = {
  id: string;
};

function formatDateTime(value?: string) {
  if (!value) return "N/A";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "full",
    timeStyle: "medium",
  }).format(date);
}

function useCountdown(targetDate?: string) {
  const [timeLeft, setTimeLeft] = useState<{
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
    isPast: boolean;
  }>({ days: 0, hours: 0, minutes: 0, seconds: 0, isPast: false });

  useEffect(() => {
    if (!targetDate) return;

    const calculateTime = () => {
      const target = new Date(targetDate).getTime();
      const now = new Date().getTime();
      const diff = target - now;

      if (diff <= 0) {
        setTimeLeft({ days: 0, hours: 0, minutes: 0, seconds: 0, isPast: true });
        return;
      }

      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);

      setTimeLeft({ days, hours, minutes, seconds, isPast: false });
    };

    calculateTime();
    const interval = setInterval(calculateTime, 1000);
    return () => clearInterval(interval);
  }, [targetDate]);

  return timeLeft;
}

export function StudentAssignmentDetailClient({ id }: StudentAssignmentDetailClientProps) {
  const [data, setData] = useState<StudentAssignmentDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Form State
  const [submissionText, setSubmissionText] = useState("");
  const [fileUrl, setFileUrl] = useState("");
  const [uploadingFile, setUploadingFile] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [unsubmitting, setUnsubmitting] = useState(false);
  const [submitSuccessMessage, setSubmitSuccessMessage] = useState<string | null>(null);

  const countdown = useCountdown(data?.deadline);

  const fetchDetail = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getStudentAssignmentDetail(id);
      if (!res) {
        setError("Assignment not found or no longer available.");
      } else {
        setData(res);
        if (res.existingSubmission) {
          setSubmissionText(res.existingSubmission.submissionText || "");
          setFileUrl(res.existingSubmission.fileUrl || "");
        } else {
          setSubmissionText("");
          setFileUrl("");
        }
      }
    } catch (err) {
      console.error("Failed to load assignment detail:", err);
      setError(err instanceof Error ? err.message : "Unable to load assignment details.");
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchDetail();
  }, [fetchDetail]);

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingFile(true);
    setError(null);

    try {
      const uploadRes = await uploadAssignmentFile(file);
      setFileUrl(uploadRes.filePath);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to upload attachment file.");
    } finally {
      setUploadingFile(false);
    }
  };

  const handleRemoveFile = () => {
    setFileUrl("");
  };

  const handleSubmitWork = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!submissionText.trim() && !fileUrl.trim()) {
      setError("Please provide either submission text or upload a file attachment.");
      return;
    }

    setSubmitting(true);
    setError(null);
    setSubmitSuccessMessage(null);

    try {
      await submitStudentAssignment({
        assignmentId: id,
        submissionText: submissionText.trim() || undefined,
        fileUrl: fileUrl.trim() || undefined,
      });

      setSubmitSuccessMessage("Your work has been submitted successfully!");
      fetchDetail();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to submit assignment.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleUnsubmit = async () => {
    const targetId = data?.existingSubmission?.submissionId || id;
    if (!targetId) return;

    if (!window.confirm("Are you sure you want to unsubmit this assignment? This will remove your submission so you can make changes and resubmit.")) {
      return;
    }

    setUnsubmitting(true);
    setError(null);
    setSubmitSuccessMessage(null);

    try {
      await unsubmitStudentAssignment(targetId);
      setSubmissionText("");
      setFileUrl("");
      setSubmitSuccessMessage("Submission unsubmitted successfully. You can now edit your work and turn it in again.");
      fetchDetail();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to unsubmit assignment.");
    } finally {
      setUnsubmitting(false);
    }
  };

  if (loading) {
    return (
      <main className="min-h-screen px-4 py-8 sm:px-6 lg:px-8">
        <div className="mx-auto flex max-w-4xl flex-col gap-6">
          <div className="flex h-64 items-center justify-center rounded-3xl border border-slate-200 bg-white text-sm text-slate-400">
            Loading assignment details...
          </div>
        </div>
      </main>
    );
  }

  if (error && !data) {
    return (
      <main className="min-h-screen px-4 py-8 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-4xl space-y-4">
          <div className="rounded-3xl border border-rose-200 bg-rose-50 p-6 text-rose-800">
            <h2 className="text-lg font-bold">Unable to Load Assignment</h2>
            <p className="mt-1 text-sm">{error}</p>
          </div>
          <Link
            href="/student/assignments"
            className="inline-flex items-center gap-2 rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
          >
            ← Back to Assignments List
          </Link>
        </div>
      </main>
    );
  }

  const isGraded = data?.status === "Graded";
  const isSubmitted = data?.status === "Submitted" || data?.status === "Graded";
  const canSubmit = !isGraded && (!isSubmitted || data?.allowResubmission) && (!countdown.isPast || data?.allowLateSubmission);
  const canUnsubmit = isSubmitted && !isGraded && data?.allowResubmission && (!countdown.isPast || data?.allowLateSubmission);

  return (
    <main className="min-h-screen px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6">
        {/* Navigation Top Header */}
        <div className="flex items-center justify-between">
          <Link
            href="/student/assignments"
            className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2 text-xs font-semibold text-slate-700 shadow-xs hover:bg-slate-50"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Back to Assignments
          </Link>

          {/* Submission Status Pill Header */}
          <div className="flex items-center gap-2">
            {isGraded ? (
              <span className="rounded-full border border-emerald-200 bg-emerald-100 px-3.5 py-1 text-xs font-bold text-emerald-800">
                ★ GRADED ({data?.existingSubmission?.marks} / {data?.maxMarks} pts)
              </span>
            ) : isSubmitted ? (
              <span className="rounded-full border border-sky-200 bg-sky-100 px-3.5 py-1 text-xs font-bold text-sky-800">
                ✓ SUBMITTED
              </span>
            ) : (
              <span className="rounded-full border border-amber-200 bg-amber-100 px-3.5 py-1 text-xs font-bold text-amber-800">
                ⏳ NOT SUBMITTED YET
              </span>
            )}
          </div>
        </div>

        {/* Live Deadline Countdown Banner */}
        <section className="overflow-hidden rounded-3xl border border-indigo-100 bg-gradient-to-br from-indigo-900 to-slate-900 p-6 text-white shadow-md sm:p-8">
          <div className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
            <div className="space-y-1">
              <div className="inline-flex items-center gap-2 rounded-full border border-indigo-400/30 bg-indigo-500/20 px-3 py-0.5 text-xs font-semibold uppercase tracking-wider text-indigo-200">
                {data?.subjectCode || data?.subjectName} • {data?.className}
              </div>
              <h1 className="text-2xl font-bold tracking-tight text-white sm:text-3xl">{data?.title}</h1>
              <p className="text-xs text-indigo-200">
                Deadline: <span className="font-semibold text-white">{formatDateTime(data?.deadline)}</span>
              </p>
            </div>

            {/* Countdown Box */}
            <div className="flex items-center gap-3 rounded-2xl border border-white/10 bg-white/10 p-4 backdrop-blur-md">
              {countdown.isPast ? (
                <div className="text-center">
                  <span className="text-xs font-semibold tracking-wider text-rose-300 uppercase">Deadline Expired</span>
                  <p className="text-sm font-bold text-white">
                    {data?.allowLateSubmission ? "Late Submissions Allowed" : "Past Due Date"}
                  </p>
                </div>
              ) : (
                <>
                  <div className="text-center px-2">
                    <span className="text-2xl font-black">{countdown.days}</span>
                    <span className="block text-[10px] uppercase text-indigo-200">Days</span>
                  </div>
                  <span className="text-xl font-bold text-indigo-300">:</span>
                  <div className="text-center px-2">
                    <span className="text-2xl font-black">{String(countdown.hours).padStart(2, "0")}</span>
                    <span className="block text-[10px] uppercase text-indigo-200">Hours</span>
                  </div>
                  <span className="text-xl font-bold text-indigo-300">:</span>
                  <div className="text-center px-2">
                    <span className="text-2xl font-black">{String(countdown.minutes).padStart(2, "0")}</span>
                    <span className="block text-[10px] uppercase text-indigo-200">Mins</span>
                  </div>
                  <span className="text-xl font-bold text-indigo-300">:</span>
                  <div className="text-center px-2">
                    <span className="text-2xl font-black">{String(countdown.seconds).padStart(2, "0")}</span>
                    <span className="block text-[10px] uppercase text-indigo-200">Secs</span>
                  </div>
                </>
              )}
            </div>
          </div>
        </section>

        {/* Notifications & Messages */}
        {submitSuccessMessage && (
          <div className="rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-emerald-800 text-sm font-medium">
            {submitSuccessMessage}
          </div>
        )}
        {error && (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 p-4 text-rose-800 text-sm font-medium">
            {error}
          </div>
        )}

        {/* Assignment Information Grid */}
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {/* Left Column: Full Details & Instructions */}
          <section className="flex flex-col gap-6 lg:col-span-2">
            <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
              <h2 className="text-lg font-bold text-slate-900 border-b border-slate-100 pb-3">Assignment Description</h2>
              <div className="prose max-w-none mt-4 text-sm leading-relaxed text-slate-700 whitespace-pre-wrap">
                {data?.description || "No description or instructions provided for this assignment."}
              </div>
            </div>

            {/* Evaluated Grade & Feedback Section if Graded */}
            {data?.existingSubmission && (data.existingSubmission.marks !== null || data.existingSubmission.feedback) && (
              <div className="rounded-3xl border border-emerald-200 bg-emerald-50/50 p-6 shadow-sm sm:p-8">
                <div className="flex items-center justify-between border-b border-emerald-100 pb-3">
                  <h2 className="text-lg font-bold text-emerald-900">Teacher Feedback & Score</h2>
                  <span className="rounded-xl border border-emerald-300 bg-emerald-100 px-3 py-1 text-base font-extrabold text-emerald-900">
                    {data.existingSubmission.marks} / {data.maxMarks}
                  </span>
                </div>
                <div className="mt-4 space-y-2">
                  <p className="text-xs text-emerald-700">
                    Evaluated by: <span className="font-semibold">{data.existingSubmission.gradedByTeacherName || data.teacherName}</span>
                    {data.existingSubmission.gradedAt && ` on ${formatDateTime(data.existingSubmission.gradedAt)}`}
                  </p>
                  <div className="rounded-2xl border border-emerald-200 bg-white p-4 text-sm leading-relaxed text-slate-800 italic">
                    "{data.existingSubmission.feedback || "Great effort!"}"
                  </div>
                </div>
              </div>
            )}
          </section>

          {/* Right Column: Submission Form & Actions */}
          <section className="flex flex-col gap-6">
            {/* Metadata Card */}
            <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
              <h2 className="text-base font-bold text-slate-900 border-b border-slate-100 pb-3">Assignment Details</h2>
              <div className="space-y-3 text-xs">
                <div className="flex justify-between">
                  <span className="text-slate-500">Teacher:</span>
                  <span className="font-semibold text-slate-800">{data?.teacherName}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Max Marks:</span>
                  <span className="font-semibold text-slate-800">{data?.maxMarks} points</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Late Submissions:</span>
                  <span className={`font-semibold ${data?.allowLateSubmission ? "text-emerald-600" : "text-slate-600"}`}>
                    {data?.allowLateSubmission ? "Allowed" : "Not Allowed"}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-500">Resubmissions:</span>
                  <span className={`font-semibold ${data?.allowResubmission ? "text-emerald-600" : "text-slate-600"}`}>
                    {data?.allowResubmission ? "Allowed" : "Disabled"}
                  </span>
                </div>
              </div>
            </div>

            {/* Submission Form Card */}
            <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
              <div className="flex items-center justify-between border-b border-slate-100 pb-3">
                <h2 className="text-base font-bold text-slate-900">
                  {isSubmitted ? "Your Submission" : "Submit Work"}
                </h2>
                {canUnsubmit && (
                  <button
                    type="button"
                    onClick={handleUnsubmit}
                    disabled={unsubmitting}
                    className="rounded-xl border border-rose-300 bg-rose-100 px-3 py-1.5 text-xs font-bold text-rose-800 hover:bg-rose-200 disabled:opacity-50 transition"
                  >
                    {unsubmitting ? "Unsubmitting..." : "Unsubmit Assignment"}
                  </button>
                )}
              </div>

              {/* Status Banner inside Form Card */}
              {isSubmitted ? (
                <div className="rounded-2xl border border-sky-200 bg-sky-50 p-3.5 text-xs text-sky-900 font-medium">
                  <div className="font-bold text-sky-900">✓ Submitted Work</div>
                  {data?.existingSubmission?.submittedAt && (
                    <p className="mt-0.5 text-[11px] text-sky-700">
                      Turned in on {formatDateTime(data.existingSubmission.submittedAt)}
                    </p>
                  )}
                </div>
              ) : (
                <div className="rounded-2xl border border-amber-200 bg-amber-50 p-3.5 text-xs text-amber-900 font-medium">
                  <div className="font-bold text-amber-900">⏳ Work Not Turned In Yet</div>
                  <p className="mt-0.5 text-[11px] text-amber-700">
                    Fill out the form below and click turn in before the deadline.
                  </p>
                </div>
              )}

              <form onSubmit={handleSubmitWork} className="space-y-4">
                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                    Submission Text / Solution
                  </label>
                  <textarea
                    rows={4}
                    value={submissionText}
                    onChange={(e) => setSubmissionText(e.target.value)}
                    placeholder="Write your answer, summary, or response notes here..."
                    disabled={!canSubmit || submitting}
                    className="w-full rounded-2xl border border-slate-200 p-3 text-xs text-slate-900 focus:border-indigo-500 focus:outline-hidden disabled:bg-slate-50 disabled:text-slate-500"
                  />
                </div>

                {/* File Attachment Section */}
                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
                    File Attachment
                  </label>

                  {fileUrl ? (
                    <div className="flex items-center justify-between rounded-xl border border-indigo-200 bg-indigo-50/60 p-3 text-xs">
                      <div className="flex items-center gap-2 overflow-hidden">
                        <svg className="h-5 w-5 text-indigo-600 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
                        </svg>
                        <a
                          href={resolveServerFileUrl(fileUrl)}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="truncate font-semibold text-indigo-700 underline hover:text-indigo-900"
                        >
                          {fileUrl}
                        </a>
                      </div>
                      <div className="flex items-center gap-1.5 shrink-0">
                        <a
                          href={resolveServerFileUrl(fileUrl)}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="rounded-lg bg-indigo-100 px-2.5 py-1 text-xs font-semibold text-indigo-700 hover:bg-indigo-200"
                        >
                          Open File
                        </a>
                        {canSubmit && (
                          <button
                            type="button"
                            onClick={handleRemoveFile}
                            className="rounded-lg bg-rose-100 p-1 text-rose-700 hover:bg-rose-200"
                            title="Remove file"
                          >
                            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                          </button>
                        )}
                      </div>
                    </div>
                  ) : (
                    <div>
                      <input
                        type="file"
                        onChange={handleFileSelect}
                        disabled={!canSubmit || uploadingFile || submitting}
                        className="w-full text-xs text-slate-500 file:mr-3 file:rounded-xl file:border-0 file:bg-slate-900 file:px-3.5 file:py-2 file:text-xs file:font-semibold file:text-white hover:file:bg-slate-800 disabled:opacity-50"
                      />
                      {uploadingFile && <p className="mt-1 text-[11px] font-medium text-indigo-600">Uploading attachment to /wwwroot/assignments/...</p>}
                    </div>
                  )}
                </div>

                {canSubmit ? (
                  <button
                    type="submit"
                    disabled={submitting || uploadingFile}
                    className="w-full rounded-xl bg-indigo-600 py-3 text-xs font-bold uppercase tracking-wider text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50"
                  >
                    {submitting ? "Submitting..." : isSubmitted ? "Resubmit / Save Work" : "Turn In Assignment"}
                  </button>
                ) : (
                  <div className="rounded-xl border border-slate-200 bg-slate-50 p-3 text-center text-xs font-medium text-slate-500">
                    {isGraded
                      ? "This assignment has been graded. Submissions are closed."
                      : !data?.allowResubmission && isSubmitted
                      ? "Submission completed. Resubmissions are disabled."
                      : "Deadline expired. Submissions are closed."}
                  </div>
                )}
              </form>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}
