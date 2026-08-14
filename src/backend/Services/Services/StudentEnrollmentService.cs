namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

using AssignmentSystem.Api.Models.Enums;

public class StudentEnrollmentService : IStudentEnrollmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentEnrollmentService> _logger;

    public StudentEnrollmentService(AppDbContext context, ILogger<StudentEnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<StudentEnrollment>> GetAllStudentEnrollmentsAsync()
    {
        _logger.LogInformation("StudentEnrollmentService: Fetching all student enrollments");
        return await _context.StudentEnrollments.Include(se => se.Student).Include(se => se.Class).ToListAsync();
    }

    public async Task<StudentEnrollment?> GetStudentEnrollmentByIdAsync(Guid id)
    {
        _logger.LogInformation("StudentEnrollmentService: Fetching enrollment by Id:{Id}", id);
        return await _context.StudentEnrollments.FindAsync(id);
    }

    /// <summary>
    /// This method creates a new student enrollment record in the database after verifying the student email and role.
    /// </summary>
    public async Task<StudentEnrollment> CreateStudentEnrollmentAsync(StudentEnrollmentCreateDto dto)
    {
        _logger.LogInformation("StudentEnrollmentService: Creating enrollment for Email:{StudentEmail}, ClassId:{ClassId}", dto.StudentEmail, dto.ClassId);
        // 1. Verify user exists by email (or StudentId fallback)
        User? student = null;
        if (!string.IsNullOrWhiteSpace(dto.StudentEmail))
        {
            student = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.StudentEmail.Trim().ToLower());
        }
        else if (dto.StudentId.HasValue && dto.StudentId.Value != Guid.Empty)
        {
            student = await _context.Users.FindAsync(dto.StudentId.Value);
        }

        if (student == null)
        {
            _logger.LogWarning("StudentEnrollmentService: Student email '{StudentEmail}' not found", dto.StudentEmail);
            throw new BadRequestException($"No user found with email '{dto.StudentEmail}'.");
        }

        // 2. Verify user has the Student role
        if (student.Role != UserRole.Student)
        {
            _logger.LogWarning("StudentEnrollmentService: User '{StudentEmail}' role is {Role}, not Student", dto.StudentEmail, student.Role);
            throw new BadRequestException($"The user with email '{dto.StudentEmail}' is assigned as a {student.Role}, not a Student. Only students can be enrolled in classes.");
        }

        // 3. Check if the student is already enrolled in the specified class
        var existingEnrollment = await _context.StudentEnrollments
            .FirstOrDefaultAsync(se => se.StudentId == student.Id && se.ClassId == dto.ClassId);

        if (existingEnrollment != null)
        {
            _logger.LogWarning("StudentEnrollmentService: Student {FullName} already enrolled in ClassId:{ClassId}", student.FullName, dto.ClassId);
            throw new BadRequestException($"Student {student.FullName} ({student.Email}) is already enrolled in this class.");
        }

        var enrollment = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            ClassId = dto.ClassId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.StudentEnrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("StudentEnrollmentService: Created enrollment Id:{Id}", enrollment.Id);
        return enrollment;
    }









    public async Task DeleteStudentEnrollmentAsync(Guid id)
    {
        _logger.LogInformation("StudentEnrollmentService: Deleting enrollment Id:{Id}", id);
        var enrollment = await _context.StudentEnrollments.FindAsync(id);
        if (enrollment != null)
        {
            _context.StudentEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("StudentEnrollmentService: Deleted enrollment Id:{Id}", id);
        }
        else
        {
            _logger.LogWarning("StudentEnrollmentService: Enrollment Id:{Id} not found for deletion", id);
        }
    }




    /// <summary>
    /// This method retrieves the list of class IDs that a specific student is enrolled in.
    /// </summary>
    /// <param name="targetStudentId">The ID of the student whose enrolled class IDs are to be retrieved.</param>
    /// <returns>A list of class IDs that the student is enrolled in.</returns>
    public async Task<List<Guid>> GetEnrolledClassIdsAsync(Guid targetStudentId)
    {
        _logger.LogInformation("StudentEnrollmentService: Fetching enrolled class IDs for StudentId:{StudentId}", targetStudentId);
        return await _context.StudentEnrollments
                .Where(e => e.StudentId == targetStudentId)
                .Select(e => e.ClassId)
                .ToListAsync();
    }

    public async Task<PagedResultDto<StudentEnrollmentResponseDto>> GetStudentEnrollmentsAsync(StudentEnrollmentFilterDto filterDto)
    {
        _logger.LogInformation("StudentEnrollmentService: Querying student enrollments");
        var query = _context.StudentEnrollments
            .Include(se => se.Student)
            .Include(se => se.Class)
            .AsQueryable();

        if (filterDto.ClassId.HasValue && filterDto.ClassId.Value != Guid.Empty)
        {
            query = query.Where(se => se.ClassId == filterDto.ClassId.Value);
        }

        if (filterDto.StudentId.HasValue && filterDto.StudentId.Value != Guid.Empty)
        {
            query = query.Where(se => se.StudentId == filterDto.StudentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.StudentName))
        {
            query = query.Where(se => se.Student.FullName.ToLower().Contains(filterDto.StudentName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filterDto.ClassName))
        {
            query = query.Where(se => se.Class.Name.ToLower().Contains(filterDto.ClassName.ToLower()));
        }

        bool isDesc1 = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy1 = filterDto.SortBy?.ToLower().Trim() ?? "enrolledat";

        query = sortBy1 switch
        {
            "studentname" => isDesc1 ? query.OrderByDescending(se => se.Student.FullName) : query.OrderBy(se => se.Student.FullName),
            "rollno" or "studentrollno" => isDesc1 ? query.OrderByDescending(se => se.Student.RollNo) : query.OrderBy(se => se.Student.RollNo),
            "classname" => isDesc1 ? query.OrderByDescending(se => se.Class.Name) : query.OrderBy(se => se.Class.Name),
            _ => isDesc1 ? query.OrderByDescending(se => se.EnrolledAt) : query.OrderBy(se => se.EnrolledAt)
        };

        int totalCount = await query.CountAsync();
        int pageNumber = Math.Max(1, filterDto.PageNumber);
        int pageSize = Math.Max(1, filterDto.PageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(se => new StudentEnrollmentResponseDto
            {
                Id = se.Id,
                StudentId = se.StudentId,
                StudentName = se.Student.FullName,
                StudentEmail = se.Student.Email,
                StudentRollNo = se.Student.RollNo,
                ClassId = se.ClassId,
                ClassName = se.Class.Name,
                ClassSection = se.Class.Section,
                AcademicYear = se.Class.AcademicYear,
                EnrolledAt = se.EnrolledAt
            })
            .ToListAsync();

        _logger.LogInformation("StudentEnrollmentService: Retrieved {Count} enrollments matching filter", totalCount);
        return new PagedResultDto<StudentEnrollmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<StudentEnrollmentResponseDto>> GetStudentEnrollmentsForTeacherAsync(Guid teacherId, StudentEnrollmentFilterDto filterDto)
    {
        _logger.LogInformation("StudentEnrollmentService: Querying enrollments for TeacherId:{TeacherId}", teacherId);
        // Get class IDs assigned to the teacher via TeacherAssignments
        var teacherClassIds = await _context.TeacherAssignments
            .Include(ta => ta.ClassSubject)
            .Where(ta => ta.TeacherId == teacherId)
            .Select(ta => ta.ClassSubject.ClassId)
            .Distinct()
            .ToListAsync();

        var query = _context.StudentEnrollments
            .Include(se => se.Student)
            .Include(se => se.Class)
            .Where(se => teacherClassIds.Contains(se.ClassId))
            .AsQueryable();

        if (filterDto.ClassId.HasValue && filterDto.ClassId.Value != Guid.Empty)
        {
            query = query.Where(se => se.ClassId == filterDto.ClassId.Value);
        }

        if (filterDto.StudentId.HasValue && filterDto.StudentId.Value != Guid.Empty)
        {
            query = query.Where(se => se.StudentId == filterDto.StudentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.StudentName))
        {
            query = query.Where(se => se.Student.FullName.ToLower().Contains(filterDto.StudentName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filterDto.ClassName))
        {
            query = query.Where(se => se.Class.Name.ToLower().Contains(filterDto.ClassName.ToLower()));
        }

        bool isDesc2 = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy2 = filterDto.SortBy?.ToLower().Trim() ?? "enrolledat";

        query = sortBy2 switch
        {
            "studentname" => isDesc2 ? query.OrderByDescending(se => se.Student.FullName) : query.OrderBy(se => se.Student.FullName),
            "rollno" or "studentrollno" => isDesc2 ? query.OrderByDescending(se => se.Student.RollNo) : query.OrderBy(se => se.Student.RollNo),
            "classname" => isDesc2 ? query.OrderByDescending(se => se.Class.Name) : query.OrderBy(se => se.Class.Name),
            _ => isDesc2 ? query.OrderByDescending(se => se.EnrolledAt) : query.OrderBy(se => se.EnrolledAt)
        };

        int totalCount2 = await query.CountAsync();
        int pageNumber2 = Math.Max(1, filterDto.PageNumber);
        int pageSize2 = Math.Max(1, filterDto.PageSize);

        var items2 = await query
            .Skip((pageNumber2 - 1) * pageSize2)
            .Take(pageSize2)
            .Select(se => new StudentEnrollmentResponseDto
            {
                Id = se.Id,
                StudentId = se.StudentId,
                StudentName = se.Student.FullName,
                StudentEmail = se.Student.Email,
                StudentRollNo = se.Student.RollNo,
                ClassId = se.ClassId,
                ClassName = se.Class.Name,
                ClassSection = se.Class.Section,
                AcademicYear = se.Class.AcademicYear,
                EnrolledAt = se.EnrolledAt
            })
            .ToListAsync();

        _logger.LogInformation("StudentEnrollmentService: Retrieved {Count} enrollments for TeacherId:{TeacherId}", totalCount2, teacherId);

        return new PagedResultDto<StudentEnrollmentResponseDto>
        {
            Items = items2,
            TotalCount = totalCount2,
            PageNumber = pageNumber2,
            PageSize = pageSize2
        };
    }
}