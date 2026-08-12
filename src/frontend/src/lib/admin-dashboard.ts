export type DataSource = "api" | "demo";

export type UserRoleBreakdownItem = {
  role: "Admin" | "Teacher" | "Student";
  count: number;
};

export type AdminUsersDto = {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  newUsersThisMonth: number;
  roleBreakdown: UserRoleBreakdownItem[];
};

export type AssignmentStatusBreakdownItem = {
  status: "Draft" | "Published" | "Archived";
  count: number;
};

export type AdminAssignmentsDto = {
  totalAssignments: number;
  activeAssignments: number;
  draftAssignments: number;
  dueSoonAssignments: number;
  completionRate: number;
  statusBreakdown: AssignmentStatusBreakdownItem[];
};

export type WeeklyVolumeItem = {
  label: string;
  count: number;
};

export type AdminSubmissionsDto = {
  totalSubmissions: number;
  submittedToday: number;
  pendingReview: number;
  gradedSubmissions: number;
  weeklyVolumes: WeeklyVolumeItem[];
};

export type AdminDashboardSnapshot = {
  dataSource: DataSource;
  fetchedAt: string;
  users: AdminUsersDto;
  assignments: AdminAssignmentsDto;
  submissions: AdminSubmissionsDto;
};

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "");

const demoUsers: AdminUsersDto = {
  totalUsers: 248,
  activeUsers: 221,
  inactiveUsers: 27,
  newUsersThisMonth: 16,
  roleBreakdown: [
    { role: "Admin", count: 4 },
    { role: "Teacher", count: 31 },
    { role: "Student", count: 213 },
  ],
};

const demoAssignments: AdminAssignmentsDto = {
  totalAssignments: 86,
  activeAssignments: 37,
  draftAssignments: 11,
  dueSoonAssignments: 9,
  completionRate: 68,
  statusBreakdown: [
    { status: "Draft", count: 11 },
    { status: "Published", count: 52 },
    { status: "Archived", count: 23 },
  ],
};

const demoSubmissions: AdminSubmissionsDto = {
  totalSubmissions: 1482,
  submittedToday: 74,
  pendingReview: 38,
  gradedSubmissions: 1241,
  weeklyVolumes: [
    { label: "Mon", count: 52 },
    { label: "Tue", count: 68 },
    { label: "Wed", count: 61 },
    { label: "Thu", count: 84 },
    { label: "Fri", count: 93 },
    { label: "Sat", count: 47 },
    { label: "Sun", count: 55 },
  ],
};

async function requestJson<T>(path: string, fallback: T): Promise<T> {
  if (!apiBaseUrl) {
    return fallback;
  }

  try {
    const response = await fetch(`${apiBaseUrl}${path}`, {
      cache: "no-store",
    });

    if (!response.ok) {
      return fallback;
    }

    return (await response.json()) as T;
  } catch {
    return fallback;
  }
}

export async function getAdminDashboardSnapshot(): Promise<AdminDashboardSnapshot> {
  const [users, assignments, submissions] = await Promise.all([
    requestJson<AdminUsersDto>("/admin/dashboard/users", demoUsers),
    requestJson<AdminAssignmentsDto>("/admin/dashboard/assignments", demoAssignments),
    requestJson<AdminSubmissionsDto>("/admin/dashboard/submissions", demoSubmissions),
  ]);

  return {
    dataSource: apiBaseUrl ? "api" : "demo",
    fetchedAt: new Date().toISOString(),
    users,
    assignments,
    submissions,
  };
}