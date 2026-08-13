namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _context;

    public AssignmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync() =>
        await _context.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .ToListAsync();

    public async Task<Assignment?> GetAssignmentByIdAsync(Guid id) =>
        await _context.Assignments.FindAsync(id);




    /// <summary>
    /// This method creates a new assignment based on the provided AssignmentCreateDto. It initializes a new Assignment entity, sets its properties from the DTO, and saves it to the database. The method returns the created Assignment entity.
    /// </summary>
    /// <param name="dto">The data transfer object containing the assignment details.</param>
    /// <returns>The created Assignment entity.</returns>
    public async Task<Assignment> CreateAssignmentAsync(AssignmentCreateDto dto)
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            Deadline = dto.Deadline,
            MaxMarks = dto.MaxMarks,
            Status = dto.Status,
            AllowLateSubmission = dto.AllowLateSubmission,
            AllowResubmission = dto.AllowResubmission,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }


    /// <summary>
    /// This method updates an existing assignment based on the provided AssignmentUpdateDto. It retrieves the assignment by its ID, updates its properties with the values from the DTO (if they are not null), and saves the changes to the database. If the assignment is not found, the method simply returns without making any changes.
    /// </summary>
    /// <param name="id">The ID of the assignment to update.</param>
    /// <param name="dto">The data transfer object containing the updated assignment details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateAssignmentAsync(Guid id, AssignmentUpdateDto dto)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) return;

        if (dto.Title != null) assignment.Title = dto.Title;
        if (dto.Description != null) assignment.Description = dto.Description;
        if (dto.Deadline != null) assignment.Deadline = dto.Deadline.Value;
        if (dto.MaxMarks != null) assignment.MaxMarks = dto.MaxMarks.Value;
        if (dto.Status != null) assignment.Status = dto.Status.Value;
        if (dto.AllowLateSubmission != null) assignment.AllowLateSubmission = dto.AllowLateSubmission.Value;
        if (dto.AllowResubmission != null) assignment.AllowResubmission = dto.AllowResubmission.Value;

        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }



    /// <summary>
    /// This method deletes an assignment based on the provided assignment ID. It retrieves the assignment from the database, and if found, removes it and saves the changes. If the assignment is not found, the method simply returns without making any changes.
    /// </summary>
    /// <param name="id">The ID of the assignment to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAssignmentAsync(Guid id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }



    /// <summary>
    /// This method retrieves a summary of assignments, including total assignments, active assignments, draft assignments, due soon assignments, completion rate, and a breakdown of assignments by status.
    /// </summary>
    /// <returns>An AssignmentSummaryDto containing the summary of assignments.</returns>
    public async Task<AssignmentSummaryDto> GetAssignmentSummaryAsync()
    {
        AssignmentSummaryDto assignmentSummaryDto = new AssignmentSummaryDto()
        {
            TotalAssignments = await _context.Assignments.CountAsync(),
            ActiveAssignments = await _context.Assignments.CountAsync(a => a.Deadline > DateTime.UtcNow),
            DraftAssignments = await _context.Assignments.CountAsync(a => a.Status == AssignmentStatus.Draft),
            DueSoonAssignments = await _context.Assignments.CountAsync(a => a.Deadline <= DateTime.UtcNow.AddDays(3) && a.Deadline > DateTime.UtcNow),
            CompletionRate = await _context.Assignments.CountAsync(a => a.Status == AssignmentStatus.Published) * 100 / (await _context.Assignments.CountAsync() == 0 ? 1 : await _context.Assignments.CountAsync()),
            StatusBreakdown = await _context.Assignments
                .GroupBy(a => a.Status)
                .Select(g => new AssignmentStatusSummaryDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync()
        };
        return assignmentSummaryDto;
    }




    /// <summary>
    /// This method retrieves a paginated list of assignments based on the provided filter criteria, including title, class name, teacher name, teacher email, and status. It returns a PagedResultDto containing the filtered assignments along with pagination information.
    /// </summary>
    /// <param name="filterDto">The filter criteria for retrieving assignments.</param>
    /// <returns>A PagedResultDto containing the filtered assignments along with pagination information.</returns>
    public async Task<PagedResultDto<AssignmentResponseDto>> GetAssignmentsAsync(AssignmentFilterDto filterDto)
    {
        var query = _context.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .AsQueryable();


        if (!string.IsNullOrEmpty(filterDto.Title))
        {
            query = query.Where(a => EF.Functions.Like(a.Title, $"%{filterDto.Title}%"));
        }
        if (!string.IsNullOrEmpty(filterDto.ClassName))
        {
            query = query.Where(a => EF.Functions.Like(a.Class.Name, $"%{filterDto.ClassName}%"));
        }
        if(!string.IsNullOrEmpty(filterDto.TeacherName))
        {
            query = query.Where(a => EF.Functions.Like(a.Teacher.FullName, $"%{filterDto.TeacherName}%"));
        }
        if(!string.IsNullOrEmpty(filterDto.TeacherEmail))
        {
            query = query.Where(a => EF.Functions.Like(a.Teacher.Email, $"%{filterDto.TeacherEmail}%"));
        }
        if(!string.IsNullOrEmpty(filterDto.Status))
        {
            if (Enum.TryParse<AssignmentStatus>(filterDto.Status, true, out var status))
            {
                query = query.Where(a => a.Status == status);
            }
        }

        return await query
            .OrderBy(a => a.Deadline)
            .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ClassName = a.Class.Name,
                ClassSection = a.Class.Section,
                AcademicYear = a.Class.AcademicYear,
                SubjectName = a.Subject.Name,
                SubjectCode = a.Subject.Code,
                TeacherName = a.Teacher.FullName,
                TeacherEmail = a.Teacher.Email,
                DueDate = a.Deadline,
                CreatedAt = a.CreatedAt,
                MaxMarks = a.MaxMarks,
                Status = a.Status.ToString(),
                TotalSubmissions = _context.Submissions.Count(s => s.AssignmentId == a.Id),
                AllowLateSubmission = a.AllowLateSubmission
            })
            .ToListAsync()
            .ContinueWith(t =>
            {
                var items = t.Result;
                var totalCount = query.Count();
                return new PagedResultDto<AssignmentResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filterDto.PageNumber,
                    PageSize = filterDto.PageSize
                };
            });
    }
}