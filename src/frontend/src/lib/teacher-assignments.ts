import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";
import { type SubmissionResponseDto, type SubmissionFilterDto, type PagedSubmissionResultDto } from "./admin-submissions";

export type AssignmentStatus = "Active" | "Past Due" | "Draft" | "Published" | "Archived";

export type TeacherAssignmentItemDto = {
  id: string;
  title: string;
  description: string;
  className: string;
  classSection: string;
  academicYear: string;
  subjectName: string;
  subjectCode: string;
  teacherName: string;
  teacherEmail: string;
  dueDate: string;
  createdAt: string;
  maxMarks: number;
  status: AssignmentStatus;
  totalSubmissions: number;
  allowLateSubmission: boolean;
  allowResubmission?: boolean;
};

export type TeacherAssignmentFilterDto = {
  title?: string;
  className?: string;
  subjectName?: string;
  subjectCode?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: "Asc" | "Desc";
  pageNumber: number;
  pageSize: number;
};

export type TeacherAssignmentCreateDto = {
  title: string;
  description: string;
  classId: string;
  subjectId: string;
  teacherId?: string;
  deadline?: string;
  dueDate?: string;
  maxMarks: number;
  status: AssignmentStatus | number;
  allowLateSubmission: boolean;
  allowResubmission: boolean;
};

export type TeacherAssignmentUpdateDto = {
  id: string;
  title?: string;
  description?: string;
  dueDate?: string;
  maxMarks?: number;
  status?: AssignmentStatus | number;
  allowLateSubmission?: boolean;
  allowResubmission?: boolean;
};

export type GradeDto = {
  submissionId: string;
  marks: number;
  feedback: string;
};

export type PagedTeacherAssignmentsResultDto = {
  items: TeacherAssignmentItemDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class TeacherAssignmentsApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "TeacherAssignmentsApiError";
  }
}

// In-memory array for client state
let demoTeacherAssignmentsDatabase: TeacherAssignmentItemDto[] = [];

function getEmptyPagedResult(filter: TeacherAssignmentFilterDto): PagedTeacherAssignmentsResultDto {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);
  let filtered = [...demoTeacherAssignmentsDatabase];

  if (filter.title && filter.title.trim() !== "") {
    const titleVal = filter.title.trim().toLowerCase();
    filtered = filtered.filter((a) => a.title.toLowerCase().includes(titleVal));
  }

  if (filter.className && filter.className.trim() !== "") {
    const classVal = filter.className.trim().toLowerCase();
    filtered = filtered.filter((a) => a.className.toLowerCase().includes(classVal));
  }

  if (filter.subjectName && filter.subjectName.trim() !== "") {
    const sbjVal = filter.subjectName.trim().toLowerCase();
    filtered = filtered.filter((a) => a.subjectName.toLowerCase().includes(sbjVal));
  }

  if (filter.subjectCode && filter.subjectCode.trim() !== "") {
    const codeVal = filter.subjectCode.trim().toLowerCase();
    filtered = filtered.filter((a) => a.subjectCode.toLowerCase().includes(codeVal));
  }

  if (filter.status && filter.status.trim() !== "") {
    filtered = filtered.filter((a) => a.status === filter.status);
  }

  if (filter.sortBy) {
    const isDesc = filter.sortOrder === "Desc";
    const key = filter.sortBy.toLowerCase();
    filtered.sort((a, b) => {
      let valA: string = "";
      let valB: string = "";
      if (key === "title") { valA = a.title || ""; valB = b.title || ""; }
      else if (key === "classname") { valA = a.className || ""; valB = b.className || ""; }
      else if (key === "subjectname") { valA = a.subjectName || ""; valB = b.subjectName || ""; }
      else if (key === "status") { valA = a.status || ""; valB = b.status || ""; }
      else if (key === "createdat") { valA = a.createdAt || ""; valB = b.createdAt || ""; }
      else { valA = a.dueDate || ""; valB = b.dueDate || ""; }

      if (valA < valB) return isDesc ? 1 : -1;
      if (valA > valB) return isDesc ? -1 : 1;
      return 0;
    });
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
    dataSource: "Server API",
    fetchedAt: new Date().toISOString(),
  };
}

