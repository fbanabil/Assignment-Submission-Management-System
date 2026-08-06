namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.StudentEnrollmentDTOs;

public interface IStudentEnrollmentService
{
    Task<IEnumerable<StudentEnrollment>> GetAllStudentEnrollmentsAsync();
    Task<StudentEnrollment?> GetStudentEnrollmentByIdAsync(Guid id);
    Task<StudentEnrollment> CreateStudentEnrollmentAsync(StudentEnrollmentCreateDto dto);
    Task DeleteStudentEnrollmentAsync(Guid id);
}