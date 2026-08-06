namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.SubjectDTOs;
using Microsoft.EntityFrameworkCore;

public class SubjectService : ISubjectService
{
    private readonly AppDbContext _context;

    public SubjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync() =>
        await _context.Subjects.ToListAsync();

    public async Task<Subject?> GetSubjectByIdAsync(Guid id) =>
        await _context.Subjects.FindAsync(id);

    public async Task<Subject> CreateSubjectAsync(SubjectCreateDto dto)
    {
        var subject = new Subject
        {
            Name = dto.Name,
            Code = dto.Code
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task UpdateSubjectAsync(Guid id, SubjectUpdateDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return;

        if (dto.Name != null) subject.Name = dto.Name;
        if (dto.Code != null) subject.Code = dto.Code;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteSubjectAsync(Guid id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject != null)
        {
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }
    }
}