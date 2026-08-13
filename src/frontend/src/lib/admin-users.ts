import { getApiUrl, parseApiResponseError, safeParseJson } from "./api-error";

export type UserRole = "Admin" | "Teacher" | "Student";

export type UserResponseDto = {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
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
  role?: UserRole;
  isActive?: boolean;
};

export type UserFilterDto = {
  name?: string;
  email?: string;
  phoneNumber?: string;
  role?: UserRole | "";
  isActive?: boolean | "" | "true" | "false";
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

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");

// In-memory initial demo datasets for frontend fallback preview
let demoUsersDatabase: UserResponseDto[] = [
  {
    id: "usr-001",
    fullName: "Eleanor Vance",
    email: "eleanor.vance@school.edu",
    phoneNumber: "+15550192834",
    role: "Admin",
    isActive: true,
    createdAt: "2024-01-15T09:30:00Z",
  },
  {
    id: "usr-002",
    fullName: "Marcus Sterling",
    email: "marcus.sterling@school.edu",
    phoneNumber: "+15550183746",
    role: "Teacher",
    isActive: true,
    createdAt: "2024-02-01T11:20:00Z",
  },
  {
    id: "usr-003",
    fullName: "Sophia Rodriguez",
    email: "sophia.rodriguez@school.edu",
    phoneNumber: "+15550174635",
    role: "Teacher",
    isActive: true,
    createdAt: "2024-02-10T14:15:00Z",
  },
  {
    id: "usr-004",
    fullName: "Alexander Wright",
    email: "alexander.wright@student.edu",
    phoneNumber: "+15550165524",
    role: "Student",
    isActive: true,
    createdAt: "2024-03-05T08:45:00Z",
  },
  {
    id: "usr-005",
    fullName: "Chloe Bennett",
    email: "chloe.bennett@student.edu",
    phoneNumber: "+15550156413",
    role: "Student",
    isActive: false,
    createdAt: "2024-03-12T16:00:00Z",
  },
  {
    id: "usr-006",
    fullName: "David Chen",
    email: "david.chen@school.edu",
    phoneNumber: "+15550147302",
    role: "Teacher",
    isActive: true,
    createdAt: "2024-03-18T10:10:00Z",
  },
  {
    id: "usr-007",
    fullName: "Emma Watson",
    email: "emma.watson@student.edu",
    phoneNumber: "+15550138291",
    role: "Student",
    isActive: true,
    createdAt: "2024-04-02T13:25:00Z",
  },
  {
    id: "usr-008",
    fullName: "Franklin Pierce",
    email: "franklin.pierce@student.edu",
    phoneNumber: "+15550129180",
    role: "Student",
    isActive: true,
    createdAt: "2024-04-14T09:00:00Z",
  },
  {
    id: "usr-009",
    fullName: "Grace Hopper",
    email: "grace.hopper@school.edu",
    phoneNumber: "+15550110079",
    role: "Admin",
    isActive: true,
    createdAt: "2024-04-20T15:40:00Z",
  },
  {
    id: "usr-010",
    fullName: "Henry Cavendish",
    email: "henry.cavendish@student.edu",
    phoneNumber: "+15550101968",
    role: "Student",
    isActive: false,
    createdAt: "2024-05-01T12:30:00Z",
  },
  {
    id: "usr-011",
    fullName: "Isabella Martinez",
    email: "isabella.martinez@student.edu",
    phoneNumber: "+15550092857",
    role: "Student",
    isActive: true,
    createdAt: "2024-05-09T17:00:00Z",
  },
  {
    id: "usr-012",
    fullName: "James Maxwell",
    email: "james.maxwell@school.edu",
    phoneNumber: "+15550083746",
    role: "Teacher",
    isActive: true,
    createdAt: "2024-05-15T08:15:00Z",
  },
];

export class UserApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "UserApiError";
  }
}

/**
 * Filter users endpoint call. Accepts default or modified filters (Name, Email, PhoneNumber, Role, IsActive) and page params.
 */
