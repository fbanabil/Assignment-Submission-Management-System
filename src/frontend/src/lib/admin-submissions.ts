import { authenticatedFetch, parseApiResponseError, safeParseJson } from "./api-error";

export type SubmissionResponseDto = {
  id: string;
  studentName: string;
  studentEmail: string;
  assignmentTitle: string;
  className: string;
  classSection: string;
  academicYear: string;
  subjectName: string;
  subjectCode: string;
  fileUrl: string;
  submittedAt: string;
  grade?: number;
  maxMarks: number;
  feedback?: string;
  status: "Submitted" | "Graded" | "Late" | "Pending";
};

export type SubmissionFilterDto = {
  className?: string;
  subjectName?: string;
  subjectCode?: string;
  assignmentTitle?: string;
  studentName?: string;
  studentEmail?: string;
  status?: string;
  pageNumber: number;
  pageSize: number;
};

export type PagedSubmissionResultDto = {
  items: SubmissionResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  dataSource?: string;
  fetchedAt?: string;
};

const rawBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ??
  "";

export const serverOriginUrl = (rawBaseUrl || "http://localhost:5000").replace(/\/api$/i, "");

export class SubmissionApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "SubmissionApiError";
  }
}

export function resolveSubmissionFileUrl(fileUrl: string): string {
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

export async function getSubmissions(
  filter: SubmissionFilterDto
): Promise<PagedSubmissionResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  const query = new URLSearchParams();
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (filter.assignmentTitle) query.set("assignmentTitle", filter.assignmentTitle);
  if (filter.studentName) query.set("studentName", filter.studentName);
  if (filter.studentEmail) query.set("studentEmail", filter.studentEmail);
  if (filter.status) query.set("status", filter.status);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const path = `/Admin/Submissions?${query.toString()}`;

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
      throw new SubmissionApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedSubmissionResultDto>(response, {
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
    if (err instanceof SubmissionApiError) throw err;
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
