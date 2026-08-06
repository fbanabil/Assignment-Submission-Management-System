namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface IClassRepository
{
    Task<IEnumerable<Class>> GetAllAsync();
    Task<Class?> GetByIdAsync(Guid id);
    Task<Class> AddAsync(Class entity);
    Task UpdateAsync(Class entity);
    Task DeleteAsync(Class entity);
}