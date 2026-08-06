namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface IAssignmentRepository
{
    Task<IEnumerable<Assignment>> GetAllAsync();
    Task<Assignment?> GetByIdAsync(Guid id);
    Task<Assignment> AddAsync(Assignment entity);
    Task UpdateAsync(Assignment entity);
    Task DeleteAsync(Assignment entity);
}