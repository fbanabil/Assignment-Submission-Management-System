using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.Handlers.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly TeacherDashboardHandler _dashboardHandler;
        private readonly TeacherClassHandler _classHandler;
        private readonly TeacherAssignmentHandler _assignmentHandler;
        private readonly TeacherSubmissionHandler _submissionHandler;
        private readonly TeacherEnrollmentHandler _enrollmentHandler;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            TeacherDashboardHandler dashboardHandler,
            TeacherClassHandler classHandler,
            TeacherAssignmentHandler assignmentHandler,
            TeacherSubmissionHandler submissionHandler,
            TeacherEnrollmentHandler enrollmentHandler,
            ILogger<TeacherController> logger)
        {
            _dashboardHandler = dashboardHandler;
            _classHandler = classHandler;
            _assignmentHandler = assignmentHandler;
            _submissionHandler = submissionHandler;
            _enrollmentHandler = enrollmentHandler;
            _logger = logger;
        }




        /// <summary>
        /// This endpoint retrieves the dashboard data for a teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard([FromQuery] TeacherDashboardFilterDto dto, [FromQuery] Guid teacherId)
        {
            _logger.LogInformation("TeacherController: Dashboard requested for TeacherId:{TeacherId}", teacherId);
            var result = await _dashboardHandler.HandleDashboardAsync(dto, teacherId);
            _logger.LogInformation("TeacherController: Dashboard executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated and filtered list of classes assigned to the requesting teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Classes([FromQuery] TeacherClassFilterDto dto, [FromQuery] Guid teacherId)
        {
            _logger.LogInformation("TeacherController: Classes requested for TeacherId:{TeacherId}", teacherId);
            var result = await _classHandler.HandleGetClassesAsync(dto, teacherId);
            _logger.LogInformation("TeacherController: Classes executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of assignments for a teacher based on filter criteria.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] AssignmentFilterDto dto)
        {
            _logger.LogInformation("TeacherController: Get Assignments requested");
            var result = await _assignmentHandler.HandleGetAssignmentsAsync(dto);
            _logger.LogInformation("TeacherController: Get Assignments executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows a teacher to create a new assignment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Assignments([FromBody] AssignmentCreateDto dto)
        {
            _logger.LogInformation("TeacherController: Create Assignment requested with Title:{Title}", dto?.Title);
            var result = await _assignmentHandler.HandleCreateAssignmentAsync(dto);
            _logger.LogInformation("TeacherController: Create Assignment executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows a teacher to update an existing assignment.
        /// </summary>
        [HttpPut("Assignments/{id}")]
        public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] AssignmentUpdateDto dto)
        {
            _logger.LogInformation("TeacherController: Update Assignment requested for Id:{Id}", id);
            var result = await _assignmentHandler.HandleUpdateAssignmentAsync(id, dto);
            _logger.LogInformation("TeacherController: Update Assignment executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves student submissions for teacher assignments.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Submissions([FromQuery] SubmissionFilterDto dto)
        {
            _logger.LogInformation("TeacherController: Get Submissions requested");
            var result = await _submissionHandler.HandleGetSubmissionsAsync(dto);
            _logger.LogInformation("TeacherController: Get Submissions executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows a teacher to grade a student submission and leave feedback.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeDto dto, [FromQuery] Guid teacherId)
        {
            _logger.LogInformation("TeacherController: GradeSubmission requested for SubmissionId:{SubmissionId}, TeacherId:{TeacherId}", dto?.SubmissionId, teacherId);
            var result = await _submissionHandler.HandleGradeSubmissionAsync(dto, teacherId);
            _logger.LogInformation("TeacherController: GradeSubmission executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves student enrollments for classes taught by the requesting teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Enrollments([FromQuery] StudentEnrollmentFilterDto dto, [FromQuery] Guid teacherId)
        {
            _logger.LogInformation("TeacherController: Get Enrollments requested for TeacherId:{TeacherId}", teacherId);
            var result = await _enrollmentHandler.HandleGetEnrollmentsAsync(dto, teacherId);
            _logger.LogInformation("TeacherController: Get Enrollments executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows a teacher to enroll a student in a class.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Enrollments([FromBody] StudentEnrollmentCreateDto dto)
        {
            _logger.LogInformation("TeacherController: Create Enrollment requested for Email:{Email}", dto?.StudentEmail);
            var result = await _enrollmentHandler.HandleCreateEnrollmentAsync(dto);
            _logger.LogInformation("TeacherController: Create Enrollment executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows a teacher to remove a student enrollment.
        /// </summary>
        [HttpDelete("Enrollments/{id}")]
        public async Task<IActionResult> DeleteEnrollment(Guid id)
        {
            _logger.LogInformation("TeacherController: Delete Enrollment requested for Id:{Id}", id);
            var result = await _enrollmentHandler.HandleDeleteEnrollmentAsync(id);
            _logger.LogInformation("TeacherController: Delete Enrollment executed successfully");
            return result;
        }
    }
}

