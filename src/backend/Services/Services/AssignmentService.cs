namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.StudentDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;

using Backend.Middlewares;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(AppDbContext context, IHttpContextAccessor httpContextAccessor, IUserService userService, ILogger<AssignmentService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _logger = logger;
    }

    public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
    {
        _logger.LogInformation("AssignmentService: Fetching all assignments");
        return await _context.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .ToListAsync();
    }

    public async Task<Assignment?> GetAssignmentByIdAsync(Guid id)
    {
        _logger.LogInformation("AssignmentService: Fetching assignment by Id:{Id}", id);
        return await _context.Assignments.FindAsync(id);
    }




    /// <summary>
    /// This method creates a new assignment based on the provided AssignmentCreateDto. It initializes a new Assignment entity, sets its properties from the DTO, and saves it to the database. The method returns the created AssignmentResponseDto entity.
    /// </summary>
    /// <param name="dto">The data transfer object containing the assignment details.</param>
    /// <returns>The created AssignmentResponseDto entity.</returns>
    public async Task<AssignmentResponseDto> CreateAssignmentAsync(AssignmentCreateDto dto)
    {
        _logger.LogInformation("AssignmentService: Creating assignment with Title:{Title}", dto.Title);
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

        _logger.LogInformation("AssignmentService: Created assignment Id:{Id}", assignment.Id);
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
        _logger.LogInformation("AssignmentService: Updating assignment Id:{Id}", id);
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null)
        {
            _logger.LogWarning("AssignmentService: Assignment Id:{Id} not found", id);
            throw new NotFoundException($"Assignment with ID {id} not found.");
        }

        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(assignment.TeacherId != userClaims.UserId)
        {
            _logger.LogWarning("AssignmentService: User {UserId} unauthorized to update assignment Id:{Id}", userClaims.UserId, id);
            throw new ForbiddenException("You are not authorized to update this assignment.");
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
        _logger.LogInformation("AssignmentService: Updated assignment Id:{Id}", id);

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
        _logger.LogInformation("AssignmentService: Deleting assignment Id:{Id}", id);
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        var assignment = await _context.Assignments.FindAsync(id);

        if(assignment != null && assignment.TeacherId != userClaims.UserId)
        {
            _logger.LogWarning("AssignmentService: User {UserId} unauthorized to delete assignment Id:{Id}", userClaims.UserId, id);
            throw new ForbiddenException("You are not authorized to delete this assignment.");
        }


        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("AssignmentService: Deleted assignment Id:{Id}", id);
        }
        else
        {
            _logger.LogWarning("AssignmentService: Assignment Id:{Id} not found for deletion", id);
        }
    }



    /// <summary>
    /// This method retrieves a summary of assignments, including total assignments, active assignments, draft assignments, due soon assignments, completion rate, and a breakdown of assignments by status.
    /// </summary>
    /// <returns>An AssignmentSummaryDto containing the summary of assignments.</returns>
    public async Task<AssignmentSummaryDto> GetAssignmentSummaryAsync()
    {
        _logger.LogInformation("AssignmentService: Querying assignment summary metrics");
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
        _logger.LogInformation("AssignmentService: Querying paged assignments");
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

        bool isDesc = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy = filterDto.SortBy?.ToLower().Trim() ?? "duedate";

        query = sortBy switch
        {
            "title" => isDesc ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "classname" => isDesc ? query.OrderByDescending(a => a.Class.Name) : query.OrderBy(a => a.Class.Name),
            "subjectname" => isDesc ? query.OrderByDescending(a => a.Subject.Name) : query.OrderBy(a => a.Subject.Name),
            "teachername" => isDesc ? query.OrderByDescending(a => a.Teacher.FullName) : query.OrderBy(a => a.Teacher.FullName),
            "status" => isDesc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "createdat" => isDesc ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => isDesc ? query.OrderByDescending(a => a.Deadline) : query.OrderBy(a => a.Deadline)
        };

        var items = await query
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
                AllowLateSubmission = a.AllowLateSubmission,
                AllowResubmission = a.AllowResubmission
            })
            .ToListAsync();

        var totalCount = await query.CountAsync();
        _logger.LogInformation("AssignmentService: Retrieved {Count} assignments matching filter", totalCount);
        return new PagedResultDto<AssignmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filterDto.PageNumber,
            PageSize = filterDto.PageSize
        };
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
        _logger.LogInformation("AssignmentService: Fetching assignments for teacher");
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedException("User ID claim not found.");
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

        bool isDesc = dto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy = dto.SortBy?.ToLower().Trim() ?? "duedate";

        query = sortBy switch
        {
            "title" => isDesc ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "classname" => isDesc ? query.OrderByDescending(a => a.Class.Name) : query.OrderBy(a => a.Class.Name),
            "subjectname" => isDesc ? query.OrderByDescending(a => a.Subject.Name) : query.OrderBy(a => a.Subject.Name),
            "teachername" => isDesc ? query.OrderByDescending(a => a.Teacher.FullName) : query.OrderBy(a => a.Teacher.FullName),
            "status" => isDesc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "createdat" => isDesc ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => isDesc ? query.OrderByDescending(a => a.Deadline) : query.OrderBy(a => a.Deadline)
        };

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
            AllowLateSubmission = a.AllowLateSubmission,
            AllowResubmission = a.AllowResubmission
        }).ToList();

        _logger.LogInformation("AssignmentService: Retrieved {Count} assignments for teacher", totalCount);
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



    /// <summary>
    /// This method retrieves a list of assignments for the specified class IDs. It queries the Assignments table in the database, including related Class and Subject entities, and filters the assignments based on the provided list of enrolled class IDs. The method returns a list of Assignment entities that belong to the specified classes.
    /// </summary>
    /// <param name="enrolledClassIds">A list of class IDs for which to retrieve assignments.</param>
    /// <returns>A list of Assignment entities that belong to the specified classes.</returns>
    public async Task<List<Assignment>?> GetAssignmentsForClassesAsync(List<Guid> enrolledClassIds)
    {
        _logger.LogInformation("AssignmentService: Querying assignments for {Count} enrolled classes", enrolledClassIds.Count);
        var assignmentsQuery = _context.Assignments
                .Include(a => a.Class)
                .Include(a => a.Subject)
                .Where(a => enrolledClassIds.Contains(a.ClassId));

        var allAssignments = await assignmentsQuery.ToListAsync();

        return allAssignments;
    }

    public async Task<PagedResultDto<StudentAssignmentResponseDto>> GetAssignmentsForStudentPagedAsync(Guid studentId, StudentAssignmentFilterDto filterDto)
    {
        _logger.LogInformation("AssignmentService: Querying paged assignments for StudentId:{StudentId}", studentId);
        // Get class IDs the student is enrolled in
        var enrolledClassIds = await _context.StudentEnrollments
            .Where(se => se.StudentId == studentId)
            .Select(se => se.ClassId)
            .ToListAsync();

        // Base query for active published assignments in enrolled classes
        var assignments = await _context.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Where(a => enrolledClassIds.Contains(a.ClassId))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        // Submissions by student
        var submissions = await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .ToDictionaryAsync(s => s.AssignmentId);

        // Map & compute status
        var resultList = new List<StudentAssignmentResponseDto>();

        foreach (var a in assignments)
        {
            submissions.TryGetValue(a.Id, out var sub);
            string status = "Pending";
            if (sub != null)
            {
                status = sub.Marks.HasValue ? "Graded" : "Submitted";
            }
            else if (a.Deadline < DateTime.UtcNow)
            {
                status = "Overdue";
            }

            // Apply Status Filter
            if (!string.IsNullOrWhiteSpace(filterDto.StatusFilter) && !filterDto.StatusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (filterDto.StatusFilter.Equals("Pending", StringComparison.OrdinalIgnoreCase) && status != "Pending" && status != "Overdue") continue;
                if (filterDto.StatusFilter.Equals("Submitted", StringComparison.OrdinalIgnoreCase) && status != "Submitted") continue;
                if (filterDto.StatusFilter.Equals("Graded", StringComparison.OrdinalIgnoreCase) && status != "Graded") continue;
            }

            // Apply Search Filter
            if (!string.IsNullOrWhiteSpace(filterDto.Search))
            {
                var term = filterDto.Search.ToLower();
                if (!a.Title.ToLower().Contains(term) && !a.Subject!.Name.ToLower().Contains(term) && !a.Class!.Name.ToLower().Contains(term))
                {
                    continue;
                }
            }

            resultList.Add(new StudentAssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ClassName = a.Class?.Name ?? "Class",
                SubjectName = a.Subject?.Name ?? "Subject",
                SubjectCode = a.Subject?.Code ?? "SUB",
                TeacherName = a.Teacher?.FullName ?? "Teacher",
                Deadline = a.Deadline,
                MaxMarks = a.MaxMarks,
                Status = status,
                SubmittedAt = sub?.SubmittedAt,
                Marks = sub?.Marks,
                Feedback = sub?.Feedback
            });
        }

        int totalCount = resultList.Count;
        int pageNumber = Math.Max(1, filterDto.PageNumber);
        int pageSize = Math.Max(1, filterDto.PageSize);

        var pagedItems = resultList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("AssignmentService: Found {TotalCount} assignments for StudentId:{StudentId}", totalCount, studentId);
        return new PagedResultDto<StudentAssignmentResponseDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<StudentAssignmentDetailDto?> GetAssignmentDetailForStudentAsync(Guid studentId, Guid assignmentId)
    {
        _logger.LogInformation("AssignmentService: Fetching assignment detail for AssignmentId:{AssignmentId}, StudentId:{StudentId}", assignmentId, studentId);
        var assignment = await _context.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null)
        {
            _logger.LogWarning("AssignmentService: Assignment Id:{AssignmentId} not found", assignmentId);
            return null;
        }

        var submission = await _context.Submissions
            .Include(s => s.GradeGiver)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        string status = "Pending";
        StudentSubmissionDetailDto? subDetail = null;

        if (submission != null)
        {
            status = submission.Marks.HasValue ? "Graded" : "Submitted";
            subDetail = new StudentSubmissionDetailDto
            {
                SubmissionId = submission.Id,
                SubmissionText = submission.SubmissionText,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt,
                Marks = submission.Marks,
                Feedback = submission.Feedback,
                GradedAt = submission.GradedAt,
                GradedByTeacherName = submission.GradeGiver?.FullName
            };
        }
        else if (assignment.Deadline < DateTime.UtcNow)
        {
            status = "Overdue";
        }

        return new StudentAssignmentDetailDto
        {
            Id = assignment.Id,
            Title = assignment.Title,
            Description = assignment.Description,
            ClassId = assignment.ClassId,
            ClassName = assignment.Class?.Name ?? "",
            ClassSection = assignment.Class?.Section ?? "",
            SubjectId = assignment.SubjectId,
            SubjectName = assignment.Subject?.Name ?? "",
            SubjectCode = assignment.Subject?.Code ?? "",
            TeacherName = assignment.Teacher?.FullName ?? "",
            TeacherEmail = assignment.Teacher?.Email ?? "",
            Deadline = assignment.Deadline,
            MaxMarks = assignment.MaxMarks,
            AllowLateSubmission = assignment.AllowLateSubmission,
            AllowResubmission = assignment.AllowResubmission,
            Status = status,
            ExistingSubmission = subDetail
        };
    }
}