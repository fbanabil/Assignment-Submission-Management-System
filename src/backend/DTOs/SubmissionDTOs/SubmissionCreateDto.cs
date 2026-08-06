namespace Backend.DTOs.SubmissionDTOs;

public class SubmissionCreateDto
{
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string? SubmissionText { get; set; }
    public string? FileUrl { get; set; }
}