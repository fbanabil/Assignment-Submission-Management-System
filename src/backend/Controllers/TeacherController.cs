using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAssignmentService _assignmentService;
        private readonly ISubmissionService _submissionService;
        private readonly ITeacherAssignmentService _teacherAssignmentService;

        public TeacherController(IUserService userService, IAssignmentService assignmentService, ISubmissionService submissionService, ITeacherAssignmentService teacherAssignmentService)
        {
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _teacherAssignmentService = teacherAssignmentService;
        }

        /// <summary>
        /// This endpoint retrieves the dashboard data for a teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard([FromQuery] TeacherDashboardFilterDto dto, [FromQuery] Guid teacherId)
        {
            TeacherDashboardResponseDto dashboardResponseDto = new TeacherDashboardResponseDto();

            (dashboardResponseDto.TeacherName, dashboardResponseDto.TeacherEmail, teacherId) = await _userService.GetTeacherNameAndEmail(User, teacherId);

            dashboardResponseDto.TotalAssignedClasses = await _assignmentService.GetTotalAssignedClassesCount(teacherId);
            dashboardResponseDto.UngradedSubmissionsCount = await _submissionService.GetUngradedSubmissionsCount(teacherId);
            dashboardResponseDto.ActiveAssignmentsCount = await _assignmentService.GetActiveAssignmentsCount(teacherId);
            dashboardResponseDto.AssignedClasses = await _teacherAssignmentService.GetAssignedClasses(teacherId);
            dashboardResponseDto.UpcomingDeadlines = await _assignmentService.GetUpcomingDeadlines(teacherId);
            dashboardResponseDto.UpcomingDeadlinesCount = dashboardResponseDto.UpcomingDeadlines.Count;

            return Ok(dashboardResponseDto);
        }

        /// <summary>
        /// This endpoint retrieves a paginated and filtered list of classes assigned to the requesting teacher.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Classes([FromQuery] TeacherClassFilterDto dto, [FromQuery] Guid teacherId)
        {
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(User, teacherId);
            PagedResultDto<TeacherAssignedClassSubjectDto> result = await _teacherAssignmentService.GetAssignedClassesPagedAsync(teacherId, dto);
            return Ok(result);
        }

        /// <summary>
        /// This endpoint retrieves a paginated list of assignments for a teacher based on filter criteria.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] AssignmentFilterDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Filter parameters are required.");
            }

            PagedResultDto<AssignmentResponseDto> assignments = await _assignmentService.GetAssignmentsForTeacher(dto);
            return Ok(assignments);
        }

        /// <summary>
        /// This endpoint allows a teacher to create a new assignment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Assignments([FromBody] AssignmentCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Assignment data is required.");
            }
            AssignmentResponseDto response = await _assignmentService.CreateAssignmentAsync(dto);
            return StatusCode(201, response);
        }

        /// <summary>
        /// This endpoint allows a teacher to update an existing assignment.
        /// </summary>
        [HttpPut("Assignments/{id}")]
        public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] AssignmentUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Assignment data is required.");
            }
            AssignmentResponseDto response = await _assignmentService.UpdateAssignmentAsync(id, dto);
            return NoContent();
        }

        /// <summary>
        /// This endpoint retrieves student submissions for teacher assignments.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Submissions([FromQuery] SubmissionFilterDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Filter parameters are required.");
            }

            PagedResultDto<SubmissionResponseDto> submissions = await _submissionService.GetSubmissionsAsync(dto);
            return Ok(submissions);
        }

        /// <summary>
        /// This endpoint allows a teacher to grade a student submission and leave feedback.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeDto dto, [FromQuery] Guid teacherId)
        {
            if (dto == null)
            {
                return BadRequest("Grade data is required.");
            }
            (_, _, teacherId) = await _userService.GetTeacherNameAndEmail(User, teacherId);
            await _submissionService.GradeSubmissionAsync(dto, teacherId);
            return Ok(new { message = "Submission graded successfully." });
        }
    }
}
