namespace Backend.DTOs.StudentEnrollmentDTOs;

public class StudentEnrollmentResponseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentRollNo { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}
