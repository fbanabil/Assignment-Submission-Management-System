namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.TeacherAssignmentDTOs;
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

    public async Task<TeacherAssignment> CreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto)
    {
        var assignment = new TeacherAssignment
        {
            TeacherId = dto.TeacherId,
            ClassSubjectId = dto.ClassSubjectId
        };

        _context.TeacherAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task DeleteTeacherAssignmentAsync(Guid id)
    {
        var assignment = await _context.TeacherAssignments.FindAsync(id);
        if (assignment != null)
        {
            _context.TeacherAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }
}