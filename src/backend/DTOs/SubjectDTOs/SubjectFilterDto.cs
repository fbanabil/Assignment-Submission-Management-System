using AssignmentSystem.Api.Models.Enums;

namespace Backend.DTOs.SubjectDTOs;

public class SubjectFilterDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public Guid? ClassId { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
