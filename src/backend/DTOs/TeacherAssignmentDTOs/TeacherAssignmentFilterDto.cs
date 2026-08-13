namespace Backend.DTOs.TeacherAssignmentDTOs;

public class TeacherAssignmentFilterDto
{
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }
    public string? ClassName { get; set; }
    public string? SubjectCode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
