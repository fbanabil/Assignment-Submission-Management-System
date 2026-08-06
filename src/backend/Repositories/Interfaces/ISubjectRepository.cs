namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface ISubjectRepository
{
    Task<IEnumerable<Subject>> GetAllAsync();
    Task<Subject?> GetByIdAsync(Guid id);
    Task<Subject> AddAsync(Subject entity);
    Task UpdateAsync(Subject entity);
    Task DeleteAsync(Subject entity);
}