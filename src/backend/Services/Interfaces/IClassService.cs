namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.UserDTOs;

public interface IClassService
{
    Task<IEnumerable<Class>> GetAllClassesAsync();
    Task<Class?> GetClassByIdAsync(Guid id);
    Task<Class> CreateClassAsync(ClassCreateDto dto);
    Task UpdateClassAsync(Guid id, ClassUpdateDto dto);
    Task DeleteClassAsync(Guid id);
    Task<PagedResultDto<ClassResponseDto>> GetClassesAsync(ClassFilterDto filterDto);
}