namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.SubjectDTOs;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(Guid id);
    Task<Subject> CreateSubjectAsync(SubjectCreateDto dto);
    Task UpdateSubjectAsync(Guid id, SubjectUpdateDto dto);
    Task DeleteSubjectAsync(Guid id);
}