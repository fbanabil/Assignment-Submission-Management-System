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
    /// This method creates a new subject in the database based on the provided SubjectCreateDto. It initializes a new Subject entity with the name and code from the DTO, adds it to the database context, and saves the changes asynchronously. The created Subject entity is then returned.
    /// </summary>
    /// <param name="dto">The data transfer object containing the subject details.</param>
    /// <returns>The created Subject entity.</returns>
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
    /// This method updates an existing subject in the database based on the provided SubjectUpdateDto. It first retrieves the subject by its ID, and if found, updates its name and code with the values from the DTO (if they are not null). Finally, it saves the changes to the database asynchronously.
    /// </summary>
    /// <param name="id">The ID of the subject to update.</param>
    /// <param name="dto">The data transfer object containing the updated subject details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateSubjectAsync(Guid id, SubjectUpdateDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null) return;

        if (dto.Name != null) subject.Name = dto.Name;
        if (dto.Code != null) subject.Code = dto.Code;

        await _context.SaveChangesAsync();
    }



    /// <summary>
    /// This method deletes a subject from the database based on the provided subject ID. It first retrieves the subject by its ID, and if found, removes it from the database context and saves the changes asynchronously.
    /// </summary>
    /// <param name="id">The ID of the subject to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
    /// This method retrieves a paginated list of subjects based on the provided filter criteria. It allows filtering by subject name and code, and supports pagination through page number and page size parameters.
    /// </summary>
    /// <param name="filterDto">The filter criteria for retrieving subjects.</param>
    /// <returns>A PagedResultDto containing the filtered subject data and pagination information.</returns>
    public async Task<PagedResultDto<SubjectResponseDto>> GetSubjectsAsync(SubjectFilterDto filterDto)
    {
        var query = _context.Subjects.AsQueryable();
        if (!string.IsNullOrEmpty(filterDto.Name))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{filterDto.Name}%"));
        if (!string.IsNullOrEmpty(filterDto.Code))
            query = query.Where(s => EF.Functions.ILike(s.Code, $"%{filterDto.Code}%"));
        var totalCount = await query.CountAsync();
        var subjects = await query
            .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .Select(s => new SubjectResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code
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