"use client";

import { useEffect, useState } from "react";
import {
  resolveSubmissionFileUrl,
  type SubmissionResponseDto,
} from "@/lib/admin-submissions";
import { getUsers, type UserResponseDto } from "@/lib/admin-users";
import { getTeacherSubmissions, type TeacherAssignmentItemDto } from "@/lib/teacher-assignments";

interface AssignmentDetailsModalProps {
  isOpen: boolean;
  assignment: TeacherAssignmentItemDto | null;
  onClose: () => void;
}

export type StudentSubmissionRow = {
  studentId: string;
  studentName: string;
  studentEmail: string;
  isSubmitted: boolean;
  submission?: SubmissionResponseDto;
};

export function AssignmentDetailsModal({
  isOpen,
  assignment,
  onClose,
}: AssignmentDetailsModalProps) {
  const [activeTab, setActiveTab] = useState<"overview" | "submissions">("submissions");
  const [loading, setLoading] = useState(false);
  const [studentRows, setStudentRows] = useState<StudentSubmissionRow[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && assignment) {
      fetchDetailsAndSubmissions();
    }
  }, [isOpen, assignment]);

  const fetchDetailsAndSubmissions = async () => {
    if (!assignment) return;
    setLoading(true);
    setError(null);

    try {
      // 1. Fetch teacher submissions using /api/Teacher/Submissions
      const submissionsRes = await getTeacherSubmissions({
        assignmentTitle: assignment.title,
        pageNumber: 1,
        pageSize: 100,
      });

      const submissions = submissionsRes.items || [];

      // 2. Safely attempt to fetch student users (if teacher has permissions)
      let students: UserResponseDto[] = [];
      try {
        const usersRes = await getUsers({ role: "Student", pageNumber: 1, pageSize: 100 });
        students = usersRes.items || [];
      } catch {
        // Fallback: If teacher role does not have admin user access, derive student rows from submissions
      }

      // Combine student list with submissions
      const rows: StudentSubmissionRow[] = students.map((std) => {
        const sub = submissions.find(
          (s) =>
            s.studentEmail.toLowerCase() === std.email.toLowerCase() ||
            s.studentName.toLowerCase() === std.fullName.toLowerCase()
        );
        return {
          studentId: std.id,
          studentName: std.fullName,
          studentEmail: std.email,
          isSubmitted: !!sub,
          submission: sub,
        };
      });

      // Add any additional submissions not in the student list
      submissions.forEach((sub) => {
        if (!rows.some((r) => r.studentEmail.toLowerCase() === sub.studentEmail.toLowerCase())) {
          rows.push({
            studentId: sub.id,
            studentName: sub.studentName,
            studentEmail: sub.studentEmail,
            isSubmitted: true,
            submission: sub,
          });
        }
      });

      setStudentRows(rows);
    } catch (err) {
      console.error("Failed to load submissions:", err);
      setError("Unable to load submissions list.");
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen || !assignment) return null;

  const isImageFile = (url?: string) => {
    if (!url) return false;
    const clean = url.toLowerCase();
    return (
      clean.endsWith(".png") ||
      clean.endsWith(".jpg") ||
      clean.endsWith(".jpeg") ||
      clean.endsWith(".webp") ||
      clean.endsWith(".gif") ||
      clean.endsWith(".svg")
    );
  };

  const isPdfFile = (url?: string) => {
    if (!url) return false;
    return url.toLowerCase().endsWith(".pdf");
  };

  const totalStudents = studentRows.length;
  const submittedCount = studentRows.filter((r) => r.isSubmitted).length;
  const gradedCount = studentRows.filter((r) => r.submission?.status === "Graded").length;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 p-4 backdrop-blur-xs animate-in fade-in duration-200">
      <div className="w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden rounded-3xl border border-white/80 bg-white shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-slate-100 p-6 sm:p-8 bg-slate-50/50">
          <div>
            <div className="flex items-center gap-2 mb-1">
              <span className="rounded-full bg-teal-100 px-3 py-0.5 text-xs font-semibold text-teal-800">
                {assignment.className}
              </span>
              <span className="rounded-full bg-purple-100 px-3 py-0.5 text-xs font-semibold text-purple-800">
                {assignment.subjectCode} ({assignment.subjectName})
              </span>
              <span
                className={`rounded-full px-3 py-0.5 text-xs font-semibold ${
                  assignment.status === "Active" || assignment.status === "Published"
                    ? "bg-emerald-100 text-emerald-800"
                    : "bg-amber-100 text-amber-800"
                }`}
              >
                {assignment.status}
              </span>
            </div>
            <h2 className="text-2xl font-bold text-slate-900">{assignment.title}</h2>
            <p className="text-xs text-slate-500 mt-1">
              Due Date: {new Date(assignment.dueDate).toLocaleString()} | Max Marks: {assignment.maxMarks}
            </p>
          </div>

          <button
            onClick={onClose}
            className="rounded-full p-2 text-slate-400 hover:bg-slate-200 hover:text-slate-700 transition"
          >
            ✕
          </button>
        </div>

        {/* Tab Selection */}
        <div className="flex border-b border-slate-200 bg-white px-6 sm:px-8">
          <button
            onClick={() => setActiveTab("submissions")}
            className={`py-3 px-4 text-xs font-semibold border-b-2 transition ${
              activeTab === "submissions"
                ? "border-teal-600 text-teal-700"
                : "border-transparent text-slate-500 hover:text-slate-800"
            }`}
          >
            Student Submissions ({submittedCount}/{totalStudents})
          </button>
          <button
            onClick={() => setActiveTab("overview")}
            className={`py-3 px-4 text-xs font-semibold border-b-2 transition ${
              activeTab === "overview"
                ? "border-teal-600 text-teal-700"
                : "border-transparent text-slate-500 hover:text-slate-800"
            }`}
          >
            Assignment Guidelines & Info
          </button>
        </div>

        {/* Content Body */}
        <div className="flex-1 overflow-y-auto p-6 sm:p-8 space-y-6">
          {error && (
            <div className="rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-medium text-rose-700">
              {error}
            </div>
          )}

          {activeTab === "overview" ? (
            <div className="space-y-6 text-xs text-slate-700">
              <div className="rounded-2xl border border-slate-200 bg-slate-50/50 p-5">
                <h4 className="font-semibold text-slate-900 uppercase tracking-wider text-[11px] mb-2">
                  Instructions & Description
                </h4>
                <p className="text-sm font-medium text-slate-800 leading-relaxed whitespace-pre-line">
                  {assignment.description || "No specific instructions provided."}
                </p>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div className="rounded-2xl border border-slate-200 p-4 bg-white shadow-2xs">
                  <span className="block text-[11px] uppercase tracking-wider font-semibold text-slate-400">
                    Allow Late Submission
                  </span>
                  <span className="text-sm font-semibold text-slate-900 mt-1 block">
                    {assignment.allowLateSubmission ? "Yes ✅" : "No ❌"}
                  </span>
                </div>

                <div className="rounded-2xl border border-slate-200 p-4 bg-white shadow-2xs">
                  <span className="block text-[11px] uppercase tracking-wider font-semibold text-slate-400">
                    Allow Resubmission
                  </span>
                  <span className="text-sm font-semibold text-slate-900 mt-1 block">
                    {assignment.allowResubmission ? "Yes ✅" : "No ❌"}
                  </span>
                </div>

                <div className="rounded-2xl border border-slate-200 p-4 bg-white shadow-2xs">
                  <span className="block text-[11px] uppercase tracking-wider font-semibold text-slate-400">
                    Graded Submissions
                  </span>
                  <span className="text-sm font-semibold text-slate-900 mt-1 block">
                    {gradedCount} / {submittedCount}
                  </span>
                </div>
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                  Enrolled Students & Submission Status ({studentRows.length})
                </h3>
                {loading && (
                  <span className="text-xs text-teal-600 font-medium animate-pulse">
                    Refreshing submissions...
                  </span>
                )}
              </div>

              {studentRows.length > 0 ? (
                <div className="space-y-3">
                  {studentRows.map((row) => {
                    const sub = row.submission;
                    const fullFileUrl = sub?.fileUrl ? resolveSubmissionFileUrl(sub.fileUrl) : null;

                    return (
                      <div
                        key={row.studentId}
                        className="rounded-2xl border border-slate-200 bg-white p-4 shadow-2xs hover:border-slate-300 transition"
                      >
                        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-100 pb-3">
                          <div>
                            <span className="font-semibold text-sm text-slate-900">
                              {row.studentName}
                            </span>
                            <span className="ml-2 text-xs text-slate-500">({row.studentEmail})</span>
                          </div>

                          <div>
                            {row.isSubmitted ? (
                              <span
                                className={`inline-flex items-center gap-1 rounded-full px-3 py-0.5 text-xs font-semibold ${
                                  sub?.status === "Graded"
                                    ? "bg-emerald-100 text-emerald-800"
                                    : "bg-teal-100 text-teal-800"
                                }`}
                              >
                                {sub?.status === "Graded"
                                  ? `Graded: ${sub.grade}/${sub.maxMarks}`
                                  : "Submitted"}
                              </span>
                            ) : (
                              <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-0.5 text-xs font-semibold text-slate-500">
                                Pending / Not Submitted
                              </span>
                            )}
                          </div>
                        </div>

                        {/* Submission details if submitted */}
                        {row.isSubmitted && sub && (
                          <div className="mt-3 space-y-3 text-xs">
                            <div className="flex flex-wrap items-center justify-between text-slate-500">
                              <span>
                                Submitted on: {new Date(sub.submittedAt).toLocaleString()}
                              </span>
                              {sub.grade !== undefined && sub.grade !== null && (
                                <span className="font-semibold text-slate-700">
                                  Score: {sub.grade} / {sub.maxMarks}
                                </span>
                              )}
                            </div>

                            {sub.feedback && (
                              <div className="rounded-xl border border-teal-100 bg-teal-50/50 p-2.5 text-slate-700">
                                <span className="font-semibold text-teal-800">Teacher Feedback: </span>
                                {sub.feedback}
                              </div>
                            )}

                            {/* Attached Submission File */}
                            {sub.fileUrl ? (
                              <div className="mt-2 rounded-xl border border-slate-200 bg-slate-50 p-3 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
                                <div className="flex items-center gap-3">
                                  {isImageFile(sub.fileUrl) ? (
                                    <div className="h-12 w-12 rounded-lg border border-slate-300 overflow-hidden bg-slate-200 shrink-0">
                                      <img
                                        src={fullFileUrl || ""}
                                        alt="Submission preview"
                                        className="h-full w-full object-cover"
                                      />
                                    </div>
                                  ) : isPdfFile(sub.fileUrl) ? (
                                    <div className="h-10 w-10 rounded-lg bg-rose-100 text-rose-700 font-bold flex items-center justify-center text-xs shrink-0">
                                      PDF
                                    </div>
                                  ) : (
                                    <div className="h-10 w-10 rounded-lg bg-indigo-100 text-indigo-700 font-bold flex items-center justify-center text-xs shrink-0">
                                      DOC
                                    </div>
                                  )}

                                  <div>
                                    <span className="block text-xs font-semibold text-slate-800 break-all">
                                      {sub.fileUrl.split("/").pop() || "Submission_File"}
                                    </span>
                                    <span className="text-[11px] text-slate-500">
                                      Location: {sub.fileUrl}
                                    </span>
                                  </div>
                                </div>

                                <div className="flex items-center gap-2 shrink-0">
                                  {fullFileUrl && (
                                    <a
                                      href={fullFileUrl}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                      className="rounded-full border border-teal-600 bg-teal-50 px-3.5 py-1.5 text-xs font-semibold text-teal-700 hover:bg-teal-600 hover:text-white transition"
                                    >
                                      👁️ View File
                                    </a>
                                  )}
                                  {fullFileUrl && (
                                    <a
                                      href={fullFileUrl}
                                      download
                                      className="rounded-full bg-slate-900 px-3.5 py-1.5 text-xs font-semibold text-white hover:bg-slate-800 transition"
                                    >
                                      ⬇️ Download
                                    </a>
                                  )}
                                </div>
                              </div>
                            ) : (
                              <p className="text-slate-400 italic">No file attached with submission.</p>
                            )}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="rounded-2xl border border-dashed border-slate-200 p-8 text-center text-xs text-slate-400">
                  No enrolled students found for this assignment class.
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="border-t border-slate-100 p-4 sm:px-8 flex items-center justify-end bg-slate-50/50">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-slate-200 bg-white px-6 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-100 transition"
          >
            Close Details
          </button>
        </div>
      </div>
    </div>
  );
}
