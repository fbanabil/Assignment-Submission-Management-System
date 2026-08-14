using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.TeacherDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherDashboardHandler
    {
        private readonly IUserService _userService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;
        private readonly ITeacherAssignmentService _teacherAssignmentService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeacherDashboardHandler(
            IUserService userService,
            IAssignmentService assignmentService,
            ISubmissionService submissionService,
            ITeacherAssignmentService teacherAssignmentService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _teacherAssignmentService = teacherAssignmentService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> HandleDashboardAsync(TeacherDashboardFilterDto dto, Guid teacherId)
        {
            TeacherDashboardResponseDto dashboardResponseDto = new TeacherDashboardResponseDto();
            var user = _httpContextAccessor.HttpContext?.User;

            (dashboardResponseDto.TeacherName, dashboardResponseDto.TeacherEmail, teacherId) = await _userService.GetTeacherNameAndEmail(user!, teacherId);

            dashboardResponseDto.TotalAssignedClasses = await _assignmentService.GetTotalAssignedClassesCount(teacherId);
            dashboardResponseDto.UngradedSubmissionsCount = await _submissionService.GetUngradedSubmissionsCount(teacherId);
            dashboardResponseDto.ActiveAssignmentsCount = await _assignmentService.GetActiveAssignmentsCount(teacherId);
            dashboardResponseDto.AssignedClasses = await _teacherAssignmentService.GetAssignedClasses(teacherId);
            dashboardResponseDto.UpcomingDeadlines = await _assignmentService.GetUpcomingDeadlines(teacherId);
            dashboardResponseDto.UpcomingDeadlinesCount = dashboardResponseDto.UpcomingDeadlines.Count;

            return new OkObjectResult(dashboardResponseDto);
        }
    }
}
