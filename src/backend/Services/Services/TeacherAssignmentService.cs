namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TeacherAssignmentService> _logger;

    public TeacherAssignmentService(AppDbContext context, ILogger<TeacherAssignmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TeacherAssignment>> GetAllTeacherAssignmentsAsync()
    {
        _logger.LogInformation("TeacherAssignmentService: Fetching all teacher assignments");
        return await _context.TeacherAssignments.Include(ta => ta.Teacher).Include(ta => ta.ClassSubject).ToListAsync();
    }

    public async Task<TeacherAssignment?> GetTeacherAssignmentByIdAsync(Guid id)
    {
        _logger.LogInformation("TeacherAssignmentService: Fetching teacher assignment by Id:{Id}", id);
        return await _context.TeacherAssignments.FindAsync(id);
    }

    /// <summary>
    /// This method creates a new teacher assignment based on the provided TeacherAssignmentCreateDto.
    /// If ClassSubjectId is not provided or empty, it automatically resolves or creates the ClassSubject link from ClassId and SubjectId.
    /// </summary>
    /// <param name="dto">The data transfer object containing the teacher assignment details.</param>
    /// <returns>The created TeacherAssignment entity.</returns>
    public async Task<TeacherAssignment> CreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto)
    {
        _logger.LogInformation("TeacherAssignmentService: Creating teacher assignment for TeacherId:{TeacherId}", dto.TeacherId);
        Guid classSubjectId = dto.ClassSubjectId;

        var exists = await _context.TeacherAssignments.AnyAsync(ta => ta.TeacherId == dto.TeacherId && ta.ClassSubjectId == classSubjectId);


        if ((classSubjectId == Guid.Empty) && dto.ClassId.HasValue && dto.SubjectId.HasValue && dto.ClassId.Value != Guid.Empty && dto.SubjectId.Value != Guid.Empty)
        {
            var existingCs = await _context.ClassSubjects.FirstOrDefaultAsync(cs => cs.ClassId == dto.ClassId.Value && cs.SubjectId == dto.SubjectId.Value);
            if (existingCs != null)
            {
                classSubjectId = existingCs.Id;

                var assignmentExists = await _context.TeacherAssignments.AnyAsync(ta => ta.TeacherId == dto.TeacherId && ta.ClassSubjectId == classSubjectId);
                if (assignmentExists)
                {
                    _logger.LogWarning("TeacherAssignmentService: Teacher assignment already exists for TeacherId:{TeacherId}, ClassSubjectId:{ClassSubjectId}", dto.TeacherId, classSubjectId);
                    throw new BadRequestException($"This teacher assigment already exists");
                }

            }
            else
            {
                var newCs = new ClassSubject
                {
                    Id = Guid.NewGuid(),
                    ClassId = dto.ClassId.Value,
                    SubjectId = dto.SubjectId.Value
                };
                _context.ClassSubjects.Add(newCs);
                await _context.SaveChangesAsync();
                classSubjectId = newCs.Id;
            }
        }

        var assignment = new TeacherAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = dto.TeacherId,
            ClassSubjectId = classSubjectId
        };

        _context.TeacherAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("TeacherAssignmentService: Created teacher assignment Id:{Id}", assignment.Id);
        return assignment;
    }

    /// <summary>
    /// This method deletes a teacher assignment based on the provided ID.
    /// </summary>
    /// <param name="id">The ID of the teacher assignment to delete.</param>
    public async Task DeleteTeacherAssignmentAsync(Guid id)
    {
        _logger.LogInformation("TeacherAssignmentService: Deleting teacher assignment Id:{Id}", id);
        var assignment = await _context.TeacherAssignments.FindAsync(id);
        if (assignment != null)
        {
            _context.TeacherAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("TeacherAssignmentService: Deleted teacher assignment Id:{Id}", id);
        }
        else
        {
            _logger.LogWarning("TeacherAssignmentService: Teacher assignment Id:{Id} not found for deletion", id);
        }
    }

    /// <summary>
    /// This method retrieves a paginated list of teacher assignments based on the provided filter criteria.
    /// </summary>
    /// <param name="dto">The filter criteria and pagination information.</param>
    /// <returns>A PagedResultDto containing the filtered teacher assignments.</returns>
    public async Task<PagedResultDto<TeacherAssignmentResponseDto>> GetTeacherAssignmentsAsync(TeacherAssignmentFilterDto dto)
    {
        _logger.LogInformation("TeacherAssignmentService: Querying teacher assignments");
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

        bool isDesc1 = dto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy1 = dto.SortBy?.ToLower().Trim() ?? "teachername";

        query = sortBy1 switch
        {
            "classname" => isDesc1 ? query.OrderByDescending(ta => ta.ClassSubject.Class.Name) : query.OrderBy(ta => ta.ClassSubject.Class.Name),
            "subjectcode" => isDesc1 ? query.OrderByDescending(ta => ta.ClassSubject.Subject.Code) : query.OrderBy(ta => ta.ClassSubject.Subject.Code),
            "subjectname" => isDesc1 ? query.OrderByDescending(ta => ta.ClassSubject.Subject.Name) : query.OrderBy(ta => ta.ClassSubject.Subject.Name),
            "teacheremail" => isDesc1 ? query.OrderByDescending(ta => ta.Teacher.Email) : query.OrderBy(ta => ta.Teacher.Email),
            _ => isDesc1 ? query.OrderByDescending(ta => ta.Teacher.FullName) : query.OrderBy(ta => ta.Teacher.FullName)
        };

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

        _logger.LogInformation("TeacherAssignmentService: Retrieved {Count} teacher assignments matching filter", totalCount);
        return new PagedResultDto<TeacherAssignmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    /// <summary>
    /// This method retrieves a list of classes assigned to a specific teacher based on the provided teacher ID.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher for whom to retrieve assigned classes.</param>
    /// <returns>A list of TeacherAssignedClassSubjectDto objects representing the assigned classes.</returns>
    public async Task<List<TeacherAssignedClassSubjectDto>> GetAssignedClasses(Guid teacherId)
    {
        _logger.LogInformation("TeacherAssignmentService: Querying assigned classes for TeacherId:{TeacherId}", teacherId);
        var assignedClasses = await _context.TeacherAssignments
            .Where(ta => ta.TeacherId == teacherId)
            .Include(ta => ta.ClassSubject)
                .ThenInclude(cs => cs.Class)
            .Include(ta => ta.ClassSubject)
                .ThenInclude(cs => cs.Subject)
            .Select(ta => new TeacherAssignedClassSubjectDto
            {
                ClassId = ta.ClassSubject.Class.Id,
                SubjectId = ta.ClassSubject.Subject.Id,
                ClassSubjectId = ta.ClassSubjectId.ToString(),
                ClassName = ta.ClassSubject.Class.Name,
                ClassSection = ta.ClassSubject.Class.Section,
                AcademicYear = ta.ClassSubject.Class.AcademicYear,
                SubjectName = ta.ClassSubject.Subject.Name,
                SubjectCode = ta.ClassSubject.Subject.Code,
                StudentCount = _context.ClassSubjects
                    .Where(cs => cs.Id == ta.ClassSubjectId)
                    .SelectMany(cs => cs.Class.StudentEnrollments)
                    .Count()
            })
            .ToListAsync();

        _logger.LogInformation("TeacherAssignmentService: Found {Count} assigned classes for TeacherId:{TeacherId}", assignedClasses.Count, teacherId);
        return assignedClasses;
    }

    /// <summary>
    /// This method retrieves a paginated and filtered list of classes assigned to a specific teacher.
    /// </summary>
    public async Task<PagedResultDto<TeacherAssignedClassSubjectDto>> GetAssignedClassesPagedAsync(Guid teacherId, TeacherClassFilterDto filterDto)
    {
        _logger.LogInformation("TeacherAssignmentService: Querying paged assigned classes for TeacherId:{TeacherId}", teacherId);
        var query = _context.TeacherAssignments
            .Where(ta => ta.TeacherId == teacherId)
            .Include(ta => ta.ClassSubject)
                .ThenInclude(cs => cs.Class)
            .Include(ta => ta.ClassSubject)
                .ThenInclude(cs => cs.Subject)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterDto.ClassName))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Class.Name, $"%{filterDto.ClassName}%"));
        }
        if (!string.IsNullOrEmpty(filterDto.ClassSection))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Class.Section, $"%{filterDto.ClassSection}%"));
        }
        if (!string.IsNullOrEmpty(filterDto.AcademicYear))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Class.AcademicYear, $"%{filterDto.AcademicYear}%"));
        }
        if (!string.IsNullOrEmpty(filterDto.SubjectName))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Subject.Name, $"%{filterDto.SubjectName}%"));
        }
        if (!string.IsNullOrEmpty(filterDto.SubjectCode))
        {
            query = query.Where(ta => EF.Functions.Like(ta.ClassSubject.Subject.Code, $"%{filterDto.SubjectCode}%"));
        }

        bool isDesc2 = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy2 = filterDto.SortBy?.ToLower().Trim() ?? "classname";

        query = sortBy2 switch
        {
            "classsection" => isDesc2 ? query.OrderByDescending(ta => ta.ClassSubject.Class.Section) : query.OrderBy(ta => ta.ClassSubject.Class.Section),
            "academicyear" => isDesc2 ? query.OrderByDescending(ta => ta.ClassSubject.Class.AcademicYear) : query.OrderBy(ta => ta.ClassSubject.Class.AcademicYear),
            "subjectname" => isDesc2 ? query.OrderByDescending(ta => ta.ClassSubject.Subject.Name) : query.OrderBy(ta => ta.ClassSubject.Subject.Name),
            "subjectcode" => isDesc2 ? query.OrderByDescending(ta => ta.ClassSubject.Subject.Code) : query.OrderBy(ta => ta.ClassSubject.Subject.Code),
            _ => isDesc2 ? query.OrderByDescending(ta => ta.ClassSubject.Class.Name) : query.OrderBy(ta => ta.ClassSubject.Class.Name)
        };

        var totalCount = await query.CountAsync();
        var pageNumber = Math.Max(1, filterDto.PageNumber);
        var pageSize = Math.Max(1, filterDto.PageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ta => new TeacherAssignedClassSubjectDto
            {
                ClassId = ta.ClassSubject.Class.Id,
                SubjectId = ta.ClassSubject.Subject.Id,
                ClassSubjectId = ta.ClassSubjectId.ToString(),
                ClassName = ta.ClassSubject.Class.Name,
                ClassSection = ta.ClassSubject.Class.Section,
                AcademicYear = ta.ClassSubject.Class.AcademicYear,
                SubjectName = ta.ClassSubject.Subject.Name,
                SubjectCode = ta.ClassSubject.Subject.Code,
                StudentCount = _context.ClassSubjects
                    .Where(cs => cs.Id == ta.ClassSubjectId)
                    .SelectMany(cs => cs.Class.StudentEnrollments)
                    .Count()
            })
            .ToListAsync();

        _logger.LogInformation("TeacherAssignmentService: Retrieved {Count} paged assigned classes for TeacherId:{TeacherId}", totalCount, teacherId);

        return new PagedResultDto<TeacherAssignedClassSubjectDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}