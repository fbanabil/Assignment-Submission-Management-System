namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;

public class SubmissionService : ISubmissionService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public SubmissionService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IEnumerable<Submission>> GetAllSubmissionsAsync() =>
        await _context.Submissions.Include(s => s.Student).Include(s => s.Assignment).ToListAsync();

    public async Task<Submission?> GetSubmissionByIdAsync(Guid id) =>
        await _context.Submissions.FindAsync(id);



    /// <summary>
    /// This method creates a new submission based on the provided SubmissionCreateDto. It initializes a new Submission entity, sets its properties, adds it to the database context, and saves the changes asynchronously.
    /// </summary>
    /// <param name="dto">The data transfer object containing the submission details.</param>
    /// <returns>The created Submission entity.</returns>
    public async Task<Submission> CreateSubmissionAsync(SubmissionCreateDto dto)
    {
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(!userClaims.Roles.Contains("Student"))
        {
            throw new UnauthorizedAccessException("Only students can create submissions.");
        }

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




    /// <summary>
    /// This method updates an existing submission based on the provided SubmissionUpdateDto. It retrieves the submission by its ID, updates its properties if they are provided in the DTO, sets the LastUpdatedAt timestamp, and saves the changes asynchronously.
    /// </summary>
    /// <param name="id">The ID of the submission to update.</param>
    /// <param name="dto">The data transfer object containing the updated submission details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateSubmissionAsync(Guid id, SubmissionUpdateDto dto)
    {
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null) return;


        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(!userClaims.Roles.Contains("Student"))
        {
            throw new UnauthorizedAccessException("Only students can update submissions.");
        }


        if(submission.StudentId != userClaims.UserId)
        {
            throw new UnauthorizedAccessException("You can only update your own submissions.");
        }


        if (dto.SubmissionText != null) submission.SubmissionText = dto.SubmissionText;
        if (dto.FileUrl != null) submission.FileUrl = dto.FileUrl;

        submission.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }



    /// <summary>
    /// This method grades a submission based on the provided GradeDto and the ID of the grader (teacher). It retrieves the submission by its ID, checks if the grader is authorized to grade the submission, updates the submission's marks, feedback, graded by, graded at timestamp, and status, and saves the changes asynchronously.
    /// </summary>
    /// <param name="dto">The data transfer object containing the grading details.</param>
    /// <param name="graderId">The ID of the teacher grading the submission.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task GradeSubmissionAsync(GradeDto dto, Guid graderId)
    {
        var submission = await _context.Submissions.FindAsync(dto.SubmissionId);
        if (submission == null) return;

        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(!userClaims.Roles.Contains("Teacher"))
        {
            throw new UnauthorizedAccessException("Only teachers can grade submissions.");
        }



        submission.Marks = dto.Marks;
        submission.Feedback = dto.Feedback;
        submission.GradedBy = userClaims.UserId;
        submission.GradedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Graded;

        await _context.SaveChangesAsync();
    }




    /// <summary>
    /// This method retrieves a summary of submissions, including total submissions, submissions made today, pending reviews, graded submissions, and weekly submission volumes.
    /// </summary>
    /// <returns>A SubmissionSummaryDto containing the summary statistics.</returns>
    public async Task<SubmissionSummaryDto> GetSubmissionSummaryAsync()
    {
        SubmissionSummaryDto submissionSummaryDto = new SubmissionSummaryDto()
        {
            TotalSubmissions = await _context.Submissions.CountAsync(),
            SubmittedToday = await _context.Submissions.CountAsync(s => s.SubmittedAt.Date == DateTime.UtcNow.Date),
            PendingReview = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Submitted),
            GradedSubmissions = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Graded),
            WeeklyVolumes = await _context.Submissions
                .Where(s => s.SubmittedAt >= DateTime.UtcNow.AddDays(-7))
                // group by day of the week and count the number of submissions for each day
                .GroupBy(s => s.SubmittedAt.Date)
                .Select(g => new SubmissionVolumeDto
                {
                    // Label = g.Key.ToString("dddd"), // day of the week only 1st 3 letter
                    Label = new string(g.Key.ToString("dddd").Take(3).ToArray()), // date
                    Count = g.Count()
                })
                .ToListAsync()
        };
        return submissionSummaryDto;
    }



    /// <summary>
    /// This method retrieves a paginated list of submissions based on the provided filter criteria, including class name, assignment title, student name, student email, and submission status.
    /// </summary>
    /// <param name="filterDto">The filter criteria for retrieving submissions.</param>
    /// <returns>A paginated list of submissions matching the filter criteria.</returns>
    public async Task<PagedResultDto<SubmissionResponseDto>> GetSubmissionsAsync(SubmissionFilterDto filterDto)
    {
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        var query = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Subject)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Class)
                .Where(s => s.Assignment.TeacherId == userClaims.UserId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filterDto.ClassName))
            query = query.Where(s => EF.Functions.Like(s.Assignment.Class.Name, $"%{filterDto.ClassName}%"));
        if(!string.IsNullOrEmpty(filterDto.AssignmentTitle))
            query = query.Where(s => EF.Functions.Like(s.Assignment.Title, $"%{filterDto.AssignmentTitle}%"));

        if(!string.IsNullOrEmpty(filterDto.StudentName))
            query = query.Where(s => EF.Functions.Like(s.Student.FullName, $"%{filterDto.StudentName}%"));
        if(!string.IsNullOrEmpty(filterDto.StudentEmail))
            query = query.Where(s => EF.Functions.Like(s.Student.Email, $"%{filterDto.StudentEmail}%"));
        if(!string.IsNullOrEmpty(filterDto.Status))
            query = query.Where(s => s.Status.ToString() == filterDto.Status);
        if(!string.IsNullOrEmpty(filterDto.SubjectName))
            query = query.Where(s => EF.Functions.Like(s.Assignment.Subject.Name, $"%{filterDto.SubjectName}%"));

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var submissions = await query
            .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .ToListAsync();


        // Map to DTOs
        var submissionDtos = submissions.Select(s => new SubmissionResponseDto
        {
            Id = s.Id,
            StudentName = s.Student.FullName,
            StudentEmail = s.Student.Email,
            AssignmentTitle = s.Assignment.Title,
            ClassName = s.Assignment.Class.Name,
            ClassSection = s.Assignment.Class.Section,
            AcademicYear = s.Assignment.Class.AcademicYear,
            SubjectName = s.Assignment.Subject.Name,
            SubjectCode = s.Assignment.Subject.Code,
            FileUrl = s.FileUrl,
            SubmittedAt = s.SubmittedAt,
            Grade = s.Marks,
            MaxMarks = s.Assignment.MaxMarks,
            Feedback = s.Feedback,
            Status = s.Status.ToString()
        }).ToList();


        // Return paged result
        return new PagedResultDto<SubmissionResponseDto>
        {
            Items = submissionDtos,
            TotalCount = totalCount,
            PageNumber = filterDto.PageNumber,
            PageSize = filterDto.PageSize
        };
    }



    /// <summary>
    /// This method retrieves the count of ungraded submissions for a specific teacher, based on the teacher's ID. It counts the number of submissions that are associated with assignments created by the specified teacher and have a status of "Submitted".
    /// </summary>
    /// <param name="teacherId">The ID of the teacher.</param>
    /// <returns>The count of ungraded submissions for the specified teacher.</returns>
    public async Task<int> GetUngradedSubmissionsCount(Guid teacherId)
    {
        if(teacherId == Guid.Empty)
        {
            teacherId = await _userService.GetUserIdAndEmailFromClaims().ContinueWith(t => t.Result.UserId);
        }

        return await _context.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.Assignment.TeacherId == teacherId && s.Status == SubmissionStatus.Submitted)
            .CountAsync();
    }
}