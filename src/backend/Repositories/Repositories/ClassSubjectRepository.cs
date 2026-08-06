namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ClassSubjectRepository : IClassSubjectRepository
{
    private readonly AppDbContext _context;

    public ClassSubjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassSubject>> GetAllAsync()
        => await _context.ClassSubjects
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .ToListAsync();

    public async Task<ClassSubject?> GetByIdAsync(Guid id)
        => await _context.ClassSubjects
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<ClassSubject> AddAsync(ClassSubject entity)
    {
        await _context.ClassSubjects.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ClassSubject entity)
    {
        _context.ClassSubjects.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ClassSubject entity)
    {
        _context.ClassSubjects.Remove(entity);
        await _context.SaveChangesAsync();
    }
}