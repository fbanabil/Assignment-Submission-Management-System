namespace Backend.DTOs.SubjectDTOs;

public class GradeDto
{
    public Guid SubmissionId { get; set; }
    public int Marks { get; set; }
    public string? Feedback { get; set; }
}