using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherAssignmentHandler
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<TeacherAssignmentHandler> _logger;

        public TeacherAssignmentHandler(IAssignmentService assignmentService, ILogger<TeacherAssignmentHandler> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetAssignmentsAsync(AssignmentFilterDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("TeacherAssignmentHandler: Filter parameters null");
                throw new BadRequestException("Filter parameters are required.");
            }

            _logger.LogInformation("TeacherAssignmentHandler: Querying teacher assignments");
            PagedResultDto<AssignmentResponseDto> assignments = await _assignmentService.GetAssignmentsForTeacher(dto);
            _logger.LogInformation("TeacherAssignmentHandler: Retrieved {Count} assignments", assignments.TotalCount);
            return new OkObjectResult(assignments);
        }

        public async Task<IActionResult> HandleCreateAssignmentAsync(AssignmentCreateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("TeacherAssignmentHandler: Assignment create data null");
                throw new BadRequestException("Assignment data is required.");
            }

            _logger.LogInformation("TeacherAssignmentHandler: Creating assignment with Title:{Title}", dto.Title);
            AssignmentResponseDto response = await _assignmentService.CreateAssignmentAsync(dto);
            _logger.LogInformation("TeacherAssignmentHandler: Created assignment Id:{AssignmentId}", response.Id);
            return new ObjectResult(response) { StatusCode = 201 };
        }

        public async Task<IActionResult> HandleUpdateAssignmentAsync(Guid id, AssignmentUpdateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("TeacherAssignmentHandler: Assignment update data null");
                throw new BadRequestException("Assignment data is required.");
            }

            _logger.LogInformation("TeacherAssignmentHandler: Updating assignment Id:{AssignmentId}", id);
            await _assignmentService.UpdateAssignmentAsync(id, dto);
            _logger.LogInformation("TeacherAssignmentHandler: Updated assignment Id:{AssignmentId}", id);
            return new NoContentResult();
        }
    }
}
