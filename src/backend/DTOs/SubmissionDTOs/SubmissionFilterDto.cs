namespace Backend.DTOs.SubmissionDTOs;

public class SubmissionFilterDto
{
    public string? ClassName { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public string? AssignmentTitle { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
