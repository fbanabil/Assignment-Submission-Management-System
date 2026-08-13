namespace Backend.DTOs.AssignmentDTOs;

public class AssignmentResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MaxMarks { get; set; }
    public string Status { get; set; } = "Active";
    public int TotalSubmissions { get; set; }
    public bool AllowLateSubmission { get; set; }
}
