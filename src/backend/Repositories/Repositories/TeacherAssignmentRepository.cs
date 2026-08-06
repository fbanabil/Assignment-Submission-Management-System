namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class TeacherAssignmentRepository : ITeacherAssignmentRepository
{
    private readonly AppDbContext _context;

    public TeacherAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeacherAssignment>> GetAllAsync()
        => await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.ClassSubject)
            .ToListAsync();

    public async Task<TeacherAssignment?> GetByIdAsync(Guid id)
        => await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.ClassSubject)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<TeacherAssignment> AddAsync(TeacherAssignment entity)
    {
        await _context.TeacherAssignments.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TeacherAssignment entity)
    {
        _context.TeacherAssignments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TeacherAssignment entity)
    {
        _context.TeacherAssignments.Remove(entity);
        await _context.SaveChangesAsync();
    }
}