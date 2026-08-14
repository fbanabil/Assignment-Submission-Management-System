using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherEnrollmentHandler
    {
        private readonly IUserService _userService;
        private readonly IStudentEnrollmentService _studentEnrollmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TeacherEnrollmentHandler> _logger;

        public TeacherEnrollmentHandler(
            IUserService userService,
            IStudentEnrollmentService studentEnrollmentService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TeacherEnrollmentHandler> logger)
        {
            _userService = userService;
            _studentEnrollmentService = studentEnrollmentService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetEnrollmentsAsync(StudentEnrollmentFilterDto dto, Guid teacherId)
        {
            _logger.LogInformation("TeacherEnrollmentHandler: Fetching enrollments for TeacherId:{TeacherId}", teacherId);
            var user = _httpContextAccessor.HttpContext?.User;
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);
            PagedResultDto<StudentEnrollmentResponseDto> result = await _studentEnrollmentService.GetStudentEnrollmentsForTeacherAsync(teacherId, dto);
            _logger.LogInformation("TeacherEnrollmentHandler: Retrieved {Count} enrollments", result.TotalCount);
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleCreateEnrollmentAsync(StudentEnrollmentCreateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("TeacherEnrollmentHandler: Enrollment create data null");
                throw new BadRequestException("Student enrollment data is required.");
            }

            _logger.LogInformation("TeacherEnrollmentHandler: Enrolling student Email:{StudentEmail} in ClassId:{ClassId}", dto.StudentEmail, dto.ClassId);
            var enrollment = await _studentEnrollmentService.CreateStudentEnrollmentAsync(dto);
            _logger.LogInformation("TeacherEnrollmentHandler: Created enrollment Id:{Id}", enrollment.Id);
            return new ObjectResult(enrollment) { StatusCode = 201 };
        }

        public async Task<IActionResult> HandleDeleteEnrollmentAsync(Guid id)
        {
            _logger.LogInformation("TeacherEnrollmentHandler: Deleting enrollment Id:{Id}", id);
            await _studentEnrollmentService.DeleteStudentEnrollmentAsync(id);
            _logger.LogInformation("TeacherEnrollmentHandler: Deleted enrollment Id:{Id}", id);
            return new NoContentResult();
        }
    }
}
