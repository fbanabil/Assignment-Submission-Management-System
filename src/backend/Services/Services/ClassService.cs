namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.ClassDTOs;
using Microsoft.EntityFrameworkCore;

public class ClassService : IClassService
{
    private readonly AppDbContext _context;

    public ClassService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Class>> GetAllClassesAsync() =>
        await _context.Classes.ToListAsync();

    public async Task<Class?> GetClassByIdAsync(Guid id) =>
        await _context.Classes.FindAsync(id);

    public async Task<Class> CreateClassAsync(ClassCreateDto dto)
    {
        var cls = new Class
        {
            Name = dto.Name,
            Section = dto.Section,
            AcademicYear = dto.AcademicYear
        };

        _context.Classes.Add(cls);
        await _context.SaveChangesAsync();
        return cls;
    }

    public async Task UpdateClassAsync(Guid id, ClassUpdateDto dto)
    {
        var cls = await _context.Classes.FindAsync(id);
        if (cls == null) return;

        if (dto.Name != null) cls.Name = dto.Name;
        if (dto.Section != null) cls.Section = dto.Section;
        if (dto.AcademicYear != null) cls.AcademicYear = dto.AcademicYear;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteClassAsync(Guid id)
    {
        var cls = await _context.Classes.FindAsync(id);
        if (cls != null)
        {
            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();
        }
    }
}