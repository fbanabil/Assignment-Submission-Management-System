using AssignmentSystem.Api.Models.Enums;

namespace Backend.DTOs.TeacherAssignmentDTOs;

public class TeacherAssignmentFilterDto
{
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }
    public string? ClassName { get; set; }
    public string? SubjectCode { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
