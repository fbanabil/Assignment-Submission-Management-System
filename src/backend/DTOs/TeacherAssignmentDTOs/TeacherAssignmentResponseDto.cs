namespace Backend.DTOs.TeacherAssignmentDTOs;

public class TeacherAssignmentResponseDto
{
    public Guid Id { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
