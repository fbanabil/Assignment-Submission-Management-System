namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.AssignmentDTOs;

public interface IAssignmentService
{
    Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
    Task<Assignment?> GetAssignmentByIdAsync(Guid id);
    Task<Assignment> CreateAssignmentAsync(AssignmentCreateDto dto);
    Task UpdateAssignmentAsync(Guid id, AssignmentUpdateDto dto);
    Task DeleteAssignmentAsync(Guid id);
}