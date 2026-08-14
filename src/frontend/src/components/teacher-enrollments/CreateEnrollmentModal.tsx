"use client";

import { useEffect, useState } from "react";
import { enrollStudent, type StudentEnrollmentCreateDto, type StudentEnrollmentResponseDto } from "@/lib/teacher-enrollments";
import { getTeacherClasses } from "@/lib/teacher-classes";
import { type TeacherAssignedClassSubjectDto } from "@/lib/teacher-dashboard";
import { getUsers, type UserResponseDto } from "@/lib/admin-users";

type CreateEnrollmentModalProps = {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (enrollment: StudentEnrollmentResponseDto) => void;
};

export function CreateEnrollmentModal({ isOpen, onClose, onSuccess }: CreateEnrollmentModalProps) {
  const [formData, setFormData] = useState<StudentEnrollmentCreateDto>({
    studentEmail: "",
    classId: "",
  });

  const [classes, setClasses] = useState<TeacherAssignedClassSubjectDto[]>([]);
  const [students, setStudents] = useState<UserResponseDto[]>([]);
  const [loadingData, setLoadingData] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      loadSelectOptions();
    }
  }, [isOpen]);

  const loadSelectOptions = async () => {
    setLoadingData(true);
    setError(null);
    try {
      // Load teacher assigned classes and deduplicate by classId
      const classesData = await getTeacherClasses({ pageNumber: 1, pageSize: 100 });
      const rawClasses = classesData.items || [];
      const uniqueClasses: TeacherAssignedClassSubjectDto[] = [];
      const seenClassIds = new Set<string>();

      for (const cls of rawClasses) {
        const idKey = cls.classId || cls.classSubjectId;
        if (idKey && !seenClassIds.has(idKey)) {
          seenClassIds.add(idKey);
          uniqueClasses.push(cls);
        }
      }

      setClasses(uniqueClasses);

      // Load registered student users
      const usersData = await getUsers({ role: "Student", pageNumber: 1, pageSize: 100 });
      setStudents(usersData.items || []);
    } catch {
      // Ignore if teacher permissions differ for global user list
    } finally {
      setLoadingData(false);
    }
  };

  if (!isOpen) return null;

  const handleChange = (e: React.ChangeEvent<HTMLSelectElement | HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (error) setError(null);
  };

  const handleStudentSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const selectedEmail = e.target.value;
    const foundStudent = students.find((s) => s.email.toLowerCase() === selectedEmail.toLowerCase());
    setFormData((prev) => ({
      ...prev,
      studentEmail: selectedEmail,
      studentId: foundStudent?.id || undefined,
    }));
    if (error) setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.classId) {
      setError("Please select a class.");
      return;
    }
    if (!formData.studentEmail.trim()) {
      setError("Please enter or select a student email address.");
      return;
    }
    if (!formData.studentEmail.includes("@")) {
      setError("Please enter a valid student email address.");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const created = await enrollStudent(formData);
      onSuccess(created);
      onClose();
      setFormData({ studentEmail: "", classId: "" });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to enroll student.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4 backdrop-blur-xs">
      <div className="w-full max-w-md rounded-3xl border border-slate-200 bg-white p-6 shadow-xl sm:p-8">
        <div className="mb-6 flex items-center justify-between border-b border-slate-100 pb-4">
          <div>
            <h2 className="text-xl font-bold text-slate-900">Enroll Student by Email</h2>
            <p className="text-xs text-slate-500">Provide a valid student email to add them to your class roster</p>
          </div>
          <button
            onClick={onClose}
            className="rounded-full p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {error && (
          <div className="mb-4 rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs font-medium text-rose-800">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Class Select */}
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
              Select Class <span className="text-rose-500">*</span>
            </label>
            <select
              name="classId"
              value={formData.classId}
              onChange={handleChange}
              disabled={loadingData || loading}
              className="w-full rounded-xl border border-slate-200 px-3.5 py-2.5 text-sm text-slate-900 focus:border-indigo-500 focus:outline-hidden focus:ring-1 focus:ring-indigo-500"
            >
              <option value="">-- Choose Class --</option>
              {classes.map((c, idx) => (
                <option key={c.classSubjectId || `${c.classId}-${idx}`} value={c.classId || c.classSubjectId}>
                  {c.className} {c.classSection ? `(Sec ${c.classSection})` : ""} - {c.academicYear || "Current"}
                </option>
              ))}
            </select>
          </div>

          {/* Student Email Select / Input */}
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
              Student Email <span className="text-rose-500">*</span>
            </label>
            {students.length > 0 && (
              <div className="mb-2">
                <select
                  value={formData.studentEmail}
                  onChange={handleStudentSelect}
                  disabled={loadingData || loading}
                  className="w-full rounded-xl border border-slate-200 px-3.5 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-hidden focus:ring-1 focus:ring-indigo-500"
                >
                  <option value="">-- Select Registered Student --</option>
                  {students.map((s) => (
                    <option key={s.id} value={s.email}>
                      {s.fullName} {s.rollNo ? `(Roll: ${s.rollNo})` : ""} - {s.email}
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-[11px] text-slate-400">Or type student email address manually below:</p>
              </div>
            )}

            <input
              type="email"
              name="studentEmail"
              value={formData.studentEmail}
              onChange={handleChange}
              placeholder="e.g. student@school.com"
              disabled={loading}
              className="w-full rounded-xl border border-slate-200 px-3.5 py-2.5 text-sm text-slate-900 focus:border-indigo-500 focus:outline-hidden focus:ring-1 focus:ring-indigo-500"
            />
          </div>

          <div className="mt-6 flex items-center justify-end gap-3 border-t border-slate-100 pt-4">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-5 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50"
            >
              {loading ? "Verifying & Enrolling..." : "Enroll Student"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
