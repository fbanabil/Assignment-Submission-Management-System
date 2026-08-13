namespace Backend.DTOs.TeacherDTOs;

public class TeacherClassFilterDto
{
    public string? ClassName { get; set; }
    public string? ClassSection { get; set; }
    public string? AcademicYear { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
