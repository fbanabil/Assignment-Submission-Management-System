import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type StudentAssignmentDueDto = {
  assignmentId: string;
  title: string;
  subjectName: string;
  subjectCode: string;
  className: string;
  dueDate: string;
  maxMarks: number;
  status: "Pending" | "Overdue" | string;
};

export type StudentRecentGradeDto = {
  submissionId: string;
  assignmentTitle: string;
  subjectName: string;
  subjectCode: string;
  submittedAt: string;
  gradedAt?: string;
  grade?: number;
  maxMarks: number;
  feedback: string;
  gradedByTeacherName: string;
};

export type StudentDashboardResponseDto = {
  studentName: string;
  studentEmail: string;
  enrolledClassesCount: number;
  pendingAssignmentsCount: number;
  completedAssignmentsCount: number;
  averageGrade: number;
  assignmentsDueSoon: StudentAssignmentDueDto[];
  recentGradesFeedback: StudentRecentGradeDto[];
  dataSource?: string;
  fetchedAt?: string;
};

export class StudentDashboardApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "StudentDashboardApiError";
  }
}

const emptyStudentDashboard: StudentDashboardResponseDto = {
  studentName: "Student User",
  studentEmail: "",
  enrolledClassesCount: 0,
  pendingAssignmentsCount: 0,
  completedAssignmentsCount: 0,
  averageGrade: 0,
  assignmentsDueSoon: [],
  recentGradesFeedback: [],
  dataSource: "Server API",
  fetchedAt: new Date().toISOString(),
};

/**
 * Retrieves student dashboard metrics, assignments due soon, and recent grades & feedback.
 */
export async function getStudentDashboard(studentId?: string): Promise<StudentDashboardResponseDto> {
  const query = studentId ? `?studentId=${encodeURIComponent(studentId)}` : "";
  const path = `/Student/Dashboard${query}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 415 || response.status === 404) {
        return { ...emptyStudentDashboard, fetchedAt: new Date().toISOString() };
      }
      const errMessage = await parseApiResponseError(response);
      throw new StudentDashboardApiError(response.status, errMessage);
    }
    const data = await safeParseJson<StudentDashboardResponseDto>(response, emptyStudentDashboard);
    return {
      ...data,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  } catch (err) {
    if (err instanceof StudentDashboardApiError) throw err;
    return { ...emptyStudentDashboard, fetchedAt: new Date().toISOString() };
  }
}
