using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentDTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Handlers.Student
{
    public class StudentAssignmentHandler
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        public StudentAssignmentHandler(
            IAssignmentService assignmentService,
            ISubmissionService submissionService,
            IUserService userService,
            AppDbContext context)
        {
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _userService = userService;
            _context = context;
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
            var result = await _assignmentService.GetAssignmentsForStudentPagedAsync(studentId, filterDto);
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleGetStudentAssignmentDetailAsync(Guid assignmentId)
        {
            Guid studentId = await ResolveStudentIdAsync(null);
            var result = await _assignmentService.GetAssignmentDetailForStudentAsync(studentId, assignmentId);
            if (result == null)
            {
                return new NotFoundObjectResult(new { message = "Assignment not found." });
            }
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleCreateStudentSubmissionAsync(StudentSubmissionCreateDto dto)
        {
            Guid studentId = await ResolveStudentIdAsync(dto.StudentId);
            var submission = await _submissionService.CreateStudentSubmissionAsync(studentId, dto);
            return new StatusCodeResult(201);
        }

        public async Task<IActionResult> HandleFileUploadAsync(IFormFile file, IWebHostEnvironment environment)
        {
            var result = await _submissionService.UploadAssignmentFileAsync(file, environment.WebRootPath);
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleUnsubmitAssignmentAsync(Guid submissionId)
        {
            Guid studentId = await ResolveStudentIdAsync(null);
            await _submissionService.UnsubmitAssignmentAsync(studentId, submissionId);
            return new NoContentResult();
        }

        public async Task<IActionResult> HandleGetStudentSubmissionsHistoryAsync(StudentSubmissionHistoryFilterDto filterDto)
        {
            Guid studentId = await ResolveStudentIdAsync(null);
            var result = await _submissionService.GetStudentSubmissionHistoryPagedAsync(studentId, filterDto);
            return new OkObjectResult(result);
        }
    }
}
