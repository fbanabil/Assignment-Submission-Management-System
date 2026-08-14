using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class AssignmentHandler
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<AssignmentHandler> _logger;

        public AssignmentHandler(IAssignmentService assignmentService, ILogger<AssignmentHandler> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetAssignmentsAsync(AssignmentFilterDto filterDto)
        {
            if (filterDto == null)
            {
                _logger.LogWarning("Admin AssignmentHandler: Filter parameters null");
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            _logger.LogInformation("Admin AssignmentHandler: Querying assignments");
            PagedResultDto<AssignmentResponseDto> pagedAssignments = await _assignmentService.GetAssignmentsAsync(filterDto);
            _logger.LogInformation("Admin AssignmentHandler: Retrieved {Count} assignments", pagedAssignments.TotalCount);
            return new OkObjectResult(pagedAssignments);
        }
    }
}
