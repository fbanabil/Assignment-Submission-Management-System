namespace Backend.DTOs.AssignmentDTOs;

public class AssignmentFilterDto
{
    public string? Title { get; set; }
    public string? ClassName { get; set; }
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
