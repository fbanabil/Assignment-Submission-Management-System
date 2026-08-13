import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

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
  classSubjectId?: string;
  classId?: string;
  subjectId?: string;
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

export class TeacherAssignmentApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "TeacherAssignmentApiError";
  }
}

export async function getTeacherAssignments(
  filter: TeacherAssignmentFilterDto
): Promise<PagedTeacherAssignmentResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.teacherName) query.set("teacherName", filter.teacherName);
  if (filter.teacherEmail) query.set("teacherEmail", filter.teacherEmail);
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Admin/TeacherAssignments?${query.toString()}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 404 || response.status === 415) {
        return {
          items: [],
          totalCount: 0,
          pageNumber,
          pageSize,
          totalPages: 1,
          hasPreviousPage: false,
          hasNextPage: false,
          dataSource: "Server API",
          fetchedAt: new Date().toISOString(),
        };
      }
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
    return {
      items: [],
      totalCount: 0,
      pageNumber,
      pageSize,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  }
}

export async function createTeacherAssignment(
  dto: TeacherAssignmentCreateDto,
  meta?: { teacherName?: string; teacherEmail?: string; className?: string; classSection?: string; academicYear?: string; subjectName?: string; subjectCode?: string }
): Promise<TeacherAssignmentResponseDto> {
  const response = await authenticatedFetch("/Admin/TeacherAssignments", {
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

export async function deleteTeacherAssignment(id: string): Promise<void> {
  const response = await authenticatedFetch(`/Admin/DeleteTeacherAssignment/TeacherAssignments/${id}`, { method: "DELETE" });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentApiError(response.status, errMessage);
  }
}
