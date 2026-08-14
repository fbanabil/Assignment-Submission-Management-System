using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentDTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Student
{
    public class StudentAssignmentHandler
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;
        private readonly IUserService _userService;
        private readonly AppDbContext _context;
        private readonly ILogger<StudentAssignmentHandler> _logger;

        public StudentAssignmentHandler(
            IAssignmentService assignmentService,
            ISubmissionService submissionService,
            IUserService userService,
            AppDbContext context,
            ILogger<StudentAssignmentHandler> logger)
        {
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _userService = userService;
            _context = context;
            _logger = logger;
        }

        private async Task<Guid> ResolveStudentIdAsync(Guid? requestedStudentId)
        {
            if (requestedStudentId.HasValue && requestedStudentId.Value != Guid.Empty)
            {
                return requestedStudentId.Value;
            }

            try
            {
                var claimsInfo = await _userService.GetUserIdAndEmailFromClaims();
                if (claimsInfo.UserId != Guid.Empty)
                {
                    return claimsInfo.UserId;
                }
            }
            catch
            {
                // Fallback to student role lookup if testing or unauthenticated claims
            }

            var fallbackStudent = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Student);
            return fallbackStudent?.Id ?? Guid.Empty;
        }

        public async Task<IActionResult> HandleGetStudentAssignmentsAsync(StudentAssignmentFilterDto filterDto)
        {
            Guid studentId = await ResolveStudentIdAsync(filterDto.StudentId);
            _logger.LogInformation("StudentAssignmentHandler: Get assignments for StudentId:{StudentId}", studentId);
            var result = await _assignmentService.GetAssignmentsForStudentPagedAsync(studentId, filterDto);
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleGetStudentAssignmentDetailAsync(Guid assignmentId)
        {
            Guid studentId = await ResolveStudentIdAsync(null);
            _logger.LogInformation("StudentAssignmentHandler: Get assignment detail for AssignmentId:{AssignmentId}, StudentId:{StudentId}", assignmentId, studentId);
            var result = await _assignmentService.GetAssignmentDetailForStudentAsync(studentId, assignmentId);
            if (result == null)
            {
                _logger.LogWarning("StudentAssignmentHandler: AssignmentId:{AssignmentId} not found", assignmentId);
                return new NotFoundObjectResult(new { message = "Assignment not found." });
            }
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleCreateStudentSubmissionAsync(StudentSubmissionCreateDto dto)
        {
            Guid studentId = await ResolveStudentIdAsync(dto.StudentId);
            _logger.LogInformation("StudentAssignmentHandler: Submitting work for AssignmentId:{AssignmentId}, StudentId:{StudentId}", dto.AssignmentId, studentId);
            var submission = await _submissionService.CreateStudentSubmissionAsync(studentId, dto);
            _logger.LogInformation("StudentAssignmentHandler: Created submission Id:{SubmissionId}", submission.SubmissionId);
            return new StatusCodeResult(201);
        }

        public async Task<IActionResult> HandleFileUploadAsync(IFormFile file, IWebHostEnvironment environment)
        {
            _logger.LogInformation("StudentAssignmentHandler: Uploading submission file {FileName}", file?.FileName);
            var result = await _submissionService.UploadAssignmentFileAsync(file, environment.WebRootPath);
            _logger.LogInformation("StudentAssignmentHandler: File uploaded successfully to {FilePath}", result.FilePath);
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleUnsubmitAssignmentAsync(Guid submissionId)
        {
            Guid studentId = await ResolveStudentIdAsync(null);
            _logger.LogInformation("StudentAssignmentHandler: Unsubmitting SubmissionId:{SubmissionId}, StudentId:{StudentId}", submissionId, studentId);
            await _submissionService.UnsubmitAssignmentAsync(studentId, submissionId);
            _logger.LogInformation("StudentAssignmentHandler: Unsubmitted SubmissionId:{SubmissionId}", submissionId);
            return new NoContentResult();
        }

        public async Task<IActionResult> HandleGetStudentSubmissionsHistoryAsync(StudentSubmissionHistoryFilterDto filterDto)
        {
            Guid studentId = await ResolveStudentIdAsync(null);
            _logger.LogInformation("StudentAssignmentHandler: Querying submission history for StudentId:{StudentId}", studentId);
            var result = await _submissionService.GetStudentSubmissionHistoryPagedAsync(studentId, filterDto);
            return new OkObjectResult(result);
        }
    }
}
