namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentEnrollmentDTOs;
using Microsoft.EntityFrameworkCore;

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

    public async Task<StudentEnrollment> CreateStudentEnrollmentAsync(StudentEnrollmentCreateDto dto)
    {
        var enrollment = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = dto.StudentId,
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
}