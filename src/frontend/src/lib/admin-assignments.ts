import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type AssignmentResponseDto = {
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
  status: "Active" | "Past Due" | "Draft" | "Published";
  totalSubmissions: number;
  allowLateSubmission: boolean;
};

export type AssignmentFilterDto = {
  title?: string;
  className?: string;
  subjectName?: string;
  subjectCode?: string;
  teacherName?: string;
  teacherEmail?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: "Asc" | "Desc";
  pageNumber: number;
  pageSize: number;
};

export type PagedAssignmentResultDto = {
  items: AssignmentResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class AssignmentApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "AssignmentApiError";
  }
}

export async function getAssignments(
  filter: AssignmentFilterDto
): Promise<PagedAssignmentResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.title) query.set("title", filter.title);
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (filter.teacherName) query.set("teacherName", filter.teacherName);
  if (filter.teacherEmail) query.set("teacherEmail", filter.teacherEmail);
  if (filter.status) query.set("status", filter.status);
  if (filter.sortBy) query.set("sortBy", filter.sortBy);
  if (filter.sortOrder) query.set("sortOrder", filter.sortOrder);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Admin/Assignments?${query.toString()}`;

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
      throw new AssignmentApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedAssignmentResultDto>(response, {
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
    if (err instanceof AssignmentApiError) throw err;
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
