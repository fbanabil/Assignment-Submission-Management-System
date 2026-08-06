namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface IClassSubjectRepository
{
    Task<IEnumerable<ClassSubject>> GetAllAsync();
    Task<ClassSubject?> GetByIdAsync(Guid id);
    Task<ClassSubject> AddAsync(ClassSubject entity);
    Task UpdateAsync(ClassSubject entity);
    Task DeleteAsync(ClassSubject entity);
}