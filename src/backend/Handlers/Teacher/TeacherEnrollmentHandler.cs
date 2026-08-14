using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherEnrollmentHandler
    {
        private readonly IUserService _userService;
        private readonly IStudentEnrollmentService _studentEnrollmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeacherEnrollmentHandler(
            IUserService userService,
            IStudentEnrollmentService studentEnrollmentService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _studentEnrollmentService = studentEnrollmentService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> HandleGetEnrollmentsAsync(StudentEnrollmentFilterDto dto, Guid teacherId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);
            PagedResultDto<StudentEnrollmentResponseDto> result = await _studentEnrollmentService.GetStudentEnrollmentsForTeacherAsync(teacherId, dto);
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleCreateEnrollmentAsync(StudentEnrollmentCreateDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Student enrollment data is required.");
            }

            var enrollment = await _studentEnrollmentService.CreateStudentEnrollmentAsync(dto);
            return new ObjectResult(enrollment) { StatusCode = 201 };
        }

        public async Task<IActionResult> HandleDeleteEnrollmentAsync(Guid id)
        {
            await _studentEnrollmentService.DeleteStudentEnrollmentAsync(id);
            return new NoContentResult();
        }
    }
}
