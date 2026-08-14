using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class DashboardHandler
    {
        private readonly IUserService _userService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;
        private readonly ILogger<DashboardHandler> _logger;

        public DashboardHandler(IUserService userService, IAssignmentService assignmentService, ISubmissionService submissionService, ILogger<DashboardHandler> logger)
        {
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleDashboardAsync()
        {
            _logger.LogInformation("Admin DashboardHandler: Aggregating dashboard summary metrics");
            DashboardSummaryDto dashboardSummaryDto = new DashboardSummaryDto();
            dashboardSummaryDto.DataSource = "Server";
            dashboardSummaryDto.FetchedAt = DateTime.UtcNow;

            // Fetching summary data from services
            dashboardSummaryDto.Users = await _userService.GetUserSummaryAsync();
            dashboardSummaryDto.Assignments = await _assignmentService.GetAssignmentSummaryAsync();
            dashboardSummaryDto.Submissions = await _submissionService.GetSubmissionSummaryAsync();

            _logger.LogInformation("Admin DashboardHandler: Aggregated total users:{TotalUsers}, assignments:{TotalAssignments}, submissions:{TotalSubmissions}",
                dashboardSummaryDto.Users.TotalUsers, dashboardSummaryDto.Assignments.TotalAssignments, dashboardSummaryDto.Submissions.TotalSubmissions);

            return new OkObjectResult(dashboardSummaryDto);
        }
    }
}
