namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public AssignmentService(AppDbContext context, IHttpContextAccessor httpContextAccessor, IUserService userService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
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
    /// This method creates a new assignment based on the provided AssignmentCreateDto. It initializes a new Assignment entity, sets its properties from the DTO, and saves it to the database. The method returns the created AssignmentResponseDto entity.
    /// </summary>
    /// <param name="dto">The data transfer object containing the assignment details.</param>
    /// <returns>The created AssignmentResponseDto entity.</returns>
    public async Task<AssignmentResponseDto> CreateAssignmentAsync(AssignmentCreateDto dto)
    {
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = userClaims.UserId,
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

        AssignmentResponseDto assignmentResponseDto = await AssignmentToAssignmentResponseDto(assignment);

        return assignmentResponseDto;
    }


    /// <summary>
    /// This method updates an existing assignment based on the provided AssignmentUpdateDto. It retrieves the assignment by its ID, updates its properties with the values from the DTO (if they are not null), and saves the changes to the database. If the assignment is not found, the method throws a KeyNotFoundException.
    /// </summary>
    /// <param name="id">The ID of the assignment to update.</param>
    /// <param name="dto">The data transfer object containing the updated assignment details.</param>
    /// <returns>The updated AssignmentResponseDto.</returns>
    public async Task<AssignmentResponseDto> UpdateAssignmentAsync(Guid id, AssignmentUpdateDto dto)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null) throw new KeyNotFoundException($"Assignment with ID {id} not found.");

        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(assignment.TeacherId != userClaims.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this assignment.");
        }


        if (dto.Title != null) assignment.Title = dto.Title;
        if (dto.Description != null) assignment.Description = dto.Description;
        if (dto.Deadline != null) assignment.Deadline = dto.Deadline.Value;
        if (dto.MaxMarks != null) assignment.MaxMarks = dto.MaxMarks.Value;
        if (dto.Status != null) assignment.Status = dto.Status.Value;
        if (dto.AllowLateSubmission != null) assignment.AllowLateSubmission = dto.AllowLateSubmission.Value;
        if (dto.AllowResubmission != null) assignment.AllowResubmission = dto.AllowResubmission.Value;

        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        AssignmentResponseDto assignmentResponseDto = await AssignmentToAssignmentResponseDto(assignment);

        return assignmentResponseDto;
    }



    /// <summary>
    /// This method deletes an assignment based on the provided assignment ID. It retrieves the assignment from the database, and if found, removes it and saves the changes. If the assignment is not found, the method simply returns without making any changes.
    /// </summary>
    /// <param name="id">The ID of the assignment to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAssignmentAsync(Guid id)
    {
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        var assignment = await _context.Assignments.FindAsync(id);

        if(assignment != null && assignment.TeacherId != userClaims.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this assignment.");
        }

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



    /// <summary>
    /// This method retrieves the total count of classes assigned to a specific teacher based on their teacher ID. It queries the Assignments table in the database and counts the number of assignments associated with the given teacher ID.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher for whom to count the assigned classes.</param>
    /// <returns>The total count of classes assigned to the specified teacher.</returns>
    public async Task<int> GetTotalAssignedClassesCount(Guid teacherId)
    {
        return await _context.Assignments.CountAsync(a => a.TeacherId == teacherId);
    }



    /// <summary>
    /// This method retrieves the count of active assignments for a specific teacher based on their teacher ID. It queries the Assignments table in the database and counts the number of assignments associated with the given teacher ID that have a status of "Active."
    /// </summary>
    /// <param name="teacherId">The ID of the teacher for whom to count the active assignments.</param>
    /// <returns>The count of active assignments for the specified teacher.</returns>
    public async Task<int> GetActiveAssignmentsCount(Guid teacherId)
    {
        return await _context.Assignments.CountAsync(a => a.TeacherId == teacherId && a.Status == AssignmentStatus.Published);
    }



    /// <summary>
    /// This method retrieves a list of upcoming assignment deadlines for a specific teacher based on their teacher ID. It queries the Assignments table in the database and selects assignments that have a deadline within the next three days. The method returns a list of TeacherUpcomingDeadlineDto objects containing relevant information about each upcoming assignment.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher for whom to retrieve upcoming assignment deadlines.</param>
    /// <returns>A list of TeacherUpcomingDeadlineDto objects representing upcoming assignment deadlines.</returns>
    public async Task<List<TeacherUpcomingDeadlineDto>> GetUpcomingDeadlines(Guid teacherId)
    {
        return await _context.Assignments
            .Where(a => a.TeacherId == teacherId && a.Deadline > DateTime.UtcNow && a.Deadline <= DateTime.UtcNow.AddDays(3))
            .OrderBy(a => a.Deadline)
            .Select(a => new TeacherUpcomingDeadlineDto
            {
                AssignmentId = a.Id.ToString(),
                Title = a.Title,
                ClassName = a.Class.Name,
                SubjectName = a.Subject.Name,
                SubjectCode = a.Subject.Code,
                DueDate = a.Deadline,
                TotalSubmissions = _context.Submissions.Count(s => s.AssignmentId == a.Id),
                UngradedSubmissions = _context.Submissions.Count(s => s.AssignmentId == a.Id && s.Marks == null)
            })
            .ToListAsync();
    }



    /// <summary>
    /// This method retrieves a paginated list of assignments for a specific teacher based on the provided filter criteria. It extracts the teacher's user ID from the HTTP context, applies filters such as title, class name, teacher name, teacher email, and status, and returns a PagedResultDto containing the filtered assignments along with pagination information. If the user ID claim is not found, an UnauthorizedAccessException is thrown.
    /// </summary>
    /// <param name="dto">The filter criteria for retrieving assignments.</param>
    /// <returns>A PagedResultDto containing the filtered assignments and pagination information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID claim is not found in the HTTP context.</exception>
    public async Task<PagedResultDto<AssignmentResponseDto>> GetAssignmentsForTeacher(AssignmentFilterDto dto)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID claim not found.");
        var userRoles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList() ?? new List<string>(); 
        var query = _context.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Where(a => a.TeacherId == Guid.Parse(userIdClaim.Value));

        if (!string.IsNullOrEmpty(dto.Title))
        {
            query = query.Where(a => EF.Functions.Like(a.Title, $"%{dto.Title}%"));
        }
        if (!string.IsNullOrEmpty(dto.ClassName))
        {
            query = query.Where(a => EF.Functions.Like(a.Class.Name, $"%{dto.ClassName}%"));
        }
        if (!string.IsNullOrEmpty(dto.TeacherName))
        {
            query = query.Where(a => EF.Functions.Like(a.Teacher.FullName, $"%{dto.TeacherName}%"));
        }
        if (!string.IsNullOrEmpty(dto.TeacherEmail))
        {
            query = query.Where(a => EF.Functions.Like(a.Teacher.Email, $"%{dto.TeacherEmail}%"));
        }
        if (!string.IsNullOrEmpty(dto.Status))
        {
            if (Enum.TryParse<AssignmentStatus>(dto.Status, true, out var status))
            {
                query = query.Where(a => a.Status == status);
            }
        }
        var totalCount = await query.CountAsync();
        var assignments = await query
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync();

        var assignmentDtos = assignments.Select(a => new AssignmentResponseDto
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
        }).ToList();

        return new PagedResultDto<AssignmentResponseDto>
        {
            Items = assignmentDtos,
            TotalCount = totalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }




    /// <summary>
    /// This method converts an Assignment entity to an AssignmentResponseDto. It retrieves related entities such as Class, Subject, and Teacher from the database and populates the corresponding fields in the DTO. The method returns the populated AssignmentResponseDto.
    /// </summary>
    /// <param name="assignment">The Assignment entity to convert.</param>
    /// <returns>The populated AssignmentResponseDto.</returns>
    public async Task<AssignmentResponseDto> AssignmentToAssignmentResponseDto(Assignment assignment)
    {
        var classEntity = await _context.Classes.FindAsync(assignment.ClassId);
        var subjectEntity = await _context.Subjects.FindAsync(assignment.SubjectId);
        var teacherEntity = await _context.Users.FindAsync(assignment.TeacherId);
        return new AssignmentResponseDto
        {
            Id = assignment.Id,
            Title = assignment.Title,
            Description = assignment.Description,
            ClassName = classEntity?.Name ?? string.Empty,
            ClassSection = classEntity?.Section ?? string.Empty,
            AcademicYear = classEntity?.AcademicYear ?? string.Empty,
            SubjectName = subjectEntity?.Name ?? string.Empty,
            SubjectCode = subjectEntity?.Code ?? string.Empty,
            TeacherName = teacherEntity?.FullName ?? string.Empty,
            TeacherEmail = teacherEntity?.Email ?? string.Empty,
            DueDate = assignment.Deadline,
            CreatedAt = assignment.CreatedAt,
            MaxMarks = assignment.MaxMarks,
            Status = assignment.Status.ToString(),
            TotalSubmissions = await _context.Submissions.CountAsync(s => s.AssignmentId == assignment.Id),
            AllowLateSubmission = assignment.AllowLateSubmission,
            AllowResubmission = assignment.AllowResubmission
        };
    }
}