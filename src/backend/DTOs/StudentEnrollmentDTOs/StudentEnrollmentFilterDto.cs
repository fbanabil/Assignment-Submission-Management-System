using AssignmentSystem.Api.Models.Enums;

namespace Backend.DTOs.StudentEnrollmentDTOs;

public class StudentEnrollmentFilterDto
{
    public Guid? ClassId { get; set; }
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? ClassName { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
