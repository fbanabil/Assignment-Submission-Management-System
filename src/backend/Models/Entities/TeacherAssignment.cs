namespace AssignmentSystem.Api.Models.Entities;

public class TeacherAssignment
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassSubjectId { get; set; }

    // Navigation properties
    public User Teacher { get; set; } = null!;
    public ClassSubject ClassSubject { get; set; } = null!;
}