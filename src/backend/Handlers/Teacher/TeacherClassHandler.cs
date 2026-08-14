using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherClassHandler
    {
        private readonly IUserService _userService;
        private readonly ITeacherAssignmentService _teacherAssignmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeacherClassHandler(
            IUserService userService,
            ITeacherAssignmentService teacherAssignmentService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _teacherAssignmentService = teacherAssignmentService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> HandleGetClassesAsync(TeacherClassFilterDto dto, Guid teacherId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);
            PagedResultDto<TeacherAssignedClassSubjectDto> result = await _teacherAssignmentService.GetAssignedClassesPagedAsync(teacherId, dto);
            return new OkObjectResult(result);
        }
    }
}
