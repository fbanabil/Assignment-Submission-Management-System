using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class TeacherAssignmentHandler
    {
        private readonly ITeacherAssignmentService _teacherAssignmentService;

        public TeacherAssignmentHandler(ITeacherAssignmentService teacherAssignmentService)
        {
            _teacherAssignmentService = teacherAssignmentService;
        }

        public async Task<IActionResult> HandleGetTeacherAssignmentsAsync(TeacherAssignmentFilterDto dto)
        {
            if (dto == null)
            {
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            PagedResultDto<TeacherAssignmentResponseDto> pagedTeacherAssignments = await _teacherAssignmentService.GetTeacherAssignmentsAsync(dto);
            return new OkObjectResult(pagedTeacherAssignments);
        }

        public async Task<IActionResult> HandleCreateTeacherAssignmentAsync(TeacherAssignmentCreateDto dto)
        {
            if (dto == null)
            {
                return new BadRequestObjectResult("TeacherAssignment data is required.");
            }
            var createdTeacherAssignment = await _teacherAssignmentService.CreateTeacherAssignmentAsync(dto);
            return new CreatedAtActionResult(nameof(AdminController.TeacherAssignments), "Admin", new { id = createdTeacherAssignment.Id }, createdTeacherAssignment);
        }

        public async Task<IActionResult> HandleDeleteTeacherAssignmentAsync(Guid id)
        {
            await _teacherAssignmentService.DeleteTeacherAssignmentAsync(id);
            return new NoContentResult();
        }
    }
}
