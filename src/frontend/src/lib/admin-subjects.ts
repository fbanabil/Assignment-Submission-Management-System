import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";
import { type ClassResponseDto } from "./admin-classes";

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
  sortBy?: string;
  sortOrder?: "Asc" | "Desc";
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

export class SubjectApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "SubjectApiError";
  }
}

export async function getSubjects(filter: SubjectFilterDto): Promise<PagedSubjectResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.name) query.set("name", filter.name);
  if (filter.code) query.set("code", filter.code);
  if (filter.classId) query.set("classId", filter.classId);
  if (filter.sortBy) query.set("sortBy", filter.sortBy);
  if (filter.sortOrder) query.set("sortOrder", filter.sortOrder);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Admin/Subjects?${query.toString()}`;

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

export async function createSubject(dto: SubjectCreateDto): Promise<SubjectResponseDto> {
  const response = await authenticatedFetch("/Admin/Subjects", {
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

export async function updateSubject(dto: SubjectUpdateDto): Promise<SubjectResponseDto> {
  const response = await authenticatedFetch(`/Admin/Subjects/${dto.id}`, {
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

export async function deleteSubject(id: string): Promise<void> {
  const response = await authenticatedFetch(`/Admin/DeleteSubject/${id}`, { method: "DELETE" });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }
}

export async function linkSubjectToClass(dto: ClassSubjectCreateDto, selectedClassData?: ClassResponseDto): Promise<ClassSummaryDto> {
  const response = await authenticatedFetch("/Admin/ClassSubjects", {
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

export async function unlinkSubjectFromClass(classId: string, subjectId: string): Promise<void> {
  const queryParams = `classId=${encodeURIComponent(classId)}&subjectId=${encodeURIComponent(subjectId)}`;

  const response = await authenticatedFetch(`/Admin/DeleteClassSubject/ClassSubjects?${queryParams}`, { method: "DELETE" });

  if (!response.ok) {
    if (response.status === 404 || response.status === 405) {
      return;
    }
    const errMessage = await parseApiResponseError(response);
    throw new SubjectApiError(response.status, errMessage);
  }
}
