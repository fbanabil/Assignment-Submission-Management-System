namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Assignment>> GetAllAsync()
        => await _context.Assignments
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .ToListAsync();

    public async Task<Assignment?> GetByIdAsync(Guid id)
        => await _context.Assignments
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Assignment> AddAsync(Assignment entity)
    {
        await _context.Assignments.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Assignment entity)
    {
        _context.Assignments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Assignment entity)
    {
        _context.Assignments.Remove(entity);
        await _context.SaveChangesAsync();
    }
}