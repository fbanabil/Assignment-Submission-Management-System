namespace Backend.DTOs.StudentDTOs;

public class FileUploadResponseDto
{
    public string FilePath { get; set; } = string.Empty; // Format: /assignments/filename.ext
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public class StudentSubmissionHistoryFilterDto
{
    public string? SubjectName { get; set; }
    public string? Status { get; set; } = "All"; // "All", "Submitted", "Graded"
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class StudentSubmissionHistoryResponseDto
{
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? FileUrl { get; set; }
    public string? SubmissionText { get; set; }
    public string Status { get; set; } = "Submitted"; // "Submitted", "Graded"
    public int? Marks { get; set; }
    public int MaxMarks { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
    public bool AllowResubmission { get; set; }
    public DateTime Deadline { get; set; }
}
