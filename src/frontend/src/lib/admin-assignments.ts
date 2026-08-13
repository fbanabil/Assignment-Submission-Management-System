import { getApiUrl, parseApiResponseError, safeParseJson } from "./api-error";

export type AssignmentResponseDto = {
  id: string;
  title: string;
  description: string;
  className: string;
  classSection: string;
  academicYear: string;
  subjectName: string;
  subjectCode: string;
  teacherName: string;
  teacherEmail: string;
  dueDate: string;
  createdAt: string;
  maxMarks: number;
  status: "Active" | "Past Due" | "Draft" | "Published";
  totalSubmissions: number;
  allowLateSubmission: boolean;
};

export type AssignmentFilterDto = {
  title?: string;
  className?: string;
  subjectName?: string;
  subjectCode?: string;
  teacherName?: string;
  teacherEmail?: string;
  status?: string;
  pageNumber: number;
  pageSize: number;
};

export type PagedAssignmentResultDto = {
  items: AssignmentResponseDto[];
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

export class AssignmentApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "AssignmentApiError";
  }
}

// In-memory fallback dataset for frontend demo preview
let demoAssignmentsDatabase: AssignmentResponseDto[] = [
  {
    id: "asg-001",
    title: "Quadratic Equations Problem Set",
    description: "Solve problems 1 to 20 on page 142. Show all work and steps clearly.",
    className: "Grade 10 - Mathematics",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Mathematics",
    subjectCode: "MATH101",
    teacherName: "Marcus Sterling",
    teacherEmail: "marcus.sterling@school.edu",
    dueDate: "2024-11-20T23:59:00Z",
    createdAt: "2024-11-10T09:00:00Z",
    maxMarks: 100,
    status: "Active",
    totalSubmissions: 28,
    allowLateSubmission: true,
  },
  {
    id: "asg-002",
    title: "Physics Mechanics Lab Report",
    description: "Submit a complete lab report detailing experiment results for Newton's laws of motion.",
    className: "Grade 11 - Physics",
    classSection: "Section C",
    academicYear: "2024-2025",
    subjectName: "Physics",
    subjectCode: "PHYS102",
    teacherName: "Sophia Rodriguez",
    teacherEmail: "sophia.rodriguez@school.edu",
    dueDate: "2024-10-15T23:59:00Z",
    createdAt: "2024-10-01T10:30:00Z",
    maxMarks: 50,
    status: "Past Due",
    totalSubmissions: 32,
    allowLateSubmission: false,
  },
  {
    id: "asg-003",
    title: "Data Structures Binary Trees Project",
    description: "Implement a Binary Search Tree (BST) in C++ or Java with insert, delete, and traversal methods.",
    className: "Grade 11 - Computer Science",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Computer Science",
    subjectCode: "CS103",
    teacherName: "David Chen",
    teacherEmail: "david.chen@school.edu",
    dueDate: "2024-12-05T23:59:00Z",
    createdAt: "2024-11-15T14:00:00Z",
    maxMarks: 100,
    status: "Active",
    totalSubmissions: 19,
    allowLateSubmission: true,
  },
  {
    id: "asg-004",
    title: "Organic Chemistry Reaction Mechanisms Essay",
    description: "Write a 1500-word analysis on substitution and elimination reaction mechanisms.",
    className: "Grade 12 - Chemistry",
    classSection: "Section B",
    academicYear: "2024-2025",
    subjectName: "Chemistry",
    subjectCode: "CHEM104",
    teacherName: "James Maxwell",
    teacherEmail: "james.maxwell@school.edu",
    dueDate: "2024-09-30T23:59:00Z",
    createdAt: "2024-09-12T11:00:00Z",
    maxMarks: 100,
    status: "Past Due",
    totalSubmissions: 25,
    allowLateSubmission: true,
  },
  {
    id: "asg-005",
    title: "Shakespeare's Hamlet Critical Analysis",
    description: "Draft a critical essay exploring the theme of madness in Shakespeare's Hamlet.",
    className: "Grade 12 - Advanced English",
    classSection: "Section A",
    academicYear: "2024-2025",
    subjectName: "Advanced English",
    subjectCode: "ENG107",
    teacherName: "Grace Hopper",
    teacherEmail: "grace.hopper@school.edu",
    dueDate: "2024-12-15T23:59:00Z",
    createdAt: "2024-11-20T08:45:00Z",
    maxMarks: 100,
    status: "Active",
    totalSubmissions: 12,
    allowLateSubmission: false,
  },
];

/**
 * Retrieves paginated system-wide assignments with filtering options
 */
export async function getAssignments(
  filter: AssignmentFilterDto
): Promise<PagedAssignmentResultDto> {
  const pageNumber = Math.max(1, filter.pageNumber || 1);
  const pageSize = Math.max(1, filter.pageSize || 10);

  if (!apiBaseUrl) {
    let filtered = [...demoAssignmentsDatabase];

    if (filter.title && filter.title.trim() !== "") {
      const titleVal = filter.title.trim().toLowerCase();
      filtered = filtered.filter((a) => a.title.toLowerCase().includes(titleVal));
    }

    if (filter.className && filter.className.trim() !== "") {
      const classVal = filter.className.trim().toLowerCase();
      filtered = filtered.filter((a) => a.className.toLowerCase().includes(classVal));
    }

    if (filter.subjectName && filter.subjectName.trim() !== "") {
      const sbjVal = filter.subjectName.trim().toLowerCase();
      filtered = filtered.filter((a) => a.subjectName.toLowerCase().includes(sbjVal));
    }

    if (filter.subjectCode && filter.subjectCode.trim() !== "") {
      const codeVal = filter.subjectCode.trim().toLowerCase();
      filtered = filtered.filter((a) => a.subjectCode.toLowerCase().includes(codeVal));
    }

    if (filter.teacherName && filter.teacherName.trim() !== "") {
      const teacherVal = filter.teacherName.trim().toLowerCase();
      filtered = filtered.filter((a) => a.teacherName.toLowerCase().includes(teacherVal));
    }

    if (filter.teacherEmail && filter.teacherEmail.trim() !== "") {
      const emailVal = filter.teacherEmail.trim().toLowerCase();
      filtered = filtered.filter((a) => a.teacherEmail.toLowerCase().includes(emailVal));
    }

    if (filter.status && filter.status.trim() !== "") {
      filtered = filtered.filter((a) => a.status === filter.status);
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
  if (filter.title) query.set("title", filter.title);
  if (filter.className) query.set("className", filter.className);
  if (filter.subjectName) query.set("subjectName", filter.subjectName);
  if (filter.subjectCode) query.set("subjectCode", filter.subjectCode);
  if (filter.teacherName) query.set("teacherName", filter.teacherName);
  if (filter.teacherEmail) query.set("teacherEmail", filter.teacherEmail);
  if (filter.status) query.set("status", filter.status);
  query.set("pageNumber", String(pageNumber));
  query.set("pageSize", String(pageSize));

  const url = getApiUrl(`/Admin/Assignments?${query.toString()}`);

  try {
    const response = await fetch(url, { cache: "no-store" });
    if (!response.ok) {
      const errMessage = await parseApiResponseError(response);
      throw new AssignmentApiError(response.status, errMessage);
    }
    return await safeParseJson<PagedAssignmentResultDto>(response, {
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
    if (err instanceof AssignmentApiError) throw err;
    throw new Error(`Failed to fetch assignments: ${err instanceof Error ? err.message : String(err)}`);
  }
}
