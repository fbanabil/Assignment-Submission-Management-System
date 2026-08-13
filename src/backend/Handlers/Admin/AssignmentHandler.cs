using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class AssignmentHandler
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentHandler(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        public async Task<IActionResult> HandleGetAssignmentsAsync(AssignmentFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            PagedResultDto<AssignmentResponseDto> pagedAssignments = await _assignmentService.GetAssignmentsAsync(filterDto);
            return new OkObjectResult(pagedAssignments);
        }
    }
}
