namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.UserDTOs;
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

    /// <summary>
    /// This method creates a new subject in the database based on the provided SubjectCreateDto.
    /// </summary>
    public async Task<Subject> CreateSubjectAsync(SubjectCreateDto dto)
    {
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    /// <summary>
    /// This method updates an existing subject in the database based on the provided SubjectUpdateDto.
    /// </summary>
    public async Task UpdateSubjectAsync(Guid id, SubjectUpdateDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return;

        if (dto.Name != null) subject.Name = dto.Name;
        if (dto.Code != null) subject.Code = dto.Code;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// This method deletes a subject from the database based on the provided subject ID.
    /// </summary>
    public async Task DeleteSubjectAsync(Guid id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject != null)
        {
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// This method retrieves a paginated list of subjects with their linked classes populated from ClassSubjects.
    /// </summary>
    public async Task<PagedResultDto<SubjectResponseDto>> GetSubjectsAsync(SubjectFilterDto filterDto)
    {
        var query = _context.Subjects.AsQueryable();

        if (!string.IsNullOrEmpty(filterDto.Name))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{filterDto.Name}%"));

        if (!string.IsNullOrEmpty(filterDto.Code))
            query = query.Where(s => EF.Functions.ILike(s.Code, $"%{filterDto.Code}%"));

        if (filterDto.ClassId.HasValue && filterDto.ClassId.Value != Guid.Empty)
        {
            query = query.Where(s => _context.ClassSubjects.Any(cs => cs.SubjectId == s.Id && cs.ClassId == filterDto.ClassId.Value));
        }

        bool isDesc = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy = filterDto.SortBy?.ToLower().Trim() ?? "name";

        query = sortBy switch
        {
            "code" => isDesc ? query.OrderByDescending(s => s.Code) : query.OrderBy(s => s.Code),
            _ => isDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name)
        };

        var totalCount = await query.CountAsync();

        var subjects = await query
            .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .Select(s => new SubjectResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                LinkedClasses = _context.ClassSubjects
                    .Where(cs => cs.SubjectId == s.Id)
                    .Select(cs => new ClassSummaryDto
                    {
                        Id = cs.Class.Id,
                        Name = cs.Class.Name,
                        Section = cs.Class.Section,
                        AcademicYear = cs.Class.AcademicYear
                    })
                    .ToList()
            })
            .ToListAsync();

        return new PagedResultDto<SubjectResponseDto>
        {
            Items = subjects,
            TotalCount = totalCount,
            PageNumber = filterDto.PageNumber,
            PageSize = filterDto.PageSize
        };
    }
}