import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";
import { type TeacherAssignedClassSubjectDto } from "./teacher-dashboard";

export type TeacherClassFilterDto = {
  className?: string;
  classSection?: string;
  academicYear?: string;
  subjectName?: string;
  subjectCode?: string;
  pageNumber: number;
  pageSize: number;
};

export type PagedTeacherClassResultDto = {
  items: TeacherAssignedClassSubjectDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class TeacherClassesApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "TeacherClassesApiError";
  }
}

export async function getTeacherClasses(
  filter: TeacherClassFilterDto,
  teacherId?: string
): Promise<PagedTeacherClassResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.className) query.set("className", filter.className);
  if (filter.classSection) query.set("classSection", filter.classSection);
  if (filter.academicYear) query.set("academicYear", filter.academicYear);
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (teacherId) query.set("teacherId", teacherId);

  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Teacher/Classes?${query.toString()}`;

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
      throw new TeacherClassesApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedTeacherClassResultDto>(response, {
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
    if (err instanceof TeacherClassesApiError) throw err;
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
