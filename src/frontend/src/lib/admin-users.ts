import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type UserRole = "Admin" | "Teacher" | "Student";

export type UserResponseDto = {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  rollNo?: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
};

export type UserCreateDto = {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  rollNo?: string;
  role: UserRole;
};

export type UserCreateResponseDto = {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
};

export type UserUpdateDto = {
  id: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  rollNo?: string;
  role?: UserRole;
  isActive?: boolean;
};

export type UserFilterDto = {
  name?: string;
  email?: string;
  phoneNumber?: string;
  rollNo?: string;
  role?: UserRole | "";
  isActive?: boolean | "" | "true" | "false";
  sortBy?: string;
  sortOrder?: "Asc" | "Desc";
  pageNumber: number;
  pageSize: number;
};

export type PagedUserResultDto = {
  items: UserResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class UserApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "UserApiError";
  }
}

let demoUsersDatabase: UserResponseDto[] = [];

export async function getUsers(filter: UserFilterDto): Promise<PagedUserResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.name) query.set("name", filter.name);
  if (filter.email) query.set("email", filter.email);
  if (filter.phoneNumber) query.set("phoneNumber", filter.phoneNumber);
  if (filter.rollNo) query.set("rollNo", filter.rollNo);
  if (filter.role) query.set("role", filter.role);
  if (filter.isActive !== undefined && filter.isActive !== "") {
    query.set("isActive", String(filter.isActive));
  }
  if (filter.sortBy) query.set("sortBy", filter.sortBy);
  if (filter.sortOrder) query.set("sortOrder", filter.sortOrder);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Admin/Users?${query.toString()}`;

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
      throw new UserApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedUserResultDto>(response, {
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
    if (err instanceof UserApiError) throw err;
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

export async function createUser(dto: UserCreateDto): Promise<UserCreateResponseDto> {
  const response = await authenticatedFetch("/admin/users", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new UserApiError(response.status, errMessage);
  }

  return await safeParseJson<UserCreateResponseDto>(response, {
    id: "",
    fullName: dto.fullName,
    email: dto.email,
    role: dto.role,
  });
}

export async function updateUser(dto: UserUpdateDto): Promise<UserResponseDto> {
  const response = await authenticatedFetch(`/Admin/Users/${dto.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new UserApiError(response.status, errMessage);
  }

  return await safeParseJson<UserResponseDto>(response, {
    id: dto.id,
    fullName: dto.fullName || "",
    email: dto.email || "",
    phoneNumber: dto.phoneNumber || "",
    role: dto.role || "Student",
    isActive: dto.isActive ?? true,
    createdAt: new Date().toISOString(),
  });
}

export async function toggleUserStatus(id: string, isActive: boolean): Promise<UserResponseDto> {
  return updateUser({ id, isActive });
}
