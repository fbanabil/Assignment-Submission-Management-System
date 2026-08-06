namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _context;

    public ClassRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Class>> GetAllAsync()
        => await _context.Classes.ToListAsync();

    public async Task<Class?> GetByIdAsync(Guid id)
        => await _context.Classes.FindAsync(id);

    public async Task<Class> AddAsync(Class entity)
    {
        await _context.Classes.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Class entity)
    {
        _context.Classes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Class entity)
    {
        _context.Classes.Remove(entity);
        await _context.SaveChangesAsync();
    }
}