import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type TeacherAssignedClassSubjectDto = {
  classId?: string;
  subjectId?: string;
  classSubjectId: string;
  className: string;
  classSection: string;
  academicYear: string;
  subjectName: string;
  subjectCode: string;
  studentCount: number;
};

export type TeacherUpcomingDeadlineDto = {
  assignmentId: string;
  title: string;
  className: string;
  subjectName: string;
  subjectCode: string;
  dueDate: string;
  totalSubmissions: number;
  ungradedSubmissions: number;
};

export type TeacherDashboardResponseDto = {
  teacherName: string;
  teacherEmail: string;
  totalAssignedClasses: number;
  activeAssignmentsCount: number;
  ungradedSubmissionsCount: number;
  upcomingDeadlinesCount: number;
  assignedClasses: TeacherAssignedClassSubjectDto[];
  upcomingDeadlines: TeacherUpcomingDeadlineDto[];
  dataSource?: string;
  fetchedAt?: string;
};

export class TeacherDashboardApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "TeacherDashboardApiError";
  }
}

const emptyTeacherDashboard: TeacherDashboardResponseDto = {
  teacherName: "",
  teacherEmail: "",
  totalAssignedClasses: 0,
  activeAssignmentsCount: 0,
  ungradedSubmissionsCount: 0,
  upcomingDeadlinesCount: 0,
  assignedClasses: [],
  upcomingDeadlines: [],
  dataSource: "Server API",
  fetchedAt: new Date().toISOString(),
};

/**
 * Retrieves teacher dashboard metrics, assigned classes, upcoming deadlines, and ungraded count
 */
export async function getTeacherDashboard(teacherId?: string): Promise<TeacherDashboardResponseDto> {
  const query = teacherId ? `?teacherId=${encodeURIComponent(teacherId)}` : "";
  const path = `/Teacher/Dashboard${query}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 415 || response.status === 404) {
        return { ...emptyTeacherDashboard, fetchedAt: new Date().toISOString() };
      }
      const errMessage = await parseApiResponseError(response);
      throw new TeacherDashboardApiError(response.status, errMessage);
    }
    const data = await safeParseJson<TeacherDashboardResponseDto>(response, emptyTeacherDashboard);
    return {
      ...data,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  } catch (err) {
    if (err instanceof TeacherDashboardApiError) throw err;
    return { ...emptyTeacherDashboard, fetchedAt: new Date().toISOString() };
  }
}
