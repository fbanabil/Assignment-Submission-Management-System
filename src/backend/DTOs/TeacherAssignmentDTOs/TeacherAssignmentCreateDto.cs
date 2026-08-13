namespace Backend.DTOs.TeacherAssignmentDTOs;

public class TeacherAssignmentCreateDto
{
    public Guid TeacherId { get; set; }
    public Guid ClassSubjectId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
}