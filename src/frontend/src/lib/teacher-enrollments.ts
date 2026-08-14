import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type StudentEnrollmentResponseDto = {
  id: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  studentRollNo?: string;
  classId: string;
  className: string;
  classSection: string;
  academicYear: string;
  enrolledAt: string;
};

export type StudentEnrollmentCreateDto = {
  studentEmail: string;
  classId: string;
  studentId?: string;
};

export type StudentEnrollmentFilterDto = {
  classId?: string;
  studentId?: string;
  studentName?: string;
  className?: string;
  sortBy?: string;
  sortOrder?: "Asc" | "Desc";
  pageNumber: number;
  pageSize: number;
};

export type PagedStudentEnrollmentResultDto = {
  items: StudentEnrollmentResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class TeacherEnrollmentsApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "TeacherEnrollmentsApiError";
  }
}

/**
 * Retrieves student enrollments for classes taught by the teacher
 */
export async function getTeacherEnrollments(
  filter: StudentEnrollmentFilterDto
): Promise<PagedStudentEnrollmentResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.classId) query.set("classId", filter.classId);
  if (filter.studentId) query.set("studentId", filter.studentId);
  if (filter.studentName) query.set("studentName", filter.studentName);
  if (filter.className) query.set("className", filter.className);
  if (filter.sortBy) query.set("sortBy", filter.sortBy);
  if (filter.sortOrder) query.set("sortOrder", filter.sortOrder);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Teacher/Enrollments?${query.toString()}`;

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
      throw new TeacherEnrollmentsApiError(response.status, errMessage);
    }

    const data = await safeParseJson<PagedStudentEnrollmentResultDto>(response, {
      items: [],
      totalCount: 0,
      pageNumber,
      pageSize,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    });

    return {
      ...data,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  } catch (err) {
    if (err instanceof TeacherEnrollmentsApiError) throw err;
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

/**
 * Enrolls a student into a class
 */
export async function enrollStudent(dto: StudentEnrollmentCreateDto): Promise<StudentEnrollmentResponseDto> {
  const response = await authenticatedFetch("/Teacher/Enrollments", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherEnrollmentsApiError(response.status, errMessage);
  }

  return await safeParseJson<StudentEnrollmentResponseDto>(response, {
    id: "",
    studentId: dto.studentId || "",
    studentName: "",
    studentEmail: dto.studentEmail,
    classId: dto.classId,
    className: "",
    classSection: "",
    academicYear: "",
    enrolledAt: new Date().toISOString(),
  });
}

/**
 * Removes/disenrolls a student from a class
 */
export async function removeEnrollment(id: string): Promise<void> {
  const response = await authenticatedFetch(`/Teacher/Enrollments/${id}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new TeacherEnrollmentsApiError(response.status, errMessage);
  }
}