export async function getUsers(filter: UserFilterDto): Promise<PagedUserResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  if (!apiBaseUrl) {
    // Demo fallback client-side pagination & filtering
    let filtered = [...demoUsersDatabase];

    if (filter.name && filter.name.trim() !== "") {
      const nameVal = filter.name.trim().toLowerCase();
      filtered = filtered.filter((u) => u.fullName.toLowerCase().includes(nameVal));
    }

    if (filter.email && filter.email.trim() !== "") {
      const emailVal = filter.email.trim().toLowerCase();
      filtered = filtered.filter((u) => u.email.toLowerCase().includes(emailVal));
    }

    if (filter.phoneNumber && filter.phoneNumber.trim() !== "") {
      const phoneVal = filter.phoneNumber.trim().toLowerCase();
      filtered = filtered.filter((u) => u.phoneNumber.toLowerCase().includes(phoneVal));
    }

    if (filter.role) {
      filtered = filtered.filter((u) => u.role === filter.role);
    }

    if (filter.isActive !== undefined && filter.isActive !== "") {
      const activeBool =
        typeof filter.isActive === "boolean"
          ? filter.isActive
          : filter.isActive === "true";
      filtered = filtered.filter((u) => u.isActive === activeBool);
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
  if (filter.email) query.set("email", filter.email);
  if (filter.phoneNumber) query.set("phoneNumber", filter.phoneNumber);
  if (filter.role) query.set("role", filter.role);
  if (filter.isActive !== undefined && filter.isActive !== "") {
    query.set("isActive", String(filter.isActive));
  }
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const url = getApiUrl(`/Admin/Users?${query.toString()}`);

  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
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
    throw new Error(`Failed to fetch user list: ${err instanceof Error ? err.message : String(err)}`);
  }
}

/**
 * Creates a user sending UserCreateDto to backend.
 * Backend responds with 201 Created: { id, fullName, email, role }
 */
export async function createUser(dto: UserCreateDto): Promise<UserCreateResponseDto> {
  if (!apiBaseUrl) {
    // Demo fallback creation logic
    const newId = `usr-${String(demoUsersDatabase.length + 1).padStart(3, "0")}`;
    const newUser: UserResponseDto = {
      id: newId,
      fullName: dto.fullName,
      email: dto.email,
      phoneNumber: dto.phoneNumber,
      role: dto.role,
      isActive: true,
      createdAt: new Date().toISOString(),
    };
    demoUsersDatabase = [newUser, ...demoUsersDatabase];

    return {
      id: newUser.id,
      fullName: newUser.fullName,
      email: newUser.email,
      role: newUser.role,
    };
  }

  const url = getApiUrl("/Admin/Users");
  const response = await fetch(url, {
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

/**
 * Updates a user sending UserUpdateDto to backend.
 */
export async function updateUser(dto: UserUpdateDto): Promise<UserResponseDto> {
  if (!apiBaseUrl) {
    // Demo fallback update logic
    const index = demoUsersDatabase.findIndex((u) => u.id === dto.id);
    if (index === -1) {
      throw new Error(`User with ID ${dto.id} not found.`);
    }

    const existing = demoUsersDatabase[index];
    const updated: UserResponseDto = {
      ...existing,
      fullName: dto.fullName ?? existing.fullName,
      email: dto.email ?? existing.email,
      phoneNumber: dto.phoneNumber ?? existing.phoneNumber,
      role: dto.role ?? existing.role,
      isActive: dto.isActive !== undefined ? dto.isActive : existing.isActive,
    };

    demoUsersDatabase[index] = updated;
    return updated;
  }

  const url = getApiUrl(`/Admin/Users/${dto.id}`);
  const response = await fetch(url, {
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

/**
 * Convenience method to toggle active/deactive status.
 */
export async function toggleUserStatus(id: string, isActive: boolean): Promise<UserResponseDto> {
  return updateUser({ id, isActive });
}
