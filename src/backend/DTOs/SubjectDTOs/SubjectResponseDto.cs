namespace Backend.DTOs.SubjectDTOs;

public class ClassSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
}

public class SubjectResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public IEnumerable<ClassSummaryDto> LinkedClasses { get; set; } = new List<ClassSummaryDto>();
}
