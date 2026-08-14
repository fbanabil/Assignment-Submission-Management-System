namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

using AssignmentSystem.Api.Models.Enums;

public class StudentEnrollmentService : IStudentEnrollmentService
{
    private readonly AppDbContext _context;

    public StudentEnrollmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentEnrollment>> GetAllStudentEnrollmentsAsync() =>
        await _context.StudentEnrollments.Include(se => se.Student).Include(se => se.Class).ToListAsync();

    public async Task<StudentEnrollment?> GetStudentEnrollmentByIdAsync(Guid id) =>
        await _context.StudentEnrollments.FindAsync(id);

    /// <summary>
    /// This method creates a new student enrollment record in the database after verifying the student email and role.
    /// </summary>
    public async Task<StudentEnrollment> CreateStudentEnrollmentAsync(StudentEnrollmentCreateDto dto)
    {
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
            throw new BadRequestException($"No user found with email '{dto.StudentEmail}'.");
        }

        // 2. Verify user has the Student role
        if (student.Role != UserRole.Student)
        {
            throw new BadRequestException($"The user with email '{dto.StudentEmail}' is assigned as a {student.Role}, not a Student. Only students can be enrolled in classes.");
        }

        // 3. Check if the student is already enrolled in the specified class
        var existingEnrollment = await _context.StudentEnrollments
            .FirstOrDefaultAsync(se => se.StudentId == student.Id && se.ClassId == dto.ClassId);

        if (existingEnrollment != null)
        {
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
        return enrollment;
    }









    public async Task DeleteStudentEnrollmentAsync(Guid id)
    {
        var enrollment = await _context.StudentEnrollments.FindAsync(id);
        if (enrollment != null)
        {
            _context.StudentEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
        }
    }




    /// <summary>
    /// This method retrieves the list of class IDs that a specific student is enrolled in.
    /// </summary>
    /// <param name="targetStudentId">The ID of the student whose enrolled class IDs are to be retrieved.</param>
    /// <returns>A list of class IDs that the student is enrolled in.</returns>
    public async Task<List<Guid>> GetEnrolledClassIdsAsync(Guid targetStudentId)
    {
        return await _context.StudentEnrollments
                .Where(e => e.StudentId == targetStudentId)
                .Select(e => e.ClassId)
                .ToListAsync();
    }

    public async Task<PagedResultDto<StudentEnrollmentResponseDto>> GetStudentEnrollmentsAsync(StudentEnrollmentFilterDto filterDto)
    {
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

        int totalCount = await query.CountAsync();
        int pageNumber = Math.Max(1, filterDto.PageNumber);
        int pageSize = Math.Max(1, filterDto.PageSize);

        var items = await query
            .OrderByDescending(se => se.EnrolledAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(se => new StudentEnrollmentResponseDto
            {
                Id = se.Id,
                StudentId = se.StudentId,
                StudentName = se.Student.FullName,
                StudentEmail = se.Student.Email,
                ClassId = se.ClassId,
                ClassName = se.Class.Name,
                ClassSection = se.Class.Section,
                AcademicYear = se.Class.AcademicYear,
                EnrolledAt = se.EnrolledAt
            })
            .ToListAsync();

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

        int totalCount = await query.CountAsync();
        int pageNumber = Math.Max(1, filterDto.PageNumber);
        int pageSize = Math.Max(1, filterDto.PageSize);

        var items = await query
            .OrderByDescending(se => se.EnrolledAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(se => new StudentEnrollmentResponseDto
            {
                Id = se.Id,
                StudentId = se.StudentId,
                StudentName = se.Student.FullName,
                StudentEmail = se.Student.Email,
                ClassId = se.ClassId,
                ClassName = se.Class.Name,
                ClassSection = se.Class.Section,
                AcademicYear = se.Class.AcademicYear,
                EnrolledAt = se.EnrolledAt
            })
            .ToListAsync();

        return new PagedResultDto<StudentEnrollmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}