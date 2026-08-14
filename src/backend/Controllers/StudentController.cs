using Backend.DTOs.StudentDTOs;
using Backend.Handlers.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Student,Admin")]
    public class StudentController : ControllerBase
    {
        private readonly StudentDashboardHandler _dashboardHandler;
        private readonly StudentAssignmentHandler _assignmentHandler;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            StudentDashboardHandler dashboardHandler,
            StudentAssignmentHandler assignmentHandler,
            IWebHostEnvironment environment,
            ILogger<StudentController> logger)
        {
            _dashboardHandler = dashboardHandler;
            _assignmentHandler = assignmentHandler;
            _environment = environment;
            _logger = logger;
        }




        /// <summary>
        /// Retrieves the dashboard summary for the current student.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard([FromQuery] Guid? studentId)
        {
            _logger.LogInformation("StudentController: Dashboard requested for StudentId:{StudentId}", studentId);
            var result = await _dashboardHandler.HandleDashboardAsync(studentId);
            _logger.LogInformation("StudentController: Dashboard executed successfully");
            return result;
        }




        /// <summary>
        /// Retrieves published assignments for the student's enrolled classes with pending/submitted/graded filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] StudentAssignmentFilterDto filterDto)
        {
            _logger.LogInformation("StudentController: Get Assignments requested");
            var result = await _assignmentHandler.HandleGetStudentAssignmentsAsync(filterDto);
            _logger.LogInformation("StudentController: Get Assignments executed successfully");
            return result;
        }




        /// <summary>
        /// Retrieves full details, deadline countdown data, and submission status for a specific assignment.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> AssignmentDetail([FromRoute] Guid id)
        {
            _logger.LogInformation("StudentController: AssignmentDetail requested for AssignmentId:{AssignmentId}", id);
            var result = await _assignmentHandler.HandleGetStudentAssignmentDetailAsync(id);
            _logger.LogInformation("StudentController: AssignmentDetail executed successfully");
            return result;
        }




        /// <summary>
        /// Submits work for an assignment (supports text and/or file URL, handles resubmission logic).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Submissions([FromBody] StudentSubmissionCreateDto dto)
        {
            _logger.LogInformation("StudentController: Create Submission requested for AssignmentId:{AssignmentId}", dto?.AssignmentId);
            var result = await _assignmentHandler.HandleCreateStudentSubmissionAsync(dto);
            _logger.LogInformation("StudentController: Create Submission executed successfully");
            return result;
        }




        /// <summary>
        /// Uploads a submission attachment file to /wwwroot/assignments/ with a unique name and returns the relative path /assignments/*.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            _logger.LogInformation("StudentController: FileUpload requested for FileName:{FileName}", file?.FileName);
            var result = await _assignmentHandler.HandleFileUploadAsync(file, _environment);
            _logger.LogInformation("StudentController: FileUpload executed successfully");
            return result;
        }




        /// <summary>
        /// Unsubmits / deletes a student submission if resubmission is allowed and before deadline.
        /// Supports DELETE /api/Student/Submissions/{id}, DELETE /api/Student/Unsubmit/{id}, and POST /api/Student/Unsubmit/{id}.
        /// </summary>
        [HttpDelete("/api/Student/Submissions/{id}")]
        [HttpDelete("/api/Student/Unsubmit/{id}")]
        [HttpPost("/api/Student/Unsubmit/{id}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Unsubmit([FromRoute] Guid id)
        {
            _logger.LogInformation("StudentController: Unsubmit requested for SubmissionId:{Id}", id);
            var result = await _assignmentHandler.HandleUnsubmitAssignmentAsync(id);
            _logger.LogInformation("StudentController: Unsubmit executed successfully");
            return result;
        }




        /// <summary>
        /// Retrieves the history of all submissions, marks, and feedback for the requesting student.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MySubmissions([FromQuery] StudentSubmissionHistoryFilterDto filterDto)
        {
            _logger.LogInformation("StudentController: MySubmissions history requested");
            var result = await _assignmentHandler.HandleGetStudentSubmissionsHistoryAsync(filterDto);
            _logger.LogInformation("StudentController: MySubmissions history executed successfully");
            return result;
        }
    }
}
