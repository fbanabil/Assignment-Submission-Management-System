import { getApiUrl, parseApiResponseError, safeParseJson } from "./api-error";

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

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");

export class ClassApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ClassApiError";
  }
}

// In-memory fallback dataset for frontend demo preview
let demoClassesDatabase: ClassResponseDto[] = [
  {
    id: "cls-001",
    name: "Grade 10 - Mathematics",
    section: "Section A",
    academicYear: "2024-2025",
    createdAt: "2024-01-10T08:00:00Z",
  },
  {
    id: "cls-002",
    name: "Grade 10 - Science",
    section: "Section B",
    academicYear: "2024-2025",
    createdAt: "2024-01-12T09:30:00Z",
  },
  {
    id: "cls-003",
    name: "Grade 11 - Computer Science",
    section: "Section A",
    academicYear: "2024-2025",
    createdAt: "2024-01-15T11:00:00Z",
  },
  {
    id: "cls-004",
    name: "Grade 11 - Physics",
    section: "Section C",
    academicYear: "2024-2025",
    createdAt: "2024-01-20T14:15:00Z",
  },
  {
    id: "cls-005",
    name: "Grade 12 - Advanced English",
    section: "Section A",
    academicYear: "2024-2025",
    createdAt: "2024-02-01T10:45:00Z",
  },
  {
    id: "cls-006",
    name: "Grade 12 - Chemistry",
    section: "Section B",
    academicYear: "2024-2025",
    createdAt: "2024-02-05T13:20:00Z",
  },
  {
    id: "cls-007",
    name: "Grade 9 - General History",
    section: "Section A",
    academicYear: "2024-2025",
    createdAt: "2024-02-14T09:00:00Z",
  },
  {
    id: "cls-008",
    name: "Grade 9 - Biology",
    section: "Section D",
    academicYear: "2024-2025",
    createdAt: "2024-02-20T15:30:00Z",
  },
];

/**
 * Retrieves paginated classes with filtering options (Name, Section, AcademicYear)
 */
export async function getClasses(filter: ClassFilterDto): Promise<PagedClassResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  if (!apiBaseUrl) {
    let filtered = [...demoClassesDatabase];

    if (filter.name && filter.name.trim() !== "") {
      const nameVal = filter.name.trim().toLowerCase();
      filtered = filtered.filter((c) => c.name.toLowerCase().includes(nameVal));
    }

    if (filter.section && filter.section.trim() !== "") {
      const secVal = filter.section.trim().toLowerCase();
      filtered = filtered.filter((c) => c.section.toLowerCase().includes(secVal));
    }

    if (filter.academicYear && filter.academicYear.trim() !== "") {
      const yearVal = filter.academicYear.trim().toLowerCase();
      filtered = filtered.filter((c) => c.academicYear.toLowerCase().includes(yearVal));
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
  if (filter.section) query.set("section", filter.section);
  if (filter.academicYear) query.set("academicYear", filter.academicYear);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const url = getApiUrl(`/Admin/Classes?${query.toString()}`);

  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
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
    throw new Error(`Failed to fetch class list: ${err instanceof Error ? err.message : String(err)}`);
  }
}

/**
 * Creates a new class section
 */
export async function createClass(dto: ClassCreateDto): Promise<ClassResponseDto> {
  if (!apiBaseUrl) {
    const newId = `cls-${String(demoClassesDatabase.length + 1).padStart(3, "0")}`;
    const newClass: ClassResponseDto = {
      id: newId,
      name: dto.name,
      section: dto.section,
      academicYear: dto.academicYear,
      createdAt: new Date().toISOString(),
    };
    demoClassesDatabase = [newClass, ...demoClassesDatabase];
    return newClass;
  }

  const url = getApiUrl("/Admin/Classes");
  const response = await fetch(url, {
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

/**
 * Updates an existing class section
 */
export async function updateClass(dto: ClassUpdateDto): Promise<ClassResponseDto> {
  if (!apiBaseUrl) {
    const index = demoClassesDatabase.findIndex((c) => c.id === dto.id);
    if (index === -1) {
      throw new Error(`Class with ID ${dto.id} not found.`);
    }

    const existing = demoClassesDatabase[index];
    const updated: ClassResponseDto = {
      ...existing,
      name: dto.name ?? existing.name,
      section: dto.section ?? existing.section,
      academicYear: dto.academicYear ?? existing.academicYear,
    };
    demoClassesDatabase[index] = updated;
    return updated;
  }

  const url = getApiUrl(`/Admin/Classes/${dto.id}`);
  const response = await fetch(url, {
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

/**
 * Deletes a class section
 */
export async function deleteClass(id: string): Promise<void> {
  if (!apiBaseUrl) {
    demoClassesDatabase = demoClassesDatabase.filter((c) => c.id !== id);
    return;
  }

  const url = getApiUrl(`/Admin/Classes/${id}`);
  const response = await fetch(url, {
    method: "DELETE",
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new ClassApiError(response.status, errMessage);
  }
}
