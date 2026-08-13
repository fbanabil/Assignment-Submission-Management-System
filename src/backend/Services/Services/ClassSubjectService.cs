namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.ClassSubjectDTOs;
using Microsoft.EntityFrameworkCore;

public class ClassSubjectService : IClassSubjectService
{
    private readonly AppDbContext _context;

    public ClassSubjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassSubject>> GetAllClassSubjectsAsync() =>
        await _context.ClassSubjects.Include(cs => cs.Class).Include(cs => cs.Subject).ToListAsync();

    public async Task<ClassSubject?> GetClassSubjectByIdAsync(Guid id) =>
        await _context.ClassSubjects.FindAsync(id);




    /// <summary>
    /// This method creates a new ClassSubject entity based on the provided ClassSubjectCreateDto and saves it to the database.
    /// </summary>
    /// <param name="dto">The data transfer object containing the class and subject IDs.</param>
    /// <returns>The created ClassSubject entity.</returns>
    public async Task<ClassSubject> CreateClassSubjectAsync(ClassSubjectCreateDto dto)
    {
        var classSubject = new ClassSubject
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId
        };

        _context.ClassSubjects.Add(classSubject);
        await _context.SaveChangesAsync();
        return classSubject;
    }





    public async Task DeleteClassSubjectAsync(Guid id)
    {
        var classSubject = await _context.ClassSubjects.FindAsync(id);
        if (classSubject != null)
        {
            _context.ClassSubjects.Remove(classSubject);
            await _context.SaveChangesAsync();
        }
    }
}