using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IUserService userService, IAssignmentService assignmentService, ISubmissionService submissionService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
        }


        [HttpGet("summary")]
        [AllowAnonymous]
        public async Task<IActionResult> Dashboard()
        {
            DashboardSummaryDto dashboardSummaryDto = new DashboardSummaryDto();
            dashboardSummaryDto.DataSource = "Server";
            dashboardSummaryDto.FetchedAt = DateTime.UtcNow;
            dashboardSummaryDto.Users = await _userService.GetUserSummaryAsync();
            dashboardSummaryDto.Assignments = await _assignmentService.GetAssignmentSummaryAsync();
            dashboardSummaryDto.Submissions = await _submissionService.GetSubmissionSummaryAsync();

            return Ok(dashboardSummaryDto);           
        }
    }
}
