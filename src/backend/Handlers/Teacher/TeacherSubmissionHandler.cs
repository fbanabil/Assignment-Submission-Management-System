using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherSubmissionHandler
    {
        private readonly IUserService _userService;
        private readonly ISubmissionService _submissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TeacherSubmissionHandler> _logger;

        public TeacherSubmissionHandler(
            IUserService userService,
            ISubmissionService submissionService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TeacherSubmissionHandler> logger)
        {
            _userService = userService;
            _submissionService = submissionService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetSubmissionsAsync(SubmissionFilterDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("TeacherSubmissionHandler: Filter parameters null");
                throw new BadRequestException("Filter parameters are required.");
            }

            _logger.LogInformation("TeacherSubmissionHandler: Querying submissions");
            PagedResultDto<SubmissionResponseDto> submissions = await _submissionService.GetSubmissionsAsync(dto);
            _logger.LogInformation("TeacherSubmissionHandler: Retrieved {Count} submissions", submissions.TotalCount);
            return new OkObjectResult(submissions);
        }

        public async Task<IActionResult> HandleGradeSubmissionAsync(GradeDto dto, Guid teacherId)
        {
            if (dto == null)
            {
                _logger.LogWarning("TeacherSubmissionHandler: Grade data null");
                throw new BadRequestException("Grade data is required.");
            }

            _logger.LogInformation("TeacherSubmissionHandler: Grading submission Id:{SubmissionId} with Marks:{Marks}", dto.SubmissionId, dto.Marks);
            var user = _httpContextAccessor.HttpContext?.User;
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);
            await _submissionService.GradeSubmissionAsync(dto, teacherId);
            _logger.LogInformation("TeacherSubmissionHandler: Graded submission Id:{SubmissionId} by TeacherId:{TeacherId}", dto.SubmissionId, teacherId);
            return new OkObjectResult(new { message = "Submission graded successfully." });
        }
    }
}
