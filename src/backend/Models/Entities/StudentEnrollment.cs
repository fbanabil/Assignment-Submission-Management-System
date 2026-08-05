namespace AssignmentSystem.Api.Models.Entities;

public class StudentEnrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public DateTime EnrolledAt { get; set; }

    // Navigation properties
    public User Student { get; set; } = null!;
    public Class Class { get; set; } = null!;
}