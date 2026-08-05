namespace AssignmentSystem.Api.DTOs;

using AssignmentSystem.Api.Models.Enums;

public class AssignmentUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public int? MaxMarks { get; set; }
    public AssignmentStatus? Status { get; set; }
    public bool? AllowLateSubmission { get; set; }
    public bool? AllowResubmission { get; set; }
}