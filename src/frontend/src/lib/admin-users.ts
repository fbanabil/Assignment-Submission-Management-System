export type AdminUserRecord = {
  id?: string | number;
  name?: string;
  email?: string;
  role?: string;
  status?: string;
  createdAt?: string;
  lastLoginAt?: string;
  [key: string]: unknown;
};

export type AdminUsersSnapshot = {
  dataSource: string;
  fetchedAt: string;
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  users: AdminUserRecord[];
  raw: unknown;
};

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");

const demoUsersSnapshot: AdminUsersSnapshot = {
  dataSource: "demo",
  fetchedAt: new Date().toISOString(),
  totalUsers: 3,
  activeUsers: 2,
  inactiveUsers: 1,
  users: [
    {
      id: 101,
      name: "Ava Johnson",
      email: "ava.johnson@example.com",
      role: "Admin",
      status: "Active",
      createdAt: "2026-07-01T10:15:00.000Z",
      lastLoginAt: "2026-08-12T08:45:00.000Z",
    },
    {
      id: 102,
      name: "Noah Kim",
      email: "noah.kim@example.com",
      role: "Teacher",
      status: "Active",
      createdAt: "2026-07-08T11:30:00.000Z",
      lastLoginAt: "2026-08-11T16:05:00.000Z",
    },
    {
      id: 103,
      name: "Mia Patel",
      email: "mia.patel@example.com",
      role: "Student",
      status: "Invited",
      createdAt: "2026-07-15T09:20:00.000Z",
      lastLoginAt: "2026-08-11T16:05:00.000Z",
    },
  ],
  raw: null,
};

class AdminUsersRequestError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "AdminUsersRequestError";
  }
}

function toNumber(value: unknown): number | null {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string" && value.trim() !== "") {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  return null;
}

function extractUsers(payload: unknown): AdminUserRecord[] {
  if (Array.isArray(payload)) {
    return payload as AdminUserRecord[];
  }

  if (!payload || typeof payload !== "object") {
    return [];
  }

  const record = payload as Record<string, unknown>;
  const candidates = [record.users, record.items, record.data, record.results, record.value];

  for (const candidate of candidates) {
    if (Array.isArray(candidate)) {
      return candidate as AdminUserRecord[];
    }
  }

  return [];
}

function countActiveUsers(users: AdminUserRecord[]) {
  return users.reduce((count, user) => {
    const status = `${user.status ?? ""}`.toLowerCase();
    return status.includes("active") || status.includes("enabled") ? count + 1 : count;
  }, 0);
}

async function requestJson<T>(path: string, fallback: T): Promise<T> {
  if (!apiBaseUrl) {
    return fallback;
  }

  try {
    const response = await fetch(`${apiBaseUrl}${path}`, {
      cache: "no-store",
    });

    if (!response.ok) {
      const contentType = response.headers.get("content-type") ?? "";
      let serverMessage = response.statusText;

      if (contentType.includes("application/json")) {
        const payload = (await response.json()) as { message?: string; error?: string; title?: string };
        serverMessage = payload.message ?? payload.error ?? payload.title ?? serverMessage;
      } else {
        const body = await response.text();
        if (body.trim()) {
          serverMessage = body.trim();
        }
      }

      throw new AdminUsersRequestError(response.status, `${response.status} ${serverMessage}`.trim());
    }

    return (await response.json()) as T;
  } catch (error) {
    if (error instanceof AdminUsersRequestError) {
      throw error;
    }

    throw new Error(`Unable to fetch ${path}`);
  }
}

export async function getAdminUsersSnapshot(): Promise<AdminUsersSnapshot> {
  if (!apiBaseUrl) {
    return demoUsersSnapshot;
  }

  const data = await requestJson<unknown>("/api/admin/users", demoUsersSnapshot.raw ?? demoUsersSnapshot);
  const users = extractUsers(data);
  const record = data && typeof data === "object" ? (data as Record<string, unknown>) : {};
  const totalUsers = toNumber(record.totalUsers) ?? (users.length || demoUsersSnapshot.totalUsers);
  const activeUsers = toNumber(record.activeUsers) ?? (users.length ? countActiveUsers(users) : demoUsersSnapshot.activeUsers);
  const inactiveUsers = toNumber(record.inactiveUsers) ?? Math.max(totalUsers - activeUsers, 0);

  return {
    dataSource: typeof record.dataSource === "string" ? record.dataSource : demoUsersSnapshot.dataSource,
    fetchedAt: typeof record.fetchedAt === "string" ? record.fetchedAt : new Date().toISOString(),
    totalUsers,
    activeUsers,
    inactiveUsers,
    users: users.length > 0 ? users : demoUsersSnapshot.users,
    raw: data,
  };
}