using Backend.DTOs.StudentDTOs;
using Backend.Handlers.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public StudentController(
            StudentDashboardHandler dashboardHandler,
            StudentAssignmentHandler assignmentHandler,
            IWebHostEnvironment environment)
        {
            _dashboardHandler = dashboardHandler;
            _assignmentHandler = assignmentHandler;
            _environment = environment;
        }

        /// <summary>
        /// Retrieves the dashboard summary for the current student.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard([FromQuery] Guid? studentId)
        {
            return await _dashboardHandler.HandleDashboardAsync(studentId);
        }

        /// <summary>
        /// Retrieves published assignments for the student's enrolled classes with pending/submitted/graded filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] StudentAssignmentFilterDto filterDto)
        {
            return await _assignmentHandler.HandleGetStudentAssignmentsAsync(filterDto);
        }

        /// <summary>
        /// Retrieves full details, deadline countdown data, and submission status for a specific assignment.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> AssignmentDetail([FromRoute] Guid id)
        {
            return await _assignmentHandler.HandleGetStudentAssignmentDetailAsync(id);
        }

        /// <summary>
        /// Submits work for an assignment (supports text and/or file URL, handles resubmission logic).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Submissions([FromBody] StudentSubmissionCreateDto dto)
        {
            return await _assignmentHandler.HandleCreateStudentSubmissionAsync(dto);
        }

        /// <summary>
        /// Uploads a submission attachment file to /wwwroot/assignments/ with a unique name and returns the relative path /assignments/*.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            return await _assignmentHandler.HandleFileUploadAsync(file, _environment);
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
            return await _assignmentHandler.HandleUnsubmitAssignmentAsync(id);
        }

        /// <summary>
        /// Retrieves the history of all submissions, marks, and feedback for the requesting student.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MySubmissions([FromQuery] StudentSubmissionHistoryFilterDto filterDto)
        {
            return await _assignmentHandler.HandleGetStudentSubmissionsHistoryAsync(filterDto);
        }
    }
}
