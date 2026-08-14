using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Handlers.Teacher
{
    public class TeacherAssignmentHandler
    {
        private readonly IAssignmentService _assignmentService;

        public TeacherAssignmentHandler(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        public async Task<IActionResult> HandleGetAssignmentsAsync(AssignmentFilterDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Filter parameters are required.");
            }

            PagedResultDto<AssignmentResponseDto> assignments = await _assignmentService.GetAssignmentsForTeacher(dto);
            return new OkObjectResult(assignments);
        }

        public async Task<IActionResult> HandleCreateAssignmentAsync(AssignmentCreateDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Assignment data is required.");
            }

            AssignmentResponseDto response = await _assignmentService.CreateAssignmentAsync(dto);
            return new ObjectResult(response) { StatusCode = 201 };
        }

        public async Task<IActionResult> HandleUpdateAssignmentAsync(Guid id, AssignmentUpdateDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Assignment data is required.");
            }

            await _assignmentService.UpdateAssignmentAsync(id, dto);
            return new NoContentResult();
        }
    }
}
