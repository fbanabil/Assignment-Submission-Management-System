namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.StudentDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Hosting;

public class SubmissionService : ISubmissionService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(AppDbContext context, IUserService userService, IWebHostEnvironment environment, ILogger<SubmissionService> logger)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
        _logger = logger;
    }

    private void DeletePhysicalFileFromWebRoot(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(_environment.WebRootPath)) return;

        try
        {
            string clean = fileUrl.Replace('\\', '/');
            if (clean.StartsWith("/")) clean = clean.Substring(1);
            if (clean.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(8);
            }

            string fullPath = Path.Combine(_environment.WebRootPath, clean);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("SubmissionService: Deleted physical file at {Path}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SubmissionService: Failed to delete physical file {FileUrl}", fileUrl);
        }
    }

    public async Task<IEnumerable<Submission>> GetAllSubmissionsAsync()
    {
        _logger.LogInformation("SubmissionService: Fetching all submissions");
        return await _context.Submissions.Include(s => s.Student).Include(s => s.Assignment).ToListAsync();
    }

    public async Task<Submission?> GetSubmissionByIdAsync(Guid id)
    {
        _logger.LogInformation("SubmissionService: Fetching submission by Id:{Id}", id);
        return await _context.Submissions.FindAsync(id);
    }



    /// <summary>
    /// This method creates a new submission based on the provided SubmissionCreateDto. It initializes a new Submission entity, sets its properties, adds it to the database context, and saves the changes asynchronously.
    /// </summary>
    /// <param name="dto">The data transfer object containing the submission details.</param>
    /// <returns>The created Submission entity.</returns>
    public async Task<Submission> CreateSubmissionAsync(SubmissionCreateDto dto)
    {
        _logger.LogInformation("SubmissionService: Creating submission for AssignmentId:{AssignmentId}, StudentId:{StudentId}", dto.AssignmentId, dto.StudentId);
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(!userClaims.Roles.Contains("Student"))
        {
            _logger.LogWarning("SubmissionService: Non-student user attempting to submit work");
            throw new ForbiddenException("Only students can create submissions.");
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
        _logger.LogInformation("SubmissionService: Created submission Id:{Id}", submission.Id);
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
        _logger.LogInformation("SubmissionService: Updating submission Id:{Id}", id);
        var submission = await _context.Submissions.FindAsync(id);
        if (submission == null)
        {
            _logger.LogWarning("SubmissionService: Submission Id:{Id} not found for update", id);
            return;
        }


        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(!userClaims.Roles.Contains("Student"))
        {
            _logger.LogWarning("SubmissionService: Non-student user attempting to update submission");
            throw new ForbiddenException("Only students can update submissions.");
        }


        if(submission.StudentId != userClaims.UserId)
        {
            _logger.LogWarning("SubmissionService: Student {UserId} attempted to update submission belonging to {OwnerId}", userClaims.UserId, submission.StudentId);
            throw new ForbiddenException("You can only update your own submissions.");
        }


        submission.SubmissionText = dto.SubmissionText;
        submission.FileUrl = string.IsNullOrWhiteSpace(dto.FileUrl) ? null : dto.FileUrl;

        submission.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("SubmissionService: Updated submission Id:{Id}", id);
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
        _logger.LogInformation("SubmissionService: Grading submission Id:{SubmissionId} by GraderId:{GraderId}", dto.SubmissionId, graderId);
        var submission = await _context.Submissions.FindAsync(dto.SubmissionId);
        if (submission == null)
        {
            _logger.LogWarning("SubmissionService: Submission Id:{SubmissionId} not found for grading", dto.SubmissionId);
            return;
        }

        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        if(!userClaims.Roles.Contains("Teacher"))
        {
            _logger.LogWarning("SubmissionService: Non-teacher attempting to grade submission");
            throw new ForbiddenException("Only teachers can grade submissions.");
        }



        submission.Marks = dto.Marks;
        submission.Feedback = dto.Feedback;
        submission.GradedBy = userClaims.UserId;
        submission.GradedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Graded;

        await _context.SaveChangesAsync();
        _logger.LogInformation("SubmissionService: Graded submission Id:{SubmissionId} with Marks:{Marks}", dto.SubmissionId, dto.Marks);
    }




    /// <summary>
    /// This method retrieves a summary of submissions, including total submissions, submissions made today, pending reviews, graded submissions, and weekly submission volumes.
    /// </summary>
    /// <returns>A SubmissionSummaryDto containing the summary statistics.</returns>
    public async Task<SubmissionSummaryDto> GetSubmissionSummaryAsync()
    {
        _logger.LogInformation("SubmissionService: Querying submission summary metrics");
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
        _logger.LogInformation("SubmissionService: Querying paged submissions");
        var userClaims = await _userService.GetUserIdAndEmailFromClaims();

        var query = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Subject)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Class)
                // if role is teacher, filter by teacher id
                .Where(s => userClaims.Roles.Contains("Teacher") ? s.Assignment.TeacherId == userClaims.UserId : true)
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

        bool isDesc = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy = filterDto.SortBy?.ToLower().Trim() ?? "submittedat";

        query = sortBy switch
        {
            "studentname" => isDesc ? query.OrderByDescending(s => s.Student.FullName) : query.OrderBy(s => s.Student.FullName),
            "assignmenttitle" => isDesc ? query.OrderByDescending(s => s.Assignment.Title) : query.OrderBy(s => s.Assignment.Title),
            "classname" => isDesc ? query.OrderByDescending(s => s.Assignment.Class.Name) : query.OrderBy(s => s.Assignment.Class.Name),
            "subjectname" => isDesc ? query.OrderByDescending(s => s.Assignment.Subject.Name) : query.OrderBy(s => s.Assignment.Subject.Name),
            "status" => isDesc ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            "marks" or "grade" => isDesc ? query.OrderByDescending(s => s.Marks) : query.OrderBy(s => s.Marks),
            _ => isDesc ? query.OrderByDescending(s => s.SubmittedAt) : query.OrderBy(s => s.SubmittedAt)
        };

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
            StudentRollNo = s.Student.RollNo,
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

        _logger.LogInformation("SubmissionService: Retrieved {Count} submissions matching filter", totalCount);

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



    /// <summary>
    /// This method retrieves all submissions made by a specific student, identified by their student ID. It includes related assignment and subject information, as well as the teacher who graded the submission (if applicable).
    /// </summary>
    /// <param name="targetStudentId">The ID of the student whose submissions are to be retrieved.</param>
    /// <returns>A list of Submission entities made by the specified student.</returns>
    public async Task<List<Submission>?> GetSubmissionsForStudentAsync(Guid targetStudentId)
    {
        _logger.LogInformation("SubmissionService: Fetching submissions for StudentId:{StudentId}", targetStudentId);
        return await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Subject)
                .Include(s => s.GradeGiver)
                .Where(s => s.StudentId == targetStudentId)
                .ToListAsync();
    }

    public async Task<Backend.DTOs.StudentDTOs.StudentSubmissionDetailDto> CreateStudentSubmissionAsync(Guid studentId, Backend.DTOs.StudentDTOs.StudentSubmissionCreateDto dto)
    {
        _logger.LogInformation("SubmissionService: Creating student submission for StudentId:{StudentId}, AssignmentId:{AssignmentId}", studentId, dto.AssignmentId);
        var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
        if (assignment == null)
        {
            _logger.LogWarning("SubmissionService: Assignment Id:{AssignmentId} not found", dto.AssignmentId);
            throw new Backend.Middlewares.BadRequestException("Assignment not found.");
        }

        // Deadline check
        if (assignment.Deadline < DateTime.UtcNow && !assignment.AllowLateSubmission)
        {
            _logger.LogWarning("SubmissionService: Late submission attempt past deadline for AssignmentId:{AssignmentId}", dto.AssignmentId);
            throw new Backend.Middlewares.BadRequestException("The deadline for this assignment has passed and late submissions are not allowed.");
        }

        var existingSubmission = await _context.Submissions
            .Include(s => s.GradeGiver)
            .FirstOrDefaultAsync(s => s.AssignmentId == dto.AssignmentId && s.StudentId == studentId);

        if (existingSubmission != null)
        {
            if (!assignment.AllowResubmission)
            {
                _logger.LogWarning("SubmissionService: Resubmission attempt when disabled for AssignmentId:{AssignmentId}", dto.AssignmentId);
                throw new Backend.Middlewares.BadRequestException("You have already submitted this assignment and resubmission is disabled.");
            }

            // If old file URL existed and was removed or replaced with a new file URL, delete old file from disk
            string? newFileUrl = string.IsNullOrWhiteSpace(dto.FileUrl) ? null : dto.FileUrl;
            if (!string.IsNullOrWhiteSpace(existingSubmission.FileUrl) && existingSubmission.FileUrl != newFileUrl)
            {
                DeletePhysicalFileFromWebRoot(existingSubmission.FileUrl);
            }

            // Update existing submission
            existingSubmission.SubmissionText = dto.SubmissionText;
            existingSubmission.FileUrl = newFileUrl;
            existingSubmission.SubmittedAt = DateTime.UtcNow;
            existingSubmission.LastUpdatedAt = DateTime.UtcNow;
            existingSubmission.Status = SubmissionStatus.Submitted;

            await _context.SaveChangesAsync();
            _logger.LogInformation("SubmissionService: Updated existing student submission Id:{SubmissionId}", existingSubmission.Id);

            return new Backend.DTOs.StudentDTOs.StudentSubmissionDetailDto
            {
                SubmissionId = existingSubmission.Id,
                SubmissionText = existingSubmission.SubmissionText,
                FileUrl = existingSubmission.FileUrl,
                SubmittedAt = existingSubmission.SubmittedAt,
                Marks = existingSubmission.Marks,
                Feedback = existingSubmission.Feedback,
                GradedAt = existingSubmission.GradedAt,
                GradedByTeacherName = existingSubmission.GradeGiver?.FullName
            };
        }

        var newSubmission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = dto.AssignmentId,
            StudentId = studentId,
            SubmissionText = dto.SubmissionText,
            FileUrl = dto.FileUrl,
            SubmittedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        };

        _context.Submissions.Add(newSubmission);
        await _context.SaveChangesAsync();
        _logger.LogInformation("SubmissionService: Created new student submission Id:{SubmissionId}", newSubmission.Id);

        return new Backend.DTOs.StudentDTOs.StudentSubmissionDetailDto
        {
            SubmissionId = newSubmission.Id,
            SubmissionText = newSubmission.SubmissionText,
            FileUrl = newSubmission.FileUrl,
            SubmittedAt = newSubmission.SubmittedAt,
            Marks = null,
            Feedback = null,
            GradedAt = null,
            GradedByTeacherName = null
        };
    }

    public async Task<FileUploadResponseDto> UploadAssignmentFileAsync(IFormFile file, string webRootPath)
    {
        _logger.LogInformation("SubmissionService: Uploading file {FileName} ({Length} bytes)", file?.FileName, file?.Length ?? 0);
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("SubmissionService: Empty or null file upload attempt");
            throw new BadRequestException("No file was selected or the file is empty.");
        }

        string folderPath = Path.Combine(webRootPath, "assignments");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileExtension = Path.GetExtension(file.FileName);
        string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        string fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string relativePath = $"/assignments/{uniqueFileName}";
        _logger.LogInformation("SubmissionService: File saved to {Path}", relativePath);

        return new FileUploadResponseDto
        {
            FilePath = relativePath,
            OriginalFileName = file.FileName,
            FileSize = file.Length
        };
    }

    public async Task UnsubmitAssignmentAsync(Guid studentId, Guid submissionId)
    {
        _logger.LogInformation("SubmissionService: Unsubmitting submission Id:{SubmissionId} for StudentId:{StudentId}", submissionId, studentId);
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => (s.Id == submissionId || s.AssignmentId == submissionId) && (studentId == Guid.Empty || s.StudentId == studentId));

        if (submission == null)
        {
            submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId || s.AssignmentId == submissionId);
        }

        if (submission == null)
        {
            _logger.LogWarning("SubmissionService: Submission Id:{SubmissionId} not found for unsubmit", submissionId);
            throw new BadRequestException("Submission not found or does not belong to you.");
        }

        if (!submission.Assignment.AllowResubmission)
        {
            _logger.LogWarning("SubmissionService: Unsubmit attempted when resubmission disabled for AssignmentId:{AssignmentId}", submission.AssignmentId);
            throw new BadRequestException("Resubmission is not enabled for this assignment.");
        }

        if (submission.Assignment.Deadline < DateTime.UtcNow && !submission.Assignment.AllowLateSubmission)
        {
            _logger.LogWarning("SubmissionService: Unsubmit attempted after deadline for AssignmentId:{AssignmentId}", submission.AssignmentId);
            throw new BadRequestException("The deadline for this assignment has passed. Unsubmitting is no longer allowed.");
        }

        // Delete physical attachment file from server disk wwwroot/assignments/ if present
        if (!string.IsNullOrWhiteSpace(submission.FileUrl))
        {
            DeletePhysicalFileFromWebRoot(submission.FileUrl);
        }

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();
        _logger.LogInformation("SubmissionService: Unsubmitted and deleted submission Id:{SubmissionId}", submission.Id);
    }

    public async Task<PagedResultDto<StudentSubmissionHistoryResponseDto>> GetStudentSubmissionHistoryPagedAsync(Guid studentId, StudentSubmissionHistoryFilterDto filterDto)
    {
        _logger.LogInformation("SubmissionService: Fetching submission history for StudentId:{StudentId}", studentId);
        var query = _context.Submissions
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Class)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Subject)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Teacher)
            .Include(s => s.GradeGiver)
            .Where(s => s.StudentId == studentId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterDto.SubjectName))
        {
            query = query.Where(s => EF.Functions.Like(s.Assignment.Subject.Name, $"%{filterDto.SubjectName}%"));
        }

        if (!string.IsNullOrWhiteSpace(filterDto.Status) && !filterDto.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (filterDto.Status.Equals("Graded", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Status == SubmissionStatus.Graded || s.Marks.HasValue);
            }
            else if (filterDto.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Status == SubmissionStatus.Submitted && !s.Marks.HasValue);
            }
        }

        int totalCount = await query.CountAsync();
        int pageNumber = Math.Max(1, filterDto.PageNumber);
        int pageSize = Math.Max(1, filterDto.PageSize);

        var submissions = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentSubmissionHistoryResponseDto
            {
                SubmissionId = s.Id,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                SubjectName = s.Assignment.Subject.Name,
                SubjectCode = s.Assignment.Subject.Code,
                ClassName = s.Assignment.Class.Name,
                TeacherName = s.Assignment.Teacher.FullName,
                SubmittedAt = s.SubmittedAt,
                FileUrl = s.FileUrl,
                SubmissionText = s.SubmissionText,
                Status = s.Marks.HasValue ? "Graded" : "Submitted",
                Marks = s.Marks,
                MaxMarks = s.Assignment.MaxMarks,
                Feedback = s.Feedback,
                GradedAt = s.GradedAt,
                AllowResubmission = s.Assignment.AllowResubmission,
                Deadline = s.Assignment.Deadline
            })
            .ToListAsync();

        _logger.LogInformation("SubmissionService: Found {Count} submission history records for StudentId:{StudentId}", totalCount, studentId);

        return new PagedResultDto<StudentSubmissionHistoryResponseDto>
        {
            Items = submissions,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}