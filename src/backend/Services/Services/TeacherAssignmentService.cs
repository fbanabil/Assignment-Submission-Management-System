namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly AppDbContext _context;

    public TeacherAssignmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeacherAssignment>> GetAllTeacherAssignmentsAsync() =>
        await _context.TeacherAssignments.Include(ta => ta.Teacher).Include(ta => ta.ClassSubject).ToListAsync();

    public async Task<TeacherAssignment?> GetTeacherAssignmentByIdAsync(Guid id) =>
        await _context.TeacherAssignments.FindAsync(id);




    /// <summary>
    /// This method creates a new teacher assignment based on the provided TeacherAssignmentCreateDto. It initializes a new TeacherAssignment entity with the specified teacher ID and class subject ID, adds it to the database context, and saves the changes asynchronously. The created TeacherAssignment entity is then returned.
    /// </summary>
    /// <param name="dto">The data transfer object containing the teacher assignment details.</param>
    /// <returns>The created TeacherAssignment entity.</returns>
    public async Task<TeacherAssignment> CreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto)
    {
        var assignment = new TeacherAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = dto.TeacherId,
            ClassSubjectId = dto.ClassSubjectId
        };

        _context.TeacherAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }



    /// <summary>
    /// This method deletes a teacher assignment based on the provided ID. It first retrieves the TeacherAssignment entity from the database using the specified ID. If the assignment exists, it removes it from the database context and saves the changes asynchronously. If the assignment does not exist, no action is taken.
    /// </summary>
    /// <param name="id">The ID of the teacher assignment to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteTeacherAssignmentAsync(Guid id)
    {
        var assignment = await _context.TeacherAssignments.FindAsync(id);
        if (assignment != null)
        {
            _context.TeacherAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }




    /// <summary>
    /// This method retrieves a paginated list of teacher assignments based on the provided filter criteria. It allows filtering by teacher name, class name, subject code, and teacher email. The results are returned in a PagedResultDto containing the filtered items, total count, page number, and page size.
    /// </summary>
    /// <param name="dto">The filter criteria and pagination information.</param>
    /// <returns>A PagedResultDto containing the filtered teacher assignments.</returns>
    public async Task<PagedResultDto<TeacherAssignmentResponseDto>> GetTeacherAssignmentsAsync(TeacherAssignmentFilterDto dto)
    {
        var query = _context.TeacherAssignments
            .Include(ta => ta.Teacher)
            .Include(ta => ta.ClassSubject)
            .AsQueryable();
        if (!string.IsNullOrEmpty(dto.TeacherName))
        {
            query = query.Where(ta => EF.Functions.Like(ta.Teacher.FullName, $"%{dto.TeacherName}%"));
        }
        if (!string.IsNullOrEmpty(dto.ClassName))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Class.Name, $"%{dto.ClassName}%"));
        }
        if (!string.IsNullOrEmpty(dto.SubjectCode))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Subject.Code, $"%{dto.SubjectCode}%"));
        }
        if (!string.IsNullOrEmpty(dto.TeacherEmail))
        {
            query = query.Where(ta => EF.Functions.Like(ta.Teacher.Email, $"%{dto.TeacherEmail}%"));
        }
        if (!string.IsNullOrEmpty(dto.ClassName))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Class.Name, $"%{dto.ClassName}%"));
        }
        if (!string.IsNullOrEmpty(dto.SubjectCode))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Subject.Code, $"%{dto.SubjectCode}%"));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .Select(ta => new TeacherAssignmentResponseDto
            {
                Id = ta.Id,
                TeacherName = ta.Teacher.FullName,
                TeacherEmail = ta.Teacher.Email,
                ClassName = ta.ClassSubject.Class.Name,
                ClassSection = ta.ClassSubject.Class.Section,
                AcademicYear = ta.ClassSubject.Class.AcademicYear,
                SubjectName = ta.ClassSubject.Subject.Name,
                SubjectCode = ta.ClassSubject.Subject.Code,
                AssignedAt = DateTime.UtcNow 
            })
            .ToListAsync();
        return new PagedResultDto<TeacherAssignmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }
}