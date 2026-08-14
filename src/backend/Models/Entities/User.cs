namespace AssignmentSystem.Api.Models.Entities;

using AssignmentSystem.Api.Models.Enums;
using Backend.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? RollNo { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
    public ICollection<StudentEnrollment> StudentEnrollments { get; set; } = new List<StudentEnrollment>();
    public ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();
    public ICollection<Submission> StudentSubmissions { get; set; } = new List<Submission>();
    public ICollection<Submission> GradedSubmissions { get; set; } = new List<Submission>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}