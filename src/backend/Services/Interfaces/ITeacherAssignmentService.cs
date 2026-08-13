namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;

public interface ITeacherAssignmentService
{
    Task<IEnumerable<TeacherAssignment>> GetAllTeacherAssignmentsAsync();
    Task<TeacherAssignment?> GetTeacherAssignmentByIdAsync(Guid id);
    Task<TeacherAssignment> CreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto);
    Task DeleteTeacherAssignmentAsync(Guid id);
    Task<PagedResultDto<TeacherAssignmentResponseDto>> GetTeacherAssignmentsAsync(TeacherAssignmentFilterDto dto);
}