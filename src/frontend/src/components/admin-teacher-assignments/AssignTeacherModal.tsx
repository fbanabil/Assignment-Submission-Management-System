"use client";

import { useEffect, useState } from "react";
import { getUsers, type UserResponseDto } from "@/lib/admin-users";
import { getSubjects, type SubjectResponseDto } from "@/lib/admin-subjects";
import {
  createTeacherAssignment,
  type TeacherAssignmentCreateDto,
  type TeacherAssignmentResponseDto,
} from "@/lib/admin-teacher-assignments";

type AssignTeacherModalProps = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (created: TeacherAssignmentResponseDto) => void;
};

export function AssignTeacherModal({ isOpen, onClose, onSuccess }: AssignTeacherModalProps) {
  const [teachers, setTeachers] = useState<UserResponseDto[]>([]);
  const [subjects, setSubjects] = useState<SubjectResponseDto[]>([]);

  const [selectedTeacherId, setSelectedTeacherId] = useState<string>("");
  const [selectedSubjectId, setSelectedSubjectId] = useState<string>("");
  const [selectedClassId, setSelectedClassId] = useState<string>("");

  const [loadingOptions, setLoadingOptions] = useState(false);
  const [loadingSubmit, setLoadingSubmit] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setError(null);
      setSelectedTeacherId("");
      setSelectedSubjectId("");
      setSelectedClassId("");
      fetchDropdownData();
    }
  }, [isOpen]);

  const fetchDropdownData = async () => {
    setLoadingOptions(true);
    try {
      const [usersRes, subjectsRes] = await Promise.all([
        getUsers({ role: "Teacher", pageNumber: 1, pageSize: 100 }),
        getSubjects({ pageNumber: 1, pageSize: 100 }),
      ]);
      setTeachers(usersRes.items);
      setSubjects(subjectsRes.items);
    } catch (err) {
      console.error("Failed to load options for assign teacher modal:", err);
    } finally {
      setLoadingOptions(false);
    }
  };

  if (!isOpen) return null;

  const activeSubject = subjects.find((s) => s.id === selectedSubjectId);
  const availableClasses = activeSubject?.linkedClasses || [];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedTeacherId) {
      setError("Please select a teacher.");
      return;
    }
    if (!selectedSubjectId) {
      setError("Please select a subject.");
      return;
    }
    if (!selectedClassId) {
      setError("Please select a class section.");
      return;
    }

    const teacher = teachers.find((t) => t.id === selectedTeacherId);
    const subject = activeSubject;
    const cls = availableClasses.find((c) => c.id === selectedClassId);

    setLoadingSubmit(true);
    setError(null);

    const dto: TeacherAssignmentCreateDto = {
      teacherId: selectedTeacherId,
      classSubjectId: "00000000-0000-0000-0000-000000000000",
      classId: selectedClassId,
      subjectId: selectedSubjectId,
    };

    try {
      const created = await createTeacherAssignment(dto, {
        teacherName: teacher?.fullName || "Teacher",
        teacherEmail: teacher?.email || "",
        className: cls?.name || "Class",
        classSection: cls?.section || "",
        academicYear: cls?.academicYear || "",
        subjectName: subject?.name || "Subject",
        subjectCode: subject?.code || "",
      });

      onSuccess(created);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to assign teacher.");
    } finally {
      setLoadingSubmit(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-lg overflow-hidden rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-teal-500/15 bg-teal-500/10 px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] text-teal-700">
              Teacher Assignment
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">Assign Teacher to Class & Subject</h2>
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

        <form onSubmit={handleSubmit} className="mt-5 space-y-4">
          {/* Teacher Selector */}
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
              Select Teacher *
            </label>
            <select
              required
              value={selectedTeacherId}
              onChange={(e) => setSelectedTeacherId(e.target.value)}
              disabled={loadingOptions || loadingSubmit}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            >
              <option value="">
                {loadingOptions ? "Loading teachers..." : "-- Select Teacher --"}
              </option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.fullName} ({t.email})
                </option>
              ))}
            </select>
          </div>

          {/* Subject Selector */}
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
              Select Subject *
            </label>
            <select
              required
              value={selectedSubjectId}
              onChange={(e) => {
                setSelectedSubjectId(e.target.value);
                setSelectedClassId("");
              }}
              disabled={loadingOptions || loadingSubmit}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            >
              <option value="">
                {loadingOptions ? "Loading subjects..." : "-- Select Subject --"}
              </option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} ({s.code})
                </option>
              ))}
            </select>
          </div>

          {/* Class Section Selector */}
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1">
              Select Class Section *
            </label>
            <select
              required
              value={selectedClassId}
              onChange={(e) => setSelectedClassId(e.target.value)}
              disabled={!selectedSubjectId || loadingSubmit}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            >
              <option value="">
                {!selectedSubjectId
                  ? "-- First Select a Subject --"
                  : availableClasses.length === 0
                  ? "No linked classes for this subject"
                  : "-- Select Class Section --"}
              </option>
              {availableClasses.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} ({c.section}, {c.academicYear})
                </option>
              ))}
            </select>
          </div>

          <div className="mt-6 flex items-center justify-end gap-3 pt-4 border-t border-black/5">
            <button
              type="button"
              onClick={onClose}
              className="rounded-full border border-slate-200 bg-white px-5 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 transition"
              disabled={loadingSubmit}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loadingSubmit || !selectedTeacherId || !selectedSubjectId || !selectedClassId}
              className="inline-flex items-center gap-2 rounded-full bg-slate-900 px-6 py-2.5 text-sm font-medium text-white shadow-md hover:bg-slate-800 transition disabled:opacity-50 cursor-pointer"
            >
              {loadingSubmit ? (
                <>
                  <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                  Assigning...
                </>
              ) : (
                "Confirm Assignment"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
