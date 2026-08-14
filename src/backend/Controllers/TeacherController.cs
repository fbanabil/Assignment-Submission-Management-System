using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.Handlers.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public TeacherController(
            TeacherDashboardHandler dashboardHandler,
            TeacherClassHandler classHandler,
            TeacherAssignmentHandler assignmentHandler,
            TeacherSubmissionHandler submissionHandler,
            TeacherEnrollmentHandler enrollmentHandler)
        {
            _dashboardHandler = dashboardHandler;
            _classHandler = classHandler;
            _assignmentHandler = assignmentHandler;
            _submissionHandler = submissionHandler;
            _enrollmentHandler = enrollmentHandler;
        }




        /// <summary>
        /// This endpoint retrieves the dashboard data for a teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard([FromQuery] TeacherDashboardFilterDto dto, [FromQuery] Guid teacherId)
            => await _dashboardHandler.HandleDashboardAsync(dto, teacherId);




        /// <summary>
        /// This endpoint retrieves a paginated and filtered list of classes assigned to the requesting teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Classes([FromQuery] TeacherClassFilterDto dto, [FromQuery] Guid teacherId)
            => await _classHandler.HandleGetClassesAsync(dto, teacherId);




        /// <summary>
        /// This endpoint retrieves a paginated list of assignments for a teacher based on filter criteria.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] AssignmentFilterDto dto)
            => await _assignmentHandler.HandleGetAssignmentsAsync(dto);





        /// <summary>
        /// This endpoint allows a teacher to create a new assignment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Assignments([FromBody] AssignmentCreateDto dto)
            => await _assignmentHandler.HandleCreateAssignmentAsync(dto);




        /// <summary>
        /// This endpoint allows a teacher to update an existing assignment.
        /// </summary>
        [HttpPut("Assignments/{id}")]
        public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] AssignmentUpdateDto dto)
            => await _assignmentHandler.HandleUpdateAssignmentAsync(id, dto);




        /// <summary>
        /// This endpoint retrieves student submissions for teacher assignments.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Submissions([FromQuery] SubmissionFilterDto dto)
            => await _submissionHandler.HandleGetSubmissionsAsync(dto);





        /// <summary>
        /// This endpoint allows a teacher to grade a student submission and leave feedback.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeDto dto, [FromQuery] Guid teacherId)
            => await _submissionHandler.HandleGradeSubmissionAsync(dto, teacherId);




        /// <summary>
        /// This endpoint retrieves student enrollments for classes taught by the requesting teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Enrollments([FromQuery] StudentEnrollmentFilterDto dto, [FromQuery] Guid teacherId)
            => await _enrollmentHandler.HandleGetEnrollmentsAsync(dto, teacherId);




        /// <summary>
        /// This endpoint allows a teacher to enroll a student in a class.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Enrollments([FromBody] StudentEnrollmentCreateDto dto)
            => await _enrollmentHandler.HandleCreateEnrollmentAsync(dto);




        /// <summary>
        /// This endpoint allows a teacher to remove a student enrollment.
        /// </summary>
        [HttpDelete("Enrollments/{id}")]
        public async Task<IActionResult> DeleteEnrollment(Guid id)
            => await _enrollmentHandler.HandleDeleteEnrollmentAsync(id);
    }
}

