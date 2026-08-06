namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface ITeacherAssignmentRepository
{
    Task<IEnumerable<TeacherAssignment>> GetAllAsync();
    Task<TeacherAssignment?> GetByIdAsync(Guid id);
    Task<TeacherAssignment> AddAsync(TeacherAssignment entity);
    Task UpdateAsync(TeacherAssignment entity);
    Task DeleteAsync(TeacherAssignment entity);
}