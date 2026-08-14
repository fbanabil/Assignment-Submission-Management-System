using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class SubmissionHandler
    {
        private readonly ISubmissionService _submissionService;
        private readonly ILogger<SubmissionHandler> _logger;

        public SubmissionHandler(ISubmissionService submissionService, ILogger<SubmissionHandler> logger)
        {
            _submissionService = submissionService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetSubmissionsAsync(SubmissionFilterDto filterDto)
        {
            if (filterDto == null)
            {
                _logger.LogWarning("Admin SubmissionHandler: Filter parameters null");
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            _logger.LogInformation("Admin SubmissionHandler: Querying submissions");
            PagedResultDto<SubmissionResponseDto> pagedSubmissions = await _submissionService.GetSubmissionsAsync(filterDto);
            _logger.LogInformation("Admin SubmissionHandler: Retrieved {Count} submissions", pagedSubmissions.TotalCount);
            return new OkObjectResult(pagedSubmissions);
        }
    }
}
