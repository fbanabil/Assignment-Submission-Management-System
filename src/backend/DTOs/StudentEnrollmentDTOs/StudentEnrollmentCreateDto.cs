namespace Backend.DTOs.StudentEnrollmentDTOs;

public class StudentEnrollmentCreateDto
{
    public string StudentEmail { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public Guid? StudentId { get; set; }
}