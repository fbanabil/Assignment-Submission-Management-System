namespace Backend.DTOs.SubjectDTOs;

public class SubjectUpdateDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
}