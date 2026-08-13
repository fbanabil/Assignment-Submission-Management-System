import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

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

const emptyDashboardData: DashboardApiResponse = {
  dataSource: "Server API",
  fetchedAt: new Date().toISOString(),
  users: {
    totalUsers: 0,
    activeUsers: 0,
    inactiveUsers: 0,
    newUsersThisMonth: 0,
    roleBreakdown: [],
  },
  assignments: {
    totalAssignments: 0,
    activeAssignments: 0,
    draftAssignments: 0,
    dueSoonAssignments: 0,
    completionRate: 0,
    statusBreakdown: [],
  },
  submissions: {
    totalSubmissions: 0,
    submittedToday: 0,
    pendingReview: 0,
    gradedSubmissions: 0,
    weeklyVolumes: [],
  },
};

export class DashboardRequestError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "DashboardRequestError";
  }
}

export async function getDashboardSummarySnapshot(): Promise<DashboardSummaryDto> {
  try {
    const response = await authenticatedFetch("/Admin/Dashboard/summary", { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 404 || response.status === 415) {
        return { ...emptyDashboardData, fetchedAt: new Date().toISOString() };
      }
      const errMessage = await parseApiResponseError(response);
      throw new DashboardRequestError(response.status, errMessage);
    }

    const data = await safeParseJson<DashboardApiResponse>(response, emptyDashboardData);
    return {
      dataSource: data.dataSource,
      fetchedAt: data.fetchedAt,
      users: data.users,
      assignments: data.assignments,
      submissions: data.submissions,
    };
  } catch (error) {
    if (error instanceof DashboardRequestError) {
      throw error;
    }
    return { ...emptyDashboardData, fetchedAt: new Date().toISOString() };
  }
}