export async function getTeacherAssignments(
  filter: TeacherAssignmentFilterDto
): Promise<PagedTeacherAssignmentsResultDto> {
  const query = new URLSearchParams();
  if (filter.title) query.set("title", filter.title);
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (filter.status) query.set("status", filter.status);
  if (filter.sortBy) query.set("sortBy", filter.sortBy);
  if (filter.sortOrder) query.set("sortOrder", filter.sortOrder);
  query.set("pageNumber", String(filter.pageNumber || 1));
  query.set("pageSize", String(filter.pageSize || 10));

  const path = `/Teacher/Assignments?${query.toString()}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 415 || response.status === 404) {
        return getEmptyPagedResult(filter);
      }
      const errMessage = await parseApiResponseError(response);
      throw new TeacherAssignmentsApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedTeacherAssignmentsResultDto>(response, {
      items: [],
      totalCount: 0,
      pageNumber: filter.pageNumber || 1,
      pageSize: filter.pageSize || 10,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    });
  } catch (err) {
    if (err instanceof TeacherAssignmentsApiError) throw err;
    return getEmptyPagedResult(filter);
  }
}

export async function createAssignment(
  dto: TeacherAssignmentCreateDto,
  meta?: { className?: string; subjectName?: string; subjectCode?: string }
): Promise<TeacherAssignmentItemDto> {
  const payload = {
    title: dto.title,
    description: dto.description,
    classId: dto.classId,
    subjectId: dto.subjectId,
    teacherId: dto.teacherId,
    deadline: dto.deadline || dto.dueDate || new Date().toISOString(),
    maxMarks: dto.maxMarks,
    status: typeof dto.status === "number" ? dto.status : (dto.status === "Active" || dto.status === "Published" ? 1 : 0),
    allowLateSubmission: dto.allowLateSubmission,
    allowResubmission: dto.allowResubmission,
  };

  const response = await authenticatedFetch("/Teacher/Assignments", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentsApiError(response.status, errMessage);
  }

  return await safeParseJson<TeacherAssignmentItemDto>(response, {
    id: "",
    title: dto.title,
    description: dto.description,
    className: meta?.className || "",
    classSection: "",
    academicYear: "",
    subjectName: meta?.subjectName || "",
    subjectCode: meta?.subjectCode || "",
    teacherName: "",
    teacherEmail: "",
    dueDate: dto.dueDate || dto.deadline || new Date().toISOString(),
    createdAt: new Date().toISOString(),
    maxMarks: dto.maxMarks,
    status: typeof dto.status === "number" ? (dto.status === 1 ? "Active" : "Draft") : dto.status,
    totalSubmissions: 0,
    allowLateSubmission: dto.allowLateSubmission,
  });
}

export async function updateAssignment(dto: TeacherAssignmentUpdateDto): Promise<TeacherAssignmentItemDto> {
  const payload = {
    title: dto.title,
    description: dto.description,
    deadline: dto.dueDate,
    maxMarks: dto.maxMarks,
    status: typeof dto.status === "number" ? dto.status : (dto.status === "Active" || dto.status === "Published" ? 1 : 0),
    allowLateSubmission: dto.allowLateSubmission,
    allowResubmission: dto.allowResubmission,
  };

  const response = await authenticatedFetch(`/Teacher/UpdateAssignment/Assignments/${dto.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentsApiError(response.status, errMessage);
  }

  return await safeParseJson<TeacherAssignmentItemDto>(response, {
    id: dto.id,
    title: dto.title || "",
    description: dto.description || "",
    className: "",
    classSection: "",
    academicYear: "",
    subjectName: "",
    subjectCode: "",
    teacherName: "",
    teacherEmail: "",
    dueDate: dto.dueDate || new Date().toISOString(),
    createdAt: new Date().toISOString(),
    maxMarks: dto.maxMarks || 100,
    status: typeof dto.status === "number" ? (dto.status === 1 ? "Active" : "Draft") : dto.status || "Active",
    totalSubmissions: 0,
    allowLateSubmission: dto.allowLateSubmission ?? true,
  });
}

export async function deleteAssignment(id: string): Promise<void> {
  const response = await authenticatedFetch(`/Teacher/Assignments/${id}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentsApiError(response.status, errMessage);
  }
}

export async function getTeacherSubmissions(
  filter: SubmissionFilterDto
): Promise<PagedSubmissionResultDto> {
  const query = new URLSearchParams();
  if (filter.assignmentTitle) query.set("assignmentTitle", filter.assignmentTitle);
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (filter.studentName) query.set("studentName", filter.studentName);
  if (filter.studentEmail) query.set("studentEmail", filter.studentEmail);
  if (filter.status) query.set("status", filter.status);
  if (filter.sortBy) query.set("sortBy", filter.sortBy);
  if (filter.sortOrder) query.set("sortOrder", filter.sortOrder);
  query.set("pageNumber", String(filter.pageNumber || 1));
  query.set("pageSize", String(filter.pageSize || 10));

  const path = `/Teacher/Submissions?${query.toString()}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 404 || response.status === 415) {
        return {
          items: [],
          totalCount: 0,
          pageNumber: filter.pageNumber || 1,
          pageSize: filter.pageSize || 10,
          totalPages: 1,
          hasPreviousPage: false,
          hasNextPage: false,
          dataSource: "Server API",
          fetchedAt: new Date().toISOString(),
        };
      }
      const errMessage = await parseApiResponseError(response);
      throw new TeacherAssignmentsApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedSubmissionResultDto>(response, {
      items: [],
      totalCount: 0,
      pageNumber: filter.pageNumber || 1,
      pageSize: filter.pageSize || 10,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    });
  } catch (err) {
    if (err instanceof TeacherAssignmentsApiError) throw err;
    return {
      items: [],
      totalCount: 0,
      pageNumber: filter.pageNumber || 1,
      pageSize: filter.pageSize || 10,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  }
}

export async function gradeTeacherSubmission(dto: GradeDto): Promise<void> {
  const response = await authenticatedFetch("/Teacher/GradeSubmission", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherAssignmentsApiError(response.status, errMessage);
  }
}
