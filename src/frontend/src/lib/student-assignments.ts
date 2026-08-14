import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

const rawBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ??
  "http://localhost:5000";

export const serverOriginUrl = rawBaseUrl.replace(/\/api$/i, "");

export function resolveServerFileUrl(fileUrl?: string | null): string {
  if (!fileUrl) return "#";
  if (fileUrl.startsWith("http://") || fileUrl.startsWith("https://") || fileUrl.startsWith("data:")) {
    return fileUrl;
  }

  let clean = fileUrl.replace(/\\/g, "/");
  if (clean.toLowerCase().startsWith("wwwroot/")) {
    clean = clean.substring(8);
  }
  if (!clean.startsWith("/")) {
    clean = `/${clean}`;
  }

  return `${serverOriginUrl}${clean}`;
}

export async function downloadFileFromServer(fileUrl: string, defaultFileName?: string): Promise<void> {
  const fullUrl = resolveServerFileUrl(fileUrl);
  if (!fullUrl || fullUrl === "#") return;

  try {
    const response = await authenticatedFetch(fullUrl, { cache: "no-store" });
    if (!response.ok) {
      throw new Error(`Failed to download file: status ${response.status}`);
    }

    const blob = await response.blob();
    const blobUrl = window.URL.createObjectURL(blob);
    const fileName = defaultFileName || fileUrl.split("/").pop() || "assignment_attachment";

    const link = document.createElement("a");
    link.href = blobUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(blobUrl);
  } catch (err) {
    console.error("Download blob failed, redirecting to direct server URL:", err);
    window.open(fullUrl, "_blank");
  }
}

export type StudentAssignmentFilterDto = {
  statusFilter?: "All" | "Pending" | "Submitted" | "Graded";
  search?: string;
  pageNumber: number;
  pageSize: number;
};

export type StudentAssignmentResponseDto = {
  id: string;
  title: string;
  description: string;
  className: string;
  subjectName: string;
  subjectCode: string;
  teacherName: string;
  deadline: string;
  maxMarks: number;
  status: "Pending" | "Overdue" | "Submitted" | "Graded";
  submittedAt?: string;
  marks?: number;
  feedback?: string;
};

export type PagedStudentAssignmentResultDto = {
  items: StudentAssignmentResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export type StudentSubmissionDetailDto = {
  submissionId: string;
  submissionText?: string;
  fileUrl?: string;
  submittedAt: string;
  marks?: number;
  feedback?: string;
  gradedAt?: string;
  gradedByTeacherName?: string;
};

export type StudentAssignmentDetailDto = {
  id: string;
  title: string;
  description: string;
  classId: string;
  className: string;
  classSection: string;
  subjectId: string;
  subjectName: string;
  subjectCode: string;
  teacherName: string;
  teacherEmail: string;
  deadline: string;
  maxMarks: number;
  allowLateSubmission: boolean;
  allowResubmission: boolean;
  status: "Pending" | "Overdue" | "Submitted" | "Graded";
  existingSubmission?: StudentSubmissionDetailDto;
};

export type StudentSubmissionCreateDto = {
  assignmentId: string;
  submissionText?: string;
  fileUrl?: string;
};

export type FileUploadResponseDto = {
  filePath: string; // Relative path in format /assignments/filename.ext
  originalFileName: string;
  fileSize: number;
};

export type StudentSubmissionHistoryFilterDto = {
  subjectName?: string;
  status?: "All" | "Submitted" | "Graded";
  pageNumber: number;
  pageSize: number;
};

export type StudentSubmissionHistoryResponseDto = {
  submissionId: string;
  assignmentId: string;
  assignmentTitle: string;
  subjectName: string;
  subjectCode: string;
  className: string;
  teacherName: string;
  submittedAt: string;
  fileUrl?: string;
  submissionText?: string;
  status: "Submitted" | "Graded";
  marks?: number;
  maxMarks: number;
  feedback?: string;
  gradedAt?: string;
  allowResubmission: boolean;
  deadline: string;
};

export type PagedStudentSubmissionHistoryResultDto = {
  items: StudentSubmissionHistoryResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

export class StudentAssignmentsApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "StudentAssignmentsApiError";
  }
}

/**
 * Fetch published assignments for the student's enrolled classes with pending/submitted/graded filter
 */
export async function getStudentAssignments(
  filter: StudentAssignmentFilterDto
): Promise<PagedStudentAssignmentResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.statusFilter && filter.statusFilter !== "All") query.set("statusFilter", filter.statusFilter);
  if (filter.search) query.set("search", filter.search);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Student/Assignments?${query.toString()}`;

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
      throw new StudentAssignmentsApiError(response.status, errMessage);
    }

    const data = await safeParseJson<PagedStudentAssignmentResultDto>(response, {
      items: [],
      totalCount: 0,
      pageNumber,
      pageSize,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    });

    return {
      ...data,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  } catch (err) {
    if (err instanceof StudentAssignmentsApiError) throw err;
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

/**
 * Fetch full details, deadline countdown data, and submission form status for a specific assignment
 */
export async function getStudentAssignmentDetail(id: string): Promise<StudentAssignmentDetailDto | null> {
  const path = `/Student/AssignmentDetail/${id}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      if (response.status === 404) return null;
      const errMessage = await parseApiResponseError(response);
      throw new StudentAssignmentsApiError(response.status, errMessage);
    }
    return await safeParseJson<StudentAssignmentDetailDto | null>(response, null);
  } catch (err) {
    if (err instanceof StudentAssignmentsApiError) throw err;
    return null;
  }
}

/**
 * Submit work for an assignment (supports text and/or file URL)
 */
export async function submitStudentAssignment(dto: StudentSubmissionCreateDto): Promise<void> {
  const response = await authenticatedFetch("/Student/Submissions", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new StudentAssignmentsApiError(response.status, errMessage);
  }
}

/**
 * Upload assignment attachment file to /wwwroot/assignments/ and receive relative path /assignments/*.
 */
export async function uploadAssignmentFile(file: File): Promise<FileUploadResponseDto> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await authenticatedFetch("/Student/FileUpload", {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new StudentAssignmentsApiError(response.status, errMessage);
  }

  return await safeParseJson<FileUploadResponseDto>(response, {
    filePath: "",
    originalFileName: file.name,
    fileSize: file.size,
  });
}

/**
 * Unsubmit / remove a student submission if allowResubmission is true and before deadline
 */
export async function unsubmitStudentAssignment(submissionId: string): Promise<void> {
  const response = await authenticatedFetch(`/Student/Submissions/${submissionId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const errMessage = await parseApiResponseError(response);
    throw new StudentAssignmentsApiError(response.status, errMessage);
  }
}

/**
 * Fetch history of student's submissions, marks, and feedback
 */
export async function getStudentSubmissionsHistory(
  filter: StudentSubmissionHistoryFilterDto
): Promise<PagedStudentSubmissionHistoryResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.status && filter.status !== "All") query.set("status", filter.status);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Student/MySubmissions?${query.toString()}`;

  try {
    const response = await authenticatedFetch(path, { cache: "no-store" });
    if (!response.ok) {
      const errMessage = await parseApiResponseError(response);
      throw new StudentAssignmentsApiError(response.status, errMessage);
    }

    const data = await safeParseJson<PagedStudentSubmissionHistoryResultDto>(response, {
      items: [],
      totalCount: 0,
      pageNumber,
      pageSize,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    });

    return {
      ...data,
      dataSource: "Server API",
      fetchedAt: new Date().toISOString(),
    };
  } catch (err) {
    if (err instanceof StudentAssignmentsApiError) throw err;
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
