using AssignmentSystem.Api.Models.Enums;

namespace Backend.DTOs.ClassDTOs;

public class ClassFilterDto
{
    public string? Name { get; set; }
    public string? Section { get; set; }
    public string? AcademicYear { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
