using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherSubmissionHandler
    {
        private readonly IUserService _userService;
        private readonly ISubmissionService _submissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeacherSubmissionHandler(
            IUserService userService,
            ISubmissionService submissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _submissionService = submissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> HandleGetSubmissionsAsync(SubmissionFilterDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Filter parameters are required.");
            }

            PagedResultDto<SubmissionResponseDto> submissions = await _submissionService.GetSubmissionsAsync(dto);
            return new OkObjectResult(submissions);
        }

        public async Task<IActionResult> HandleGradeSubmissionAsync(GradeDto dto, Guid teacherId)
        {
            if (dto == null)
            {
                throw new BadRequestException("Grade data is required.");
            }

            var user = _httpContextAccessor.HttpContext?.User;
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);
            await _submissionService.GradeSubmissionAsync(dto, teacherId);
            return new OkObjectResult(new { message = "Submission graded successfully." });
        }
    }
}
