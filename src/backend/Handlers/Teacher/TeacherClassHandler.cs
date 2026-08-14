using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherClassHandler
    {
        private readonly IUserService _userService;
        private readonly ITeacherAssignmentService _teacherAssignmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TeacherClassHandler> _logger;

        public TeacherClassHandler(
            IUserService userService,
            ITeacherAssignmentService teacherAssignmentService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TeacherClassHandler> logger)
        {
            _userService = userService;
            _teacherAssignmentService = teacherAssignmentService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetClassesAsync(TeacherClassFilterDto dto, Guid teacherId)
        {
            _logger.LogInformation("TeacherClassHandler: Fetching classes for TeacherId:{TeacherId}", teacherId);
            var user = _httpContextAccessor.HttpContext?.User;
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);
            PagedResultDto<TeacherAssignedClassSubjectDto> result = await _teacherAssignmentService.GetAssignedClassesPagedAsync(teacherId, dto);
            _logger.LogInformation("TeacherClassHandler: Found {Count} assigned classes for TeacherId:{TeacherId}", result.TotalCount, teacherId);
            return new OkObjectResult(result);
        }
    }
}
