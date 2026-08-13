import { getApiUrl, parseApiResponseError } from "./api-error";

export type DataSource = string;

export type UserRoleSummaryDto = {
  role: "Admin" | "Teacher" | "Student";
  count: number;
};

export type UserSummaryDto = {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  newUsersThisMonth: number;
  roleBreakdown: UserRoleSummaryDto[];
};

export type AssignmentStatusSummaryDto = {
  status: "Draft" | "Published" | "Archived";
  count: number;
};

export type AssignmentSummaryDto = {
  totalAssignments: number;
  activeAssignments: number;
  draftAssignments: number;
  dueSoonAssignments: number;
  completionRate: number;
  statusBreakdown: AssignmentStatusSummaryDto[];
};

export type SubmissionVolumeDto = {
  label: string;
  count: number;
};

export type SubmissionSummaryDto = {
  totalSubmissions: number;
  submittedToday: number;
  pendingReview: number;
  gradedSubmissions: number;
  weeklyVolumes: SubmissionVolumeDto[];
};

export type DashboardSummaryDto = {
  dataSource: DataSource;
  fetchedAt: string;
  users: UserSummaryDto;
  assignments: AssignmentSummaryDto;
  submissions: SubmissionSummaryDto;
};

export type DashboardApiResponse = {
  dataSource: string;
  fetchedAt: string;
  users: UserSummaryDto;
  assignments: AssignmentSummaryDto;
  submissions: SubmissionSummaryDto;
};

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");

const demoUsers: UserSummaryDto = {
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

const demoAssignments: AssignmentSummaryDto = {
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

const demoSubmissions: SubmissionSummaryDto = {
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

const demoDashboardData: DashboardApiResponse = {
  dataSource: "demo",
  fetchedAt: new Date().toISOString(),
  users: demoUsers,
  assignments: demoAssignments,
  submissions: demoSubmissions,
};

class DashboardRequestError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "DashboardRequestError";
  }
}

async function requestJson<T>(path: string, fallback: T): Promise<T> {
  if (!apiBaseUrl) {
    return fallback;
  }

  const url = getApiUrl(path);

  try {
    const response = await fetch(url, {
      cache: "no-store",
    });

    if (!response.ok) {
      const errMessage = await parseApiResponseError(response);
      throw new DashboardRequestError(response.status, errMessage);
    }

    return (await response.json()) as T;
  } catch (error) {
    if (error instanceof DashboardRequestError) {
      throw error;
    }

    throw new Error(`Unable to fetch ${path}: ${error instanceof Error ? error.message : String(error)}`);
  }
}

export async function getDashboardSummarySnapshot(): Promise<DashboardSummaryDto> {
  if (!apiBaseUrl) {
    return demoDashboardData;
  }

  const data = await requestJson<DashboardApiResponse>("/Admin/Dashboard/summary", demoDashboardData);

  return {
    dataSource: data.dataSource,
    fetchedAt: data.fetchedAt,
    users: data.users,
    assignments: data.assignments,
    submissions: data.submissions,
  };
}