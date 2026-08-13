namespace Backend.DTOs.SubjectDTOs;

public class SubjectFilterDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public Guid? ClassId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
