using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class DashboardHandler
    {
        private readonly IUserService _userService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;

        public DashboardHandler(IUserService userService, IAssignmentService assignmentService, ISubmissionService submissionService)
        {
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
        }

        public async Task<IActionResult> HandleDashboardAsync()
        {
            DashboardSummaryDto dashboardSummaryDto = new DashboardSummaryDto();
            dashboardSummaryDto.DataSource = "Server";
            dashboardSummaryDto.FetchedAt = DateTime.UtcNow;

            // Fetching summary data from services
            dashboardSummaryDto.Users = await _userService.GetUserSummaryAsync();
            dashboardSummaryDto.Assignments = await _assignmentService.GetAssignmentSummaryAsync();
            dashboardSummaryDto.Submissions = await _submissionService.GetSubmissionSummaryAsync();

            return new OkObjectResult(dashboardSummaryDto);
        }
    }
}
