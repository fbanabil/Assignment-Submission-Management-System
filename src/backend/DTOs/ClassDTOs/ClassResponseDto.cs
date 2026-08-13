namespace Backend.DTOs.ClassDTOs;

public class ClassResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
