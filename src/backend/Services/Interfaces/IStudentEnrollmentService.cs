namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.StudentEnrollmentDTOs;
using System.Collections.Generic;

using Backend.DTOs;
using Backend.DTOs.UserDTOs;

public interface IStudentEnrollmentService
{
    Task<IEnumerable<StudentEnrollment>> GetAllStudentEnrollmentsAsync();
    Task<StudentEnrollment?> GetStudentEnrollmentByIdAsync(Guid id);
    Task<StudentEnrollment> CreateStudentEnrollmentAsync(StudentEnrollmentCreateDto dto);
    Task DeleteStudentEnrollmentAsync(Guid id);
    Task<List<Guid>> GetEnrolledClassIdsAsync(Guid targetStudentId);
    Task<PagedResultDto<StudentEnrollmentResponseDto>> GetStudentEnrollmentsAsync(StudentEnrollmentFilterDto filterDto);
    Task<PagedResultDto<StudentEnrollmentResponseDto>> GetStudentEnrollmentsForTeacherAsync(Guid teacherId, StudentEnrollmentFilterDto filterDto);
}