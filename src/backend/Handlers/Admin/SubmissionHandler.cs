using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class SubmissionHandler
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionHandler(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        public async Task<IActionResult> HandleGetSubmissionsAsync(SubmissionFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            PagedResultDto<SubmissionResponseDto> pagedSubmissions = await _submissionService.GetSubmissionsAsync(filterDto);
            return new OkObjectResult(pagedSubmissions);
        }
    }
}
