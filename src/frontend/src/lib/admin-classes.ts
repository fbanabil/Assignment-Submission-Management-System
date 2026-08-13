import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type ClassResponseDto = {
  id: string;
  name: string;
  section: string;
  academicYear: string;
  createdAt: string;
};

export type ClassCreateDto = {
  name: string;
  section: string;
  academicYear: string;
};

export type ClassUpdateDto = {
  id: string;
  name?: string;
  section?: string;
  academicYear?: string;
};

export type ClassFilterDto = {
  name?: string;
  section?: string;
  academicYear?: string;
  pageNumber: number;
  pageSize: number;
};

export type PagedClassResultDto = {
  items: ClassResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class ClassApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ClassApiError";
  }
}

export async function getClasses(filter: ClassFilterDto): Promise<PagedClassResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.name) query.set("name", filter.name);
  if (filter.section) query.set("section", filter.section);
  if (filter.academicYear) query.set("academicYear", filter.academicYear);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Admin/Classes?${query.toString()}`;

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
      throw new ClassApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedClassResultDto>(response, {
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
    if (err instanceof ClassApiError) throw err;
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

export async function createClass(dto: ClassCreateDto): Promise<ClassResponseDto> {
  const response = await authenticatedFetch("/Admin/Classes", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new ClassApiError(response.status, errMessage);
  }

  return await safeParseJson<ClassResponseDto>(response, {
    id: "",
    name: dto.name,
    section: dto.section,
    academicYear: dto.academicYear,
    createdAt: new Date().toISOString(),
  });
}

export async function updateClass(dto: ClassUpdateDto): Promise<ClassResponseDto> {
  const response = await authenticatedFetch(`/Admin/Classes/${dto.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new ClassApiError(response.status, errMessage);
  }

  return await safeParseJson<ClassResponseDto>(response, {
    id: dto.id,
    name: dto.name || "",
    section: dto.section || "",
    academicYear: dto.academicYear || "",
    createdAt: new Date().toISOString(),
  });
}

export async function deleteClass(id: string): Promise<void> {
  const response = await authenticatedFetch(`/Admin/DeleteClass/${id}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new ClassApiError(response.status, errMessage);
  }
}
