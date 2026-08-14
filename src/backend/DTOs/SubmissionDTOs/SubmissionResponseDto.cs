namespace Backend.DTOs.SubmissionDTOs;

public class SubmissionResponseDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentRollNo { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string? FileUrl { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int? Grade { get; set; }
    public int MaxMarks { get; set; } = 100;
    public string? Feedback { get; set; }
    public string Status { get; set; } = "Submitted";
}
