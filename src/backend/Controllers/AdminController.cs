using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.ClassSubjectDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly DashboardHandler _dashboardHandler;
        private readonly UserHandler _userHandler;
        private readonly ClassHandler _classHandler;
        private readonly SubjectHandler _subjectHandler;
        private readonly ClassSubjectHandler _classSubjectHandler;
        private readonly TeacherAssignmentHandler _teacherAssignmentHandler;
        private readonly AssignmentHandler _assignmentHandler;
        private readonly SubmissionHandler _submissionHandler;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            DashboardHandler dashboardHandler,
            UserHandler userHandler,
            ClassHandler classHandler,
            SubjectHandler subjectHandler,
            ClassSubjectHandler classSubjectHandler,
            TeacherAssignmentHandler teacherAssignmentHandler,
            AssignmentHandler assignmentHandler,
            SubmissionHandler submissionHandler,
            ILogger<AdminController> logger)
        {
            _dashboardHandler = dashboardHandler;
            _userHandler = userHandler;
            _classHandler = classHandler;
            _subjectHandler = subjectHandler;
            _classSubjectHandler = classSubjectHandler;
            _teacherAssignmentHandler = teacherAssignmentHandler;
            _assignmentHandler = assignmentHandler;
            _submissionHandler = submissionHandler;
            _logger = logger;
        }




        /// <summary>
        /// This endpoint provides a summary of the dashboard, including user, assignment, and submission statistics.
        /// </summary>
        /// <returns>A DashboardSummaryDto containing the summary statistics.</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> Dashboard()
        {
            _logger.LogInformation("AdminController: Dashboard summary requested");
            var result = await _dashboardHandler.HandleDashboardAsync();
            _logger.LogInformation("AdminController: Dashboard summary executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of users based on the provided filter parameters. It allows filtering by user ID, name, email, phone number, role, and active status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria for retrieving users.</param>
        /// <returns>A PagedResultDto containing the filtered user data and pagination information.</returns>
        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] UserFilterDto filterDto)
        {
            _logger.LogInformation("AdminController: Get Users requested");
            var result = await _userHandler.HandleGetUsersAsync(filterDto);
            _logger.LogInformation("AdminController: Get Users executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of classes based on the provided filter parameters. It allows filtering by class name, section, and academic year. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria for retrieving classes.</param>
        /// <returns>A PagedResultDto containing the filtered class data and pagination information.</returns>
        [HttpGet]
        public async Task<IActionResult> Classes([FromQuery] ClassFilterDto filterDto)
        {
            _logger.LogInformation("AdminController: Get Classes requested");
            var result = await _classHandler.HandleGetClassesAsync(filterDto);
            _logger.LogInformation("AdminController: Get Classes executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows the creation of a new class. It accepts a ClassCreateDto object containing the necessary information for creating a class, such as name, section, and academic year. Upon successful creation, it returns the created class data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the class details.</param>
        /// <returns>The created class data.</returns>
        [HttpPost]
        public async Task<IActionResult> Classes([FromBody] ClassCreateDto dto)
        {
            _logger.LogInformation("AdminController: Create Class requested with Name:{ClassName}", dto?.Name);
            var result = await _classHandler.HandleCreateClassAsync(dto);
            _logger.LogInformation("AdminController: Create Class executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows updating an existing class based on the provided class ID. It accepts a ClassUpdateDto object containing the updated information for the class, such as name, section, and academic year. Upon successful update, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the class to update.</param>
        /// <param name="dto">The data transfer object containing the updated class details.</param>
        /// <returns>A 204 No Content status code upon successful update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Classes([FromRoute] Guid id, [FromBody] ClassUpdateDto dto)
        {
            _logger.LogInformation("AdminController: Update Class requested for ClassId:{ClassId}", id);
            var result = await _classHandler.HandleUpdateClassAsync(id, dto);
            _logger.LogInformation("AdminController: Update Class executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows deleting an existing class based on the provided class ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the class to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass([FromRoute] Guid id)
        {
            _logger.LogInformation("AdminController: Delete Class requested for ClassId:{ClassId}", id);
            var result = await _classHandler.HandleDeleteClassAsync(id);
            _logger.LogInformation("AdminController: Delete Class executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of subjects based on the provided filter parameters. It allows filtering by subject name, code, and associated class ID. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter parameters for retrieving subjects.</param>
        /// <returns>A paginated list of subjects matching the filter criteria.</returns>
        [HttpGet]
        public async Task<IActionResult> Subjects([FromQuery] SubjectFilterDto filterDto)
        {
            _logger.LogInformation("AdminController: Get Subjects requested");
            var result = await _subjectHandler.HandleGetSubjectsAsync(filterDto);
            _logger.LogInformation("AdminController: Get Subjects executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows the creation of a new subject. It accepts a SubjectCreateDto object containing the necessary information for creating a subject, such as name and code. Upon successful creation, it returns the created subject data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the subject details.</param>
        /// <returns>The created subject data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> Subjects([FromBody] SubjectCreateDto dto)
        {
            _logger.LogInformation("AdminController: Create Subject requested with Code:{SubjectCode}", dto?.Code);
            var result = await _subjectHandler.HandleCreateSubjectAsync(dto);
            _logger.LogInformation("AdminController: Create Subject executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows updating an existing subject based on the provided subject ID. It accepts a SubjectUpdateDto object containing the updated information for the subject, such as name and code. Upon successful update, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the subject to update.</param>
        /// <param name="dto">The data transfer object containing the updated subject details.</param>
        /// <returns>A 204 No Content status code upon successful update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Subjects([FromRoute] Guid id, [FromBody] SubjectUpdateDto dto)
        {
            _logger.LogInformation("AdminController: Update Subject requested for SubjectId:{SubjectId}", id);
            var result = await _subjectHandler.HandleUpdateAsync(id, dto);
            _logger.LogInformation("AdminController: Update Subject executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows deleting an existing subject based on the provided subject ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject([FromRoute] Guid id)
        {
            _logger.LogInformation("AdminController: Delete Subject requested for SubjectId:{SubjectId}", id);
            var result = await _subjectHandler.HandleDeleteSubjectAsync(id);
            _logger.LogInformation("AdminController: Delete Subject executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows the creation of a new class-subject association. It accepts a ClassSubjectCreateDto object containing the necessary information for creating the association, such as class ID and subject ID. Upon successful creation, it returns the created class-subject data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the class and subject IDs.</param>
        /// <returns>The created class-subject data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> ClassSubjects([FromBody] ClassSubjectCreateDto dto)
        {
            _logger.LogInformation("AdminController: Create ClassSubject requested for ClassId:{ClassId}, SubjectId:{SubjectId}", dto?.ClassId, dto?.SubjectId);
            var result = await _classSubjectHandler.HandleCreateClassSubjectAsync(dto);
            _logger.LogInformation("AdminController: Create ClassSubject executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows deleting an existing class-subject association based on the provided class ID and subject ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        /// <param name="subjectId">The ID of the subject.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("ClassSubjects")]
        public async Task<IActionResult> DeleteClassSubject([FromQuery] Guid classId, [FromQuery] Guid subjectId)
        {
            _logger.LogInformation("AdminController: Delete ClassSubject requested for ClassId:{ClassId}, SubjectId:{SubjectId}", classId, subjectId);
            var result = await _classSubjectHandler.HandleDeleteClassSubjectAsync(classId, subjectId);
            _logger.LogInformation("AdminController: Delete ClassSubject executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of teacher assignments based on the provided filter parameters. It allows filtering by teacher name, email, class name, and subject code. The results are returned in a paginated format.
        /// </summary>
        /// <param name="dto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of teacher assignments.</returns>
        [HttpGet]
        public async Task<IActionResult> TeacherAssignments([FromQuery] TeacherAssignmentFilterDto dto)
        {
            _logger.LogInformation("AdminController: Get TeacherAssignments requested");
            var result = await _teacherAssignmentHandler.HandleGetTeacherAssignmentsAsync(dto);
            _logger.LogInformation("AdminController: Get TeacherAssignments executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows the creation of a new teacher assignment. It accepts a TeacherAssignmentCreateDto object containing the necessary information for creating the assignment, such as teacher ID and class-subject ID. Upon successful creation, it returns the created teacher assignment data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the teacher assignment details.</param>
        /// <returns>The created teacher assignment data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> TeacherAssignments([FromBody] TeacherAssignmentCreateDto dto)
        {
            _logger.LogInformation("AdminController: Create TeacherAssignment requested for TeacherId:{TeacherId}", dto?.TeacherId);
            var result = await _teacherAssignmentHandler.HandleCreateTeacherAssignmentAsync(dto);
            _logger.LogInformation("AdminController: Create TeacherAssignment executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint allows deleting an existing teacher assignment based on the provided assignment ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the teacher assignment to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("TeacherAssignments/{id}")]
        public async Task<IActionResult> DeleteTeacherAssignment([FromRoute] Guid id)
        {
            _logger.LogInformation("AdminController: Delete TeacherAssignment requested for Id:{Id}", id);
            var result = await _teacherAssignmentHandler.HandleDeleteTeacherAssignmentAsync(id);
            _logger.LogInformation("AdminController: Delete TeacherAssignment executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of assignments based on the provided filter parameters. It allows filtering by title, class name, teacher name, and status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of assignments.</returns>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] AssignmentFilterDto filterDto)
        {
            _logger.LogInformation("AdminController: Get Assignments requested");
            var result = await _assignmentHandler.HandleGetAssignmentsAsync(filterDto);
            _logger.LogInformation("AdminController: Get Assignments executed successfully");
            return result;
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of submissions based on the provided filter parameters. It allows filtering by class name, assignment title, student name, student email, and submission status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of submissions.</returns>
        [HttpGet]
        public async Task<IActionResult> Submissions([FromQuery] SubmissionFilterDto filterDto)
        {
            _logger.LogInformation("AdminController: Get Submissions requested");
            var result = await _submissionHandler.HandleGetSubmissionsAsync(filterDto);
            _logger.LogInformation("AdminController: Get Submissions executed successfully");
            return result;
        }
    }
}
