using AssignmentSystem.Api.Models.Enums;

namespace Backend.DTOs.TeacherDTOs;

public class TeacherClassFilterDto
{
    public string? ClassName { get; set; }
    public string? ClassSection { get; set; }
    public string? AcademicYear { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
