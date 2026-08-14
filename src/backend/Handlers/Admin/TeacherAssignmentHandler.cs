using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class TeacherAssignmentHandler
    {
        private readonly ITeacherAssignmentService _teacherAssignmentService;
        private readonly ILogger<TeacherAssignmentHandler> _logger;

        public TeacherAssignmentHandler(ITeacherAssignmentService teacherAssignmentService, ILogger<TeacherAssignmentHandler> logger)
        {
            _teacherAssignmentService = teacherAssignmentService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetTeacherAssignmentsAsync(TeacherAssignmentFilterDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Admin TeacherAssignmentHandler: Filter parameters null");
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            _logger.LogInformation("Admin TeacherAssignmentHandler: Querying teacher assignments");
            PagedResultDto<TeacherAssignmentResponseDto> pagedTeacherAssignments = await _teacherAssignmentService.GetTeacherAssignmentsAsync(dto);
            _logger.LogInformation("Admin TeacherAssignmentHandler: Retrieved {Count} teacher assignments", pagedTeacherAssignments.TotalCount);
            return new OkObjectResult(pagedTeacherAssignments);
        }

        public async Task<IActionResult> HandleCreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Admin TeacherAssignmentHandler: TeacherAssignment data null");
                return new BadRequestObjectResult("TeacherAssignment data is required.");
            }
            _logger.LogInformation("Admin TeacherAssignmentHandler: Creating teacher assignment for TeacherId:{TeacherId}", dto.TeacherId);
            var createdTeacherAssignment = await _teacherAssignmentService.CreateTeacherAssignmentAsync(dto);
            _logger.LogInformation("Admin TeacherAssignmentHandler: Created teacher assignment with Id:{Id}", createdTeacherAssignment.Id);
            return new CreatedAtActionResult(nameof(AdminController.TeacherAssignments), "Admin", new { id = createdTeacherAssignment.Id }, createdTeacherAssignment);
        }

        public async Task<IActionResult> HandleDeleteTeacherAssignmentAsync(Guid id)
        {
            _logger.LogInformation("Admin TeacherAssignmentHandler: Deleting teacher assignment Id:{Id}", id);
            await _teacherAssignmentService.DeleteTeacherAssignmentAsync(id);
            _logger.LogInformation("Admin TeacherAssignmentHandler: Deleted teacher assignment Id:{Id}", id);
            return new NoContentResult();
        }
    }
}
