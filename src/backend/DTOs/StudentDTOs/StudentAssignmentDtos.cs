namespace Backend.DTOs.StudentDTOs;

public class StudentAssignmentFilterDto
{
    public Guid? StudentId { get; set; }
    public string? StatusFilter { get; set; } = "All"; // "All", "Pending", "Submitted", "Graded"
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class StudentAssignmentResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public string Status { get; set; } = "Pending"; // "Pending", "Overdue", "Submitted", "Graded"
    public DateTime? SubmittedAt { get; set; }
    public int? Marks { get; set; }
    public string? Feedback { get; set; }
}

public class StudentSubmissionDetailDto
{
    public Guid SubmissionId { get; set; }
    public string? SubmissionText { get; set; }
    public string? FileUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? GradedByTeacherName { get; set; }
}

public class StudentAssignmentDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public bool AllowLateSubmission { get; set; }
    public bool AllowResubmission { get; set; }
    public string Status { get; set; } = "Pending";
    public StudentSubmissionDetailDto? ExistingSubmission { get; set; }
}

public class StudentSubmissionCreateDto
{
    public Guid AssignmentId { get; set; }
    public Guid? StudentId { get; set; }
    public string? SubmissionText { get; set; }
    public string? FileUrl { get; set; }
}
