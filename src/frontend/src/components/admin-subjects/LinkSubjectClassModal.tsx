"use client";

import { useEffect, useState } from "react";
import { getClasses, type ClassResponseDto } from "@/lib/admin-classes";
import {
  linkSubjectToClass,
  unlinkSubjectFromClass,
  type ClassSummaryDto,
  type SubjectResponseDto,
} from "@/lib/admin-subjects";

type LinkSubjectClassModalProps = {
  isOpen: boolean;
  subjectData: SubjectResponseDto | null;
  onClose: () => void;
  onUpdated: (updatedSubject: SubjectResponseDto) => void;
};

export function LinkSubjectClassModal({
  isOpen,
  subjectData,
  onClose,
  onUpdated,
}: LinkSubjectClassModalProps) {
  const [availableClasses, setAvailableClasses] = useState<ClassResponseDto[]>([]);
  const [selectedClassId, setSelectedClassId] = useState<string>("");
  const [loadingClasses, setLoadingClasses] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && subjectData) {
      setError(null);
      setSelectedClassId("");
      fetchAvailableClasses();
    }
  }, [isOpen, subjectData]);

  const fetchAvailableClasses = async () => {
    setLoadingClasses(true);
    try {
      const res = await getClasses({ pageNumber: 1, pageSize: 100 });
      setAvailableClasses(res.items);
    } catch (err) {
      console.error("Failed to load available classes:", err);
    } finally {
      setLoadingClasses(false);
    }
  };

  if (!isOpen || !subjectData) return null;

  const currentLinked = subjectData.linkedClasses || [];
  const unlinkedOptions = availableClasses.filter(
    (cls) => !currentLinked.some((linked) => linked.id === cls.id)
  );

  const handleLink = async () => {
    if (!selectedClassId) {
      setError("Please select a class to link.");
      return;
    }

    setActionLoading(true);
    setError(null);

    const targetClass = availableClasses.find((c) => c.id === selectedClassId);

    try {
      const linkedSummary = await linkSubjectToClass(
        { classId: selectedClassId, subjectId: subjectData.id },
        targetClass
      );

      const updatedClasses: ClassSummaryDto[] = [...currentLinked];
      if (!updatedClasses.some((c) => c.id === linkedSummary.id)) {
        updatedClasses.push(linkedSummary);
      }

      onUpdated({
        ...subjectData,
        linkedClasses: updatedClasses,
      });

      setSelectedClassId("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to link class.");
    } finally {
      setActionLoading(false);
    }
  };

  const handleUnlink = async (classId: string) => {
    setActionLoading(true);
    setError(null);

    try {
      await unlinkSubjectFromClass(classId, subjectData.id);

      const updatedClasses = currentLinked.filter((c) => c.id !== classId);
      onUpdated({
        ...subjectData,
        linkedClasses: updatedClasses,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to unlink class.");
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-xs p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-xl overflow-hidden rounded-3xl border border-white/80 bg-white p-6 shadow-2xl sm:p-8">
        <div className="flex items-center justify-between pb-4 border-b border-black/5">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-purple-500/15 bg-purple-500/10 px-3 py-0.5 text-xs font-semibold uppercase tracking-[0.2em] text-purple-700">
              Class Assignments
            </span>
            <h2 className="mt-2 text-2xl font-semibold text-foreground">
              Link Classes for {subjectData.name} ({subjectData.code})
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

        {error && (
          <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50/90 p-4 text-xs font-medium text-rose-700">
            {error}
          </div>
        )}

        {/* Currently Linked Classes */}
        <div className="mt-5 space-y-3">
          <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-600">
            Currently Linked Classes ({currentLinked.length})
          </h3>

          {currentLinked.length > 0 ? (
            <div className="max-h-44 overflow-y-auto space-y-2 pr-1">
              {currentLinked.map((cls) => (
                <div
                  key={cls.id}
                  className="flex items-center justify-between rounded-2xl border border-slate-200 bg-slate-50/80 px-4 py-2.5 text-sm"
                >
                  <div>
                    <span className="font-semibold text-slate-800">{cls.name}</span>
                    {cls.section && (
                      <span className="ml-2 inline-flex items-center rounded-full bg-teal-100 px-2.5 py-0.5 text-xs font-semibold text-teal-800">
                        {cls.section}
                      </span>
                    )}
                    {cls.academicYear && (
                      <span className="ml-2 text-xs text-slate-500">({cls.academicYear})</span>
                    )}
                  </div>
                  <button
                    type="button"
                    disabled={actionLoading}
                    onClick={() => handleUnlink(cls.id)}
                    className="rounded-full border border-rose-200 bg-white px-3 py-1 text-xs font-semibold text-rose-600 hover:bg-rose-600 hover:text-white transition cursor-pointer disabled:opacity-50"
                  >
                    Unlink
                  </button>
                </div>
              ))}
            </div>
          ) : (
            <div className="rounded-2xl border border-dashed border-slate-200 p-4 text-center text-xs text-slate-400">
              No classes currently linked to this subject.
            </div>
          )}
        </div>

        {/* Link New Class Dropdown Section */}
        <div className="mt-6 pt-5 border-t border-black/5 space-y-3">
          <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-600">
            Link a New Class
          </h3>

          <div className="flex flex-col sm:flex-row items-center gap-3">
            <select
              value={selectedClassId}
              onChange={(e) => setSelectedClassId(e.target.value)}
              disabled={loadingClasses || actionLoading}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50/60 px-4 py-2.5 text-sm font-medium text-foreground focus:border-teal-500 focus:bg-white focus:outline-none transition"
            >
              <option value="">
                {loadingClasses ? "Loading classes..." : "-- Select Class --"}
              </option>
              {unlinkedOptions.map((cls) => (
                <option key={cls.id} value={cls.id}>
                  {cls.name} ({cls.section || "No section"}, {cls.academicYear})
                </option>
              ))}
            </select>

            <button
              type="button"
              onClick={handleLink}
              disabled={!selectedClassId || actionLoading}
              className="w-full sm:w-auto inline-flex whitespace-nowrap items-center justify-center gap-2 rounded-full bg-teal-600 px-5 py-2.5 text-sm font-medium text-white shadow-md hover:bg-teal-700 transition disabled:opacity-50 cursor-pointer"
            >
              {actionLoading ? "Linking..." : "+ Link Class"}
            </button>
          </div>
        </div>

        <div className="mt-6 flex items-center justify-end pt-4 border-t border-black/5">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-slate-200 bg-white px-6 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 transition"
          >
            Done
          </button>
        </div>
      </div>
    </div>
  );
}
