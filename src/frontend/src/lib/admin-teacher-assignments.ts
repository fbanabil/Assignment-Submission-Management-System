import { getApiUrl, parseApiResponseError, safeParseJson } from "./api-error";

export type TeacherAssignmentResponseDto = {
  id: string;
  teacherName: string;
  teacherEmail: string;
  className: string;
  classSection: string;
  academicYear: string;
  subjectName: string;
  subjectCode: string;
  assignedAt: string;
};

export type TeacherAssignmentCreateDto = {
  teacherId: string;
  classSubjectId: string;
};

export type TeacherAssignmentFilterDto = {
  teacherName?: string;
  teacherEmail?: string;
  className?: string;
  subjectCode?: string;
  pageNumber: number;
  pageSize: number;
};

export type PagedTeacherAssignmentResultDto = {
  items: TeacherAssignmentResponseDto[];
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

export class TeacherAssignmentApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "TeacherAssignmentApiError";
  }
}

// In-memory fallback dataset for frontend demo preview
let demoAssignmentsDatabase: TeacherAssignmentResponseDto[] = [
  {
    id: "tas-001",
    teacherName: "Marcus Sterling",
    teacherEmail: "marcus.sterling@school.edu",
    className: "Grade 10 - Mathematics",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Mathematics",
    subjectCode: "MATH101",
    assignedAt: "2024-01-16T10:00:00Z",
  },
  {
    id: "tas-002",
    teacherName: "Sophia Rodriguez",
    teacherEmail: "sophia.rodriguez@school.edu",
    className: "Grade 11 - Physics",
    classSection: "Section C",
    academicYear: "2024-2025",
    subjectName: "Physics",
    subjectCode: "PHYS102",
    assignedAt: "2024-01-21T09:30:00Z",
  },
  {
    id: "tas-003",
    teacherName: "David Chen",
    teacherEmail: "david.chen@school.edu",
    className: "Grade 11 - Computer Science",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Computer Science",
    subjectCode: "CS103",
    assignedAt: "2024-01-25T14:00:00Z",
  },
  {
    id: "tas-004",
    teacherName: "James Maxwell",
    teacherEmail: "james.maxwell@school.edu",
    className: "Grade 12 - Chemistry",
    classSection: "Section B",
    academicYear: "2024-2025",
    subjectName: "Chemistry",
    subjectCode: "CHEM104",
    assignedAt: "2024-02-06T11:15:00Z",
  },
];

/**
 * Retrieves paginated teacher assignments with filtering options (TeacherName, TeacherEmail, ClassName, SubjectCode)
 */
export async function getTeacherAssignments(
  filter: TeacherAssignmentFilterDto
): Promise<PagedTeacherAssignmentResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  if (!apiBaseUrl) {
    let filtered = [...demoAssignmentsDatabase];

    if (filter.teacherName && filter.teacherName.trim() !== "") {
      const nameVal = filter.teacherName.trim().toLowerCase();
      filtered = filtered.filter((a) => a.teacherName.toLowerCase().includes(nameVal));
    }

    if (filter.teacherEmail && filter.teacherEmail.trim() !== "") {
      const emailVal = filter.teacherEmail.trim().toLowerCase();
      filtered = filtered.filter((a) => a.teacherEmail.toLowerCase().includes(emailVal));
    }

    if (filter.className && filter.className.trim() !== "") {
      const classVal = filter.className.trim().toLowerCase();
      filtered = filtered.filter((a) => a.className.toLowerCase().includes(classVal));
    }

    if (filter.subjectCode && filter.subjectCode.trim() !== "") {
      const codeVal = filter.subjectCode.trim().toLowerCase();
      filtered = filtered.filter((a) => a.subjectCode.toLowerCase().includes(codeVal));
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
  if (filter.teacherName) query.set("teacherName", filter.teacherName);
  if (filter.teacherEmail) query.set("teacherEmail", filter.teacherEmail);
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const url = getApiUrl(`/Admin/TeacherAssignments?${query.toString()}`);

  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
      const errMessage = await parseApiResponseError(response);
      throw new TeacherAssignmentApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedTeacherAssignmentResultDto>(response, {
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
    if (err instanceof TeacherAssignmentApiError) throw err;
    throw new Error(`Failed to fetch teacher assignments: ${err instanceof Error ? err.message : String(err)}`);
  }
}

/**
 * Assigns a teacher to a class+subject pair
 */
export async function createTeacherAssignment(
  dto: TeacherAssignmentCreateDto,
  meta?: { teacherName?: string; teacherEmail?: string; className?: string; classSection?: string; academicYear?: string; subjectName?: string; subjectCode?: string }
): Promise<TeacherAssignmentResponseDto> {
  if (!apiBaseUrl) {
    const newId = `tas-${String(demoAssignmentsDatabase.length + 1).padStart(3, "0")}`;
    const newRecord: TeacherAssignmentResponseDto = {
      id: newId,
      teacherName: meta?.teacherName || "Assigned Teacher",
      teacherEmail: meta?.teacherEmail || "",
      className: meta?.className || "Class Section",
      classSection: meta?.classSection || "A",
      academicYear: meta?.academicYear || "2024-2025",
      subjectName: meta?.subjectName || "Subject",
      subjectCode: meta?.subjectCode || "SUB101",
      assignedAt: new Date().toISOString(),
    };
    demoAssignmentsDatabase = [newRecord, ...demoAssignmentsDatabase];
    return newRecord;
  }

  const url = getApiUrl("/Admin/TeacherAssignments");
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentApiError(response.status, errMessage);
  }

  return await safeParseJson<TeacherAssignmentResponseDto>(response, {
    id: "",
    teacherName: meta?.teacherName || "",
    teacherEmail: meta?.teacherEmail || "",
    className: meta?.className || "",
    classSection: meta?.classSection || "",
    academicYear: meta?.academicYear || "",
    subjectName: meta?.subjectName || "",
    subjectCode: meta?.subjectCode || "",
    assignedAt: new Date().toISOString(),
  });
}

/**
 * Removes a teacher assignment
 */
export async function deleteTeacherAssignment(id: string): Promise<void> {
  if (!apiBaseUrl) {
    demoAssignmentsDatabase = demoAssignmentsDatabase.filter((a) => a.id !== id);
    return;
  }

  const url = getApiUrl(`/Admin/TeacherAssignments/${id}`);
  const response = await fetch(url, { method: "DELETE" });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentApiError(response.status, errMessage);
  }
}
