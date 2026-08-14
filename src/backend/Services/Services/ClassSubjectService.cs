namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.ClassSubjectDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Backend.Middlewares;

public class ClassSubjectService : IClassSubjectService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClassSubjectService> _logger;

    public ClassSubjectService(AppDbContext context, ILogger<ClassSubjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ClassSubject>> GetAllClassSubjectsAsync()
    {
        _logger.LogInformation("ClassSubjectService: Fetching all ClassSubjects");
        return await _context.ClassSubjects.Include(cs => cs.Class).Include(cs => cs.Subject).ToListAsync();
    }

    public async Task<ClassSubject?> GetClassSubjectByIdAsync(Guid id)
    {
        _logger.LogInformation("ClassSubjectService: Fetching ClassSubject by Id:{Id}", id);
        return await _context.ClassSubjects.FindAsync(id);
    }

    /// <summary>
    /// This method creates a new ClassSubject entity based on the provided ClassSubjectCreateDto and saves it to the database.
    /// </summary>
    /// <param name="dto">The data transfer object containing the class and subject IDs.</param>
    /// <returns>The created ClassSubject entity.</returns>
    public async Task<ClassSubject> CreateClassSubjectAsync(ClassSubjectCreateDto dto)
    {
        _logger.LogInformation("ClassSubjectService: Creating ClassSubject ClassId:{ClassId}, SubjectId:{SubjectId}", dto.ClassId, dto.SubjectId);
        // Check if the ClassSubject already exists to prevent duplicates
        var existingClassSubject = await _context.ClassSubjects
            .FirstOrDefaultAsync(cs => cs.ClassId == dto.ClassId && cs.SubjectId == dto.SubjectId);

        if (existingClassSubject != null)
        {
            _logger.LogWarning("ClassSubjectService: Association already exists for ClassId:{ClassId}, SubjectId:{SubjectId}", dto.ClassId, dto.SubjectId);
            throw new BadRequestException("This class subject association already exists.");
        }


        var classSubject = new ClassSubject
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId
        };

        _context.ClassSubjects.Add(classSubject);
        await _context.SaveChangesAsync();
        _logger.LogInformation("ClassSubjectService: Created ClassSubject Id:{Id}", classSubject.Id);
        return classSubject;
    }

    /// <summary>
    /// This method deletes a ClassSubject entity from the database after removing dependent TeacherAssignments to prevent foreign key constraints.
    /// </summary>
    /// <param name="id">The ID of the ClassSubject to delete.</param>
    public async Task DeleteClassSubjectAsync(Guid id)
    {
        _logger.LogInformation("ClassSubjectService: Deleting ClassSubject Id:{Id}", id);
        var classSubject = await _context.ClassSubjects.FindAsync(id);
        if (classSubject != null)
        {
            var teacherAssignments = await _context.TeacherAssignments
                .Where(ta => ta.ClassSubjectId == id)
                .ToListAsync();

            if (teacherAssignments.Any())
            {
                _logger.LogInformation("ClassSubjectService: Removing {Count} dependent teacher assignments for ClassSubject Id:{Id}", teacherAssignments.Count, id);
                _context.TeacherAssignments.RemoveRange(teacherAssignments);
            }

            _context.ClassSubjects.Remove(classSubject);
            await _context.SaveChangesAsync();
            _logger.LogInformation("ClassSubjectService: Deleted ClassSubject Id:{Id}", id);
        }
        else
        {
            _logger.LogWarning("ClassSubjectService: ClassSubject Id:{Id} not found for deletion", id);
        }
    }
}