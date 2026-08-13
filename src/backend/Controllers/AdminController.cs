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

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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

        public AdminController(
            DashboardHandler dashboardHandler,
            UserHandler userHandler,
            ClassHandler classHandler,
            SubjectHandler subjectHandler,
            ClassSubjectHandler classSubjectHandler,
            TeacherAssignmentHandler teacherAssignmentHandler,
            AssignmentHandler assignmentHandler,
            SubmissionHandler submissionHandler)
        {
            _dashboardHandler = dashboardHandler;
            _userHandler = userHandler;
            _classHandler = classHandler;
            _subjectHandler = subjectHandler;
            _classSubjectHandler = classSubjectHandler;
            _teacherAssignmentHandler = teacherAssignmentHandler;
            _assignmentHandler = assignmentHandler;
            _submissionHandler = submissionHandler;
        }




        /// <summary>
        /// This endpoint provides a summary of the dashboard, including user, assignment, and submission statistics.
        /// </summary>
        /// <returns>A DashboardSummaryDto containing the summary statistics.</returns>
        [HttpGet("summary")]
        [AllowAnonymous]
        public async Task<IActionResult> Dashboard()
            => await _dashboardHandler.HandleDashboardAsync();



        /// <summary>
        /// This endpoint retrieves a paginated list of users based on the provided filter parameters. It allows filtering by user ID, name, email, phone number, role, and active status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria for retrieving users.</param>
        /// <returns>A PagedResultDto containing the filtered user data and pagination information.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users([FromQuery] UserFilterDto filterDto)
            => await _userHandler.HandleGetUsersAsync(filterDto);




        /// <summary>
        /// This endpoint retrieves a paginated list of classes based on the provided filter parameters. It allows filtering by class name, section, and academic year. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria for retrieving classes.</param>
        /// <returns>A PagedResultDto containing the filtered class data and pagination information.</returns>
        [HttpGet]
        public async Task<IActionResult> Classes([FromQuery] ClassFilterDto filterDto)
            => await _classHandler.HandleGetClassesAsync(filterDto);




        /// <summary>
        /// This endpoint allows the creation of a new class. It accepts a ClassCreateDto object containing the necessary information for creating a class, such as name, section, and academic year. Upon successful creation, it returns the created class data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the class details.</param>
        /// <returns>The created class data.</returns>
        [HttpPost]
        public async Task<IActionResult> Classes([FromBody] ClassCreateDto dto)
            => await _classHandler.HandleCreateClassAsync(dto);




        /// <summary>
        /// This endpoint allows updating an existing class based on the provided class ID. It accepts a ClassUpdateDto object containing the updated information for the class, such as name, section, and academic year. Upon successful update, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the class to update.</param>
        /// <param name="dto">The data transfer object containing the updated class details.</param>
        /// <returns>A 204 No Content status code upon successful update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Classes([FromRoute] Guid id, [FromBody] ClassUpdateDto dto)
            => await _classHandler.HandleUpdateClassAsync(id, dto);




        /// <summary>
        /// This endpoint allows deleting an existing class based on the provided class ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the class to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass([FromRoute] Guid id)
            => await _classHandler.HandleDeleteClassAsync(id);




        /// <summary>
        /// This endpoint retrieves a paginated list of subjects based on the provided filter parameters. It allows filtering by subject name, code, and associated class ID. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter parameters for retrieving subjects.</param>
        /// <returns>A paginated list of subjects matching the filter criteria.</returns>
        [HttpGet]
        public async Task<IActionResult> Subjects([FromQuery] SubjectFilterDto filterDto)
            => await _subjectHandler.HandleGetSubjectsAsync(filterDto);




        /// <summary>
        /// This endpoint allows the creation of a new subject. It accepts a SubjectCreateDto object containing the necessary information for creating a subject, such as name and code. Upon successful creation, it returns the created subject data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the subject details.</param>
        /// <returns>The created subject data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> Subjects([FromBody] SubjectCreateDto dto)
            => await _subjectHandler.HandleCreateSubjectAsync(dto);




        /// <summary>
        /// This endpoint allows updating an existing subject based on the provided subject ID. It accepts a SubjectUpdateDto object containing the updated information for the subject, such as name and code. Upon successful update, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the subject to update.</param>
        /// <param name="dto">The data transfer object containing the updated subject details.</param>
        /// <returns>A 204 No Content status code upon successful update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Subjects([FromRoute] Guid id, [FromBody] SubjectUpdateDto dto)
            => await _subjectHandler.HandleUpdateAsync(id, dto);




        /// <summary>
        /// This endpoint allows deleting an existing subject based on the provided subject ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject([FromRoute] Guid id)
            => await _subjectHandler.HandleDeleteSubjectAsync(id);




        /// <summary>
        /// This endpoint allows the creation of a new class-subject association. It accepts a ClassSubjectCreateDto object containing the necessary information for creating the association, such as class ID and subject ID. Upon successful creation, it returns the created class-subject data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the class and subject IDs.</param>
        /// <returns>The created class-subject data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> ClassSubjects([FromBody] ClassSubjectCreateDto dto)
            => await _classSubjectHandler.HandleCreateClassSubjectAsync(dto);




        /// <summary>
        /// This endpoint allows deleting an existing class-subject association based on the provided class ID and subject ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        /// <param name="subjectId">The ID of the subject.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("ClassSubjects")]
        public async Task<IActionResult> DeleteClassSubject([FromQuery] Guid classId, [FromQuery] Guid subjectId)
            => await _classSubjectHandler.HandleDeleteClassSubjectAsync(classId, subjectId);




        /// <summary>
        /// This endpoint retrieves a paginated list of teacher assignments based on the provided filter parameters. It allows filtering by teacher name, email, class name, and subject code. The results are returned in a paginated format.
        /// </summary>
        /// <param name="dto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of teacher assignments.</returns>
        [HttpGet]
        public async Task<IActionResult> TeacherAssignments([FromQuery] TeacherAssignmentFilterDto dto)
            => await _teacherAssignmentHandler.HandleGetTeacherAssignmentsAsync(dto);




        /// <summary>
        /// This endpoint allows the creation of a new teacher assignment. It accepts a TeacherAssignmentCreateDto object containing the necessary information for creating the assignment, such as teacher ID and class-subject ID. Upon successful creation, it returns the created teacher assignment data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the teacher assignment details.</param>
        /// <returns>The created teacher assignment data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> TeacherAssignments([FromBody] TeacherAssignmentCreateDto dto)
            => await _teacherAssignmentHandler.HandleCreateTeacherAssignmentAsync(dto);




        /// <summary>
        /// This endpoint allows deleting an existing teacher assignment based on the provided assignment ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the teacher assignment to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("TeacherAssignments/{id}")]
        public async Task<IActionResult> DeleteTeacherAssignment([FromRoute] Guid id)
            => await _teacherAssignmentHandler.HandleDeleteTeacherAssignmentAsync(id);




        /// <summary>
        /// This endpoint retrieves a paginated list of assignments based on the provided filter parameters. It allows filtering by title, class name, teacher name, and status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of assignments.</returns>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] AssignmentFilterDto filterDto)
            => await _assignmentHandler.HandleGetAssignmentsAsync(filterDto);




        /// <summary>
        /// This endpoint retrieves a paginated list of submissions based on the provided filter parameters. It allows filtering by class name, assignment title, student name, student email, and submission status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of submissions.</returns>
        [HttpGet]
        public async Task<IActionResult> Submissions([FromQuery] SubmissionFilterDto filterDto)
            => await _submissionHandler.HandleGetSubmissionsAsync(filterDto);
    }
}
