namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.TeacherAssignmentDTOs;

public interface ITeacherAssignmentService
{
    Task<IEnumerable<TeacherAssignment>> GetAllTeacherAssignmentsAsync();
    Task<TeacherAssignment?> GetTeacherAssignmentByIdAsync(Guid id);
    Task<TeacherAssignment> CreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto);
    Task DeleteTeacherAssignmentAsync(Guid id);
}