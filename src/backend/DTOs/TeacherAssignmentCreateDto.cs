namespace AssignmentSystem.Api.DTOs;

public class TeacherAssignmentCreateDto
{
    public Guid TeacherId { get; set; }
    public Guid ClassSubjectId { get; set; }
}