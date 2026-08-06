namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.ClassSubjectDTOs;

public interface IClassSubjectService
{
    Task<IEnumerable<ClassSubject>> GetAllClassSubjectsAsync();
    Task<ClassSubject?> GetClassSubjectByIdAsync(Guid id);
    Task<ClassSubject> CreateClassSubjectAsync(ClassSubjectCreateDto dto);
    Task DeleteClassSubjectAsync(Guid id);
}