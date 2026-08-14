namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.StudentDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using System.Collections.Generic;

public interface IAssignmentService
{
    Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
    Task<Assignment?> GetAssignmentByIdAsync(Guid id);
    Task<AssignmentResponseDto> CreateAssignmentAsync(AssignmentCreateDto dto);

    Task<AssignmentResponseDto> UpdateAssignmentAsync(Guid id, AssignmentUpdateDto dto);
    Task DeleteAssignmentAsync(Guid id);
    Task<AssignmentSummaryDto> GetAssignmentSummaryAsync();
    Task<PagedResultDto<AssignmentResponseDto>> GetAssignmentsAsync(AssignmentFilterDto filterDto);
    Task<int> GetTotalAssignedClassesCount(Guid teacherId);
    Task<int> GetActiveAssignmentsCount(Guid teacherId);
    Task<List<TeacherUpcomingDeadlineDto>> GetUpcomingDeadlines(Guid teacherId);
    Task<PagedResultDto<AssignmentResponseDto>> GetAssignmentsForTeacher(AssignmentFilterDto dto);
    Task<PagedResultDto<StudentAssignmentResponseDto>> GetAssignmentsForStudentPagedAsync(Guid studentId, StudentAssignmentFilterDto filterDto);
    Task<StudentAssignmentDetailDto?> GetAssignmentDetailForStudentAsync(Guid studentId, Guid assignmentId);
}