namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using System.Collections.Generic;

public interface ITeacherAssignmentService
{
    Task<IEnumerable<TeacherAssignment>> GetAllTeacherAssignmentsAsync();
    Task<TeacherAssignment?> GetTeacherAssignmentByIdAsync(Guid id);
    Task<TeacherAssignment> CreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto);
    Task DeleteTeacherAssignmentAsync(Guid id);
    Task<PagedResultDto<TeacherAssignmentResponseDto>> GetTeacherAssignmentsAsync(TeacherAssignmentFilterDto dto);
    Task<List<TeacherAssignedClassSubjectDto>> GetAssignedClasses(Guid teacherId);
    Task<PagedResultDto<TeacherAssignedClassSubjectDto>> GetAssignedClassesPagedAsync(Guid teacherId, TeacherClassFilterDto filterDto);
}