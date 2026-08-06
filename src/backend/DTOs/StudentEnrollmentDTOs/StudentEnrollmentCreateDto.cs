namespace Backend.DTOs.StudentEnrollmentDTOs;

public class StudentEnrollmentCreateDto
{
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
}