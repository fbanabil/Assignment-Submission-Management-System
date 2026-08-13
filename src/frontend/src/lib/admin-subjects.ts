import { getApiUrl, parseApiResponseError, safeParseJson } from "./api-error";
import { getClasses, type ClassResponseDto } from "./admin-classes";

export type ClassSummaryDto = {
  id: string;
  name: string;
  section: string;
  academicYear: string;
};

export type SubjectResponseDto = {
  id: string;
  name: string;
  code: string;
  linkedClasses: ClassSummaryDto[];
};

export type SubjectCreateDto = {
  name: string;
  code: string;
};

export type SubjectUpdateDto = {
  id: string;
  name?: string;
  code?: string;
};

export type SubjectFilterDto = {
  name?: string;
  code?: string;
  classId?: string;
  pageNumber: number;
  pageSize: number;
};

export type ClassSubjectCreateDto = {
  classId: string;
  subjectId: string;
};

export type PagedSubjectResultDto = {
  items: SubjectResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");

export class SubjectApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "SubjectApiError";
  }
}

// In-memory fallback dataset for frontend demo preview
let demoSubjectsDatabase: SubjectResponseDto[] = [
  {
    id: "sbj-001",
    name: "Mathematics",
    code: "MATH101",
    linkedClasses: [
      { id: "cls-001", name: "Grade 10 - Mathematics", section: "Section A", academicYear: "2024-2025" },
    ],
  },
  {
    id: "sbj-002",
    name: "Physics",
    code: "PHYS102",
    linkedClasses: [
      { id: "cls-004", name: "Grade 11 - Physics", section: "Section C", academicYear: "2024-2025" },
    ],
  },
  {
    id: "sbj-003",
    name: "Computer Science",
    code: "CS103",
    linkedClasses: [
      { id: "cls-003", name: "Grade 11 - Computer Science", section: "Section A", academicYear: "2024-2025" },
    ],
  },
  {
    id: "sbj-004",
    name: "Chemistry",
    code: "CHEM104",
    linkedClasses: [
      { id: "cls-006", name: "Grade 12 - Chemistry", section: "Section B", academicYear: "2024-2025" },
    ],
  },
  {
    id: "sbj-005",
    name: "Biology",
    code: "BIO105",
    linkedClasses: [
      { id: "cls-008", name: "Grade 9 - Biology", section: "Section D", academicYear: "2024-2025" },
    ],
  },
  {
    id: "sbj-006",
    name: "General History",
    code: "HIST106",
    linkedClasses: [
      { id: "cls-007", name: "Grade 9 - General History", section: "Section A", academicYear: "2024-2025" },
    ],
  },
  {
    id: "sbj-007",
    name: "Advanced English",
    code: "ENG107",
    linkedClasses: [
      { id: "cls-005", name: "Grade 12 - Advanced English", section: "Section A", academicYear: "2024-2025" },
    ],
  },
];

/**
 * Retrieves paginated subjects with filtering (Name, Code, ClassId)
 */
export async function getSubjects(filter: SubjectFilterDto): Promise<PagedSubjectResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  if (!apiBaseUrl) {
    let filtered = [...demoSubjectsDatabase];

    if (filter.name && filter.name.trim() !== "") {
      const nameVal = filter.name.trim().toLowerCase();
      filtered = filtered.filter((s) => s.name.toLowerCase().includes(nameVal));
    }

    if (filter.code && filter.code.trim() !== "") {
      const codeVal = filter.code.trim().toLowerCase();
      filtered = filtered.filter((s) => s.code.toLowerCase().includes(codeVal));
    }

    if (filter.classId && filter.classId.trim() !== "") {
      filtered = filtered.filter((s) => s.linkedClasses.some((c) => c.id === filter.classId));
    }

    const totalCount = filtered.length;
    const totalPages = Math.ceil(totalCount / pageSize) || 1;
    const startIndex = (pageNumber - 1) * pageSize;
    const items = filtered.slice(startIndex, startIndex + pageSize);

    return {
      items,
      totalCount,
      pageNumber,
      pageSize,
      totalPages,
      hasPreviousPage: pageNumber > 1,
      hasNextPage: pageNumber < totalPages,
      dataSource: "demo (fallback)",
      fetchedAt: new Date().toISOString(),
    };
  }

  const query = new URLSearchParams();
  if (filter.name) query.set("name", filter.name);
  if (filter.code) query.set("code", filter.code);
  if (filter.classId) query.set("classId", filter.classId);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const url = getApiUrl(`/Admin/Subjects?${query.toString()}`);

  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
      const errMessage = await parseApiResponseError(response);
      throw new SubjectApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedSubjectResultDto>(response, {
      items: [],
      totalCount: 0,
      pageNumber,
      pageSize,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    });
  } catch (err) {
    if (err instanceof SubjectApiError) throw err;
    throw new Error(`Failed to fetch subject list: ${err instanceof Error ? err.message : String(err)}`);
  }
}

