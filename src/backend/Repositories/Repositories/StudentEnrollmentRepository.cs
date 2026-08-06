namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class StudentEnrollmentRepository : IStudentEnrollmentRepository
{
    private readonly AppDbContext _context;

    public StudentEnrollmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentEnrollment>> GetAllAsync()
        => await _context.StudentEnrollments
            .Include(x => x.Student)
            .Include(x => x.Class)
            .ToListAsync();

    public async Task<StudentEnrollment?> GetByIdAsync(Guid id)
        => await _context.StudentEnrollments
            .Include(x => x.Student)
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<StudentEnrollment> AddAsync(StudentEnrollment entity)
    {
        await _context.StudentEnrollments.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(StudentEnrollment entity)
    {
        _context.StudentEnrollments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(StudentEnrollment entity)
    {
        _context.StudentEnrollments.Remove(entity);
        await _context.SaveChangesAsync();
    }
}