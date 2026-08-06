namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface ISubmissionRepository
{
    Task<IEnumerable<Submission>> GetAllAsync();
    Task<Submission?> GetByIdAsync(Guid id);
    Task<Submission> AddAsync(Submission entity);
    Task UpdateAsync(Submission entity);
    Task DeleteAsync(Submission entity);
}