namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class SubjectRepository : ISubjectRepository
{
    private readonly AppDbContext _context;

    public SubjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Subject>> GetAllAsync()
        => await _context.Subjects.ToListAsync();

    public async Task<Subject?> GetByIdAsync(Guid id)
        => await _context.Subjects.FindAsync(id);

    public async Task<Subject> AddAsync(Subject entity)
    {
        await _context.Subjects.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Subject entity)
    {
        _context.Subjects.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subject entity)
    {
        _context.Subjects.Remove(entity);
        await _context.SaveChangesAsync();
    }
}