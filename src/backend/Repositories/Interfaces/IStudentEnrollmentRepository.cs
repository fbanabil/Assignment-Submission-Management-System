namespace AssignmentSystem.Api.Repositories.Interfaces;

using AssignmentSystem.Api.Models.Entities;

public interface IStudentEnrollmentRepository
{
    Task<IEnumerable<StudentEnrollment>> GetAllAsync();
    Task<StudentEnrollment?> GetByIdAsync(Guid id);
    Task<StudentEnrollment> AddAsync(StudentEnrollment entity);
    Task UpdateAsync(StudentEnrollment entity);
    Task DeleteAsync(StudentEnrollment entity);
}