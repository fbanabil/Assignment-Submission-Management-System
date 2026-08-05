namespace AssignmentSystem.Api.DTOs;

public class StudentEnrollmentCreateDto
{
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
}