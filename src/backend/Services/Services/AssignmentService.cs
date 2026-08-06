namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.AssignmentDTOs;
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

    public async Task<Assignment> CreateAssignmentAsync(AssignmentCreateDto dto)
    {
        var assignment = new Assignment
        {
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

    public async Task DeleteAssignmentAsync(Guid id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }
}