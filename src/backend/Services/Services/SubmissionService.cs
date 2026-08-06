namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Microsoft.EntityFrameworkCore;

public class SubmissionService : ISubmissionService
{
    private readonly AppDbContext _context;

    public SubmissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Submission>> GetAllSubmissionsAsync() =>
        await _context.Submissions.Include(s => s.Student).Include(s => s.Assignment).ToListAsync();

    public async Task<Submission?> GetSubmissionByIdAsync(Guid id) =>
        await _context.Submissions.FindAsync(id);

    public async Task<Submission> CreateSubmissionAsync(SubmissionCreateDto dto)
    {
        var submission = new Submission
        {
            AssignmentId = dto.AssignmentId,
            StudentId = dto.StudentId,
            SubmissionText = dto.SubmissionText,
            FileUrl = dto.FileUrl,
            SubmittedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task UpdateSubmissionAsync(Guid id, SubmissionUpdateDto dto)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null) return;

        if (dto.SubmissionText != null) submission.SubmissionText = dto.SubmissionText;
        if (dto.FileUrl != null) submission.FileUrl = dto.FileUrl;

        submission.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task GradeSubmissionAsync(GradeDto dto, Guid graderId)
    {
        var submission = await _context.Submissions.FindAsync(dto.SubmissionId);
        if (submission == null) return;

        submission.Marks = dto.Marks;
        submission.Feedback = dto.Feedback;
        submission.GradedBy = graderId;
        submission.GradedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Graded;

        await _context.SaveChangesAsync();
    }
}