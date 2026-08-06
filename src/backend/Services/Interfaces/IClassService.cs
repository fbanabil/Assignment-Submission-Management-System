namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.ClassDTOs;

public interface IClassService
{
    Task<IEnumerable<Class>> GetAllClassesAsync();
    Task<Class?> GetClassByIdAsync(Guid id);
    Task<Class> CreateClassAsync(ClassCreateDto dto);
    Task UpdateClassAsync(Guid id, ClassUpdateDto dto);
    Task DeleteClassAsync(Guid id);
}