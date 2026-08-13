import { getApiUrl, parseApiResponseError, safeParseJson } from "./api-error";

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

const apiBaseUrl = rawBaseUrl;

// Server root URL without /api (e.g. http://localhost:5000)
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

/**
 * Resolves submission file URLs to point to backend server static files (wwwroot/assignments)
 */
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

// In-memory fallback dataset for frontend demo preview
let demoSubmissionsDatabase: SubmissionResponseDto[] = [
  {
    id: "sub-001",
    studentName: "Alexander Wright",
    studentEmail: "alexander.wright@student.edu",
    assignmentTitle: "Quadratic Equations Problem Set",
    className: "Grade 10 - Mathematics",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Mathematics",
    subjectCode: "MATH101",
    fileUrl: "/assignments/math101_homework_alexander.pdf",
    submittedAt: "2024-11-18T14:30:00Z",
    grade: 95,
    maxMarks: 100,
    feedback: "Excellent work! All steps and work shown clearly.",
    status: "Graded",
  },
  {
    id: "sub-002",
    studentName: "Chloe Bennett",
    studentEmail: "chloe.bennett@student.edu",
    assignmentTitle: "Physics Mechanics Lab Report",
    className: "Grade 11 - Physics",
    classSection: "Section C",
    academicYear: "2024-2025",
    subjectName: "Physics",
    subjectCode: "PHYS102",
    fileUrl: "/assignments/physics_lab_report_chloe.docx",
    submittedAt: "2024-10-14T21:15:00Z",
    grade: 42,
    maxMarks: 50,
    feedback: "Good experimental data. Include error analysis next time.",
    status: "Graded",
  },
  {
    id: "sub-003",
    studentName: "Emma Watson",
    studentEmail: "emma.watson@student.edu",
    assignmentTitle: "Data Structures Binary Trees Project",
    className: "Grade 11 - Computer Science",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Computer Science",
    subjectCode: "CS103",
    fileUrl: "/assignments/bst_project_emma.zip",
    submittedAt: "2024-11-19T18:00:00Z",
    maxMarks: 100,
    status: "Submitted",
  },
  {
    id: "sub-004",
    studentName: "Franklin Pierce",
    studentEmail: "franklin.pierce@student.edu",
    assignmentTitle: "Organic Chemistry Reaction Mechanisms Essay",
    className: "Grade 12 - Chemistry",
    classSection: "Section B",
    academicYear: "2024-2025",
    subjectName: "Chemistry",
    subjectCode: "CHEM104",
    fileUrl: "/assignments/chemistry_essay_franklin.pdf",
    submittedAt: "2024-10-02T11:45:00Z",
    grade: 88,
    maxMarks: 100,
    feedback: "Well structured essay with accurate mechanism diagrams.",
    status: "Graded",
  },
  {
    id: "sub-005",
    studentName: "Henry Cavendish",
    studentEmail: "henry.cavendish@student.edu",
    assignmentTitle: "Shakespeare's Hamlet Critical Analysis",
    className: "Grade 12 - Advanced English",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Advanced English",
    subjectCode: "ENG107",
    fileUrl: "/assignments/hamlet_essay_henry.png",
    submittedAt: "2024-11-21T09:20:00Z",
    maxMarks: 100,
    status: "Late",
  },
];

/**
 * Retrieves paginated system-wide submissions with filtering options
 */
export async function getSubmissions(
  filter: SubmissionFilterDto
): Promise<PagedSubmissionResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  if (!apiBaseUrl) {
    let filtered = [...demoSubmissionsDatabase];

    if (filter.className && filter.className.trim() !== "") {
      const classVal = filter.className.trim().toLowerCase();
      filtered = filtered.filter((s) => s.className.toLowerCase().includes(classVal));
    }

    if (filter.subjectName && filter.subjectName.trim() !== "") {
      const sbjVal = filter.subjectName.trim().toLowerCase();
      filtered = filtered.filter((s) => s.subjectName.toLowerCase().includes(sbjVal));
    }

    if (filter.subjectCode && filter.subjectCode.trim() !== "") {
      const codeVal = filter.subjectCode.trim().toLowerCase();
      filtered = filtered.filter((s) => s.subjectCode.toLowerCase().includes(codeVal));
    }

    if (filter.assignmentTitle && filter.assignmentTitle.trim() !== "") {
      const titleVal = filter.assignmentTitle.trim().toLowerCase();
      filtered = filtered.filter((s) => s.assignmentTitle.toLowerCase().includes(titleVal));
    }

    if (filter.studentName && filter.studentName.trim() !== "") {
      const nameVal = filter.studentName.trim().toLowerCase();
      filtered = filtered.filter((s) => s.studentName.toLowerCase().includes(nameVal));
    }

    if (filter.studentEmail && filter.studentEmail.trim() !== "") {
      const emailVal = filter.studentEmail.trim().toLowerCase();
      filtered = filtered.filter((s) => s.studentEmail.toLowerCase().includes(emailVal));
    }

    if (filter.status && filter.status.trim() !== "") {
      filtered = filtered.filter((s) => s.status === filter.status);
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
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (filter.assignmentTitle) query.set("assignmentTitle", filter.assignmentTitle);
  if (filter.studentName) query.set("studentName", filter.studentName);
  if (filter.studentEmail) query.set("studentEmail", filter.studentEmail);
  if (filter.status) query.set("status", filter.status);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const url = getApiUrl(`/Admin/Submissions?${query.toString()}`);

  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
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
    throw new Error(`Failed to fetch submissions: ${err instanceof Error ? err.message : String(err)}`);
  }
}
