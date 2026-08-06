namespace Backend.Repositories.Repositories;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly AppDbContext _context;

    public SubmissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Submission>> GetAllAsync()
        => await _context.Submissions
            .Include(x => x.Assignment)
            .Include(x => x.Student)
            .Include(x => x.GradeGiver)
            .ToListAsync();

    public async Task<Submission?> GetByIdAsync(Guid id)
        => await _context.Submissions
            .Include(x => x.Assignment)
            .Include(x => x.Student)
            .Include(x => x.GradeGiver)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Submission> AddAsync(Submission entity)
    {
        await _context.Submissions.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Submission entity)
    {
        _context.Submissions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Submission entity)
    {
        _context.Submissions.Remove(entity);
        await _context.SaveChangesAsync();
    }
}