/**
 * Creates a new subject
 */
export async function createSubject(dto: SubjectCreateDto): Promise<SubjectResponseDto> {
  if (!apiBaseUrl) {
    const newId = `sbj-${String(demoSubjectsDatabase.length + 1).padStart(3, "0")}`;
    const newSubject: SubjectResponseDto = {
      id: newId,
      name: dto.name,
      code: dto.code,
      linkedClasses: [],
    };
    demoSubjectsDatabase = [newSubject, ...demoSubjectsDatabase];
    return newSubject;
  }

  const url = getApiUrl("/Admin/Subjects");
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }

  return await safeParseJson<SubjectResponseDto>(response, {
    id: "",
    name: dto.name,
    code: dto.code,
    linkedClasses: [],
  });
}

/**
 * Updates an existing subject
 */
export async function updateSubject(dto: SubjectUpdateDto): Promise<SubjectResponseDto> {
  if (!apiBaseUrl) {
    const index = demoSubjectsDatabase.findIndex((s) => s.id === dto.id);
    if (index === -1) {
      throw new Error(`Subject with ID ${dto.id} not found.`);
    }

    const existing = demoSubjectsDatabase[index];
    const updated: SubjectResponseDto = {
      ...existing,
      name: dto.name ?? existing.name,
      code: dto.code ?? existing.code,
    };
    demoSubjectsDatabase[index] = updated;
    return updated;
  }

  const url = getApiUrl(`/Admin/Subjects/${dto.id}`);
  const response = await fetch(url, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }

  return await safeParseJson<SubjectResponseDto>(response, {
    id: dto.id,
    name: dto.name || "",
    code: dto.code || "",
    linkedClasses: [],
  });
}

/**
 * Deletes a subject
 */
export async function deleteSubject(id: string): Promise<void> {
  if (!apiBaseUrl) {
    demoSubjectsDatabase = demoSubjectsDatabase.filter((s) => s.id !== id);
    return;
  }

  const url = getApiUrl(`/Admin/Subjects/${id}`);
  const response = await fetch(url, { method: "DELETE" });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }
}

/**
 * Links a subject to a class (ClassSubject)
 */
export async function linkSubjectToClass(dto: ClassSubjectCreateDto, selectedClassData?: ClassResponseDto): Promise<ClassSummaryDto> {
  if (!apiBaseUrl) {
    const subject = demoSubjectsDatabase.find((s) => s.id === dto.subjectId);
    if (subject && selectedClassData) {
      const alreadyLinked = subject.linkedClasses.some((c) => c.id === selectedClassData.id);
      if (!alreadyLinked) {
        const classSummary: ClassSummaryDto = {
          id: selectedClassData.id,
          name: selectedClassData.name,
          section: selectedClassData.section,
          academicYear: selectedClassData.academicYear,
        };
        subject.linkedClasses.push(classSummary);
        return classSummary;
      }
    }
    return {
      id: dto.classId,
      name: selectedClassData?.name || "Linked Class",
      section: selectedClassData?.section || "",
      academicYear: selectedClassData?.academicYear || "",
    };
  }

  const url = getApiUrl("/Admin/ClassSubjects");
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }

  return await safeParseJson<ClassSummaryDto>(response, {
    id: dto.classId,
    name: selectedClassData?.name || "",
    section: selectedClassData?.section || "",
    academicYear: selectedClassData?.academicYear || "",
  });
}

/**
 * Unlinks a subject from a class
 */
export async function unlinkSubjectFromClass(classId: string, subjectId: string): Promise<void> {
  if (!apiBaseUrl) {
    const subject = demoSubjectsDatabase.find((s) => s.id === subjectId);
    if (subject) {
      subject.linkedClasses = subject.linkedClasses.filter((c) => c.id !== classId);
    }
    return;
  }

  const url = getApiUrl(`/Admin/ClassSubjects?classId=${classId}&subjectId=${subjectId}`);
  const response = await fetch(url, { method: "DELETE" });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }
}
