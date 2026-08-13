using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.ClassSubjectDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
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
        private readonly IClassService _classService;
        private readonly ISubjectService _subjectService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly ITeacherAssignmentService _teacherAssignmentService;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IUserService userService, IAssignmentService assignmentService, ISubmissionService submissionService, IClassService classService, ISubjectService subjectService, IClassSubjectService classSubjectService, ITeacherAssignmentService teacherAssignmentService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _classService = classService;
            _subjectService = subjectService;
            _classSubjectService = classSubjectService;
            _teacherAssignmentService = teacherAssignmentService;
        }




        /// <summary>
        /// This endpoint provides a summary of the dashboard, including user, assignment, and submission statistics.
        /// </summary>
        /// <returns>A DashboardSummaryDto containing the summary statistics.</returns>
        [HttpGet("summary")]
        [AllowAnonymous]
        public async Task<IActionResult> Dashboard()
        {
            DashboardSummaryDto dashboardSummaryDto = new DashboardSummaryDto();
            dashboardSummaryDto.DataSource = "Server";
            dashboardSummaryDto.FetchedAt = DateTime.UtcNow;

            // Fetching summary data from services
            dashboardSummaryDto.Users = await _userService.GetUserSummaryAsync();
            dashboardSummaryDto.Assignments = await _assignmentService.GetAssignmentSummaryAsync();
            dashboardSummaryDto.Submissions = await _submissionService.GetSubmissionSummaryAsync();

            return Ok(dashboardSummaryDto);
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of users based on the provided filter parameters. It allows filtering by user ID, name, email, phone number, role, and active status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria for retrieving users.</param>
        /// <returns>A PagedResultDto containing the filtered user data and pagination information.</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users([FromQuery] UserFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return BadRequest("Filter parameters are required.");
            }

            PagedResultDto<UserResponseDto> pagedUsers = await _userService.GetUsersAsync(filterDto);
            return Ok(pagedUsers);
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of classes based on the provided filter parameters. It allows filtering by class name, section, and academic year. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria for retrieving classes.</param>
        /// <returns>A PagedResultDto containing the filtered class data and pagination information.</returns>
        [HttpGet]
        public async Task<IActionResult> Classes([FromQuery] ClassFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return BadRequest("Filter parameters are required.");
            }
            PagedResultDto<ClassResponseDto> pagedClasses = await _classService.GetClassesAsync(filterDto);
            return Ok(pagedClasses);
        }




        /// <summary>
        /// This endpoint allows the creation of a new class. It accepts a ClassCreateDto object containing the necessary information for creating a class, such as name, section, and academic year. Upon successful creation, it returns the created class data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the class details.</param>
        /// <returns>The created class data.</returns>
        [HttpPost]
        public async Task<IActionResult> Classes([FromBody] ClassCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Class data is required.");
            }
            var createdClass = await _classService.CreateClassAsync(dto);
            return CreatedAtAction(nameof(Classes), new { id = createdClass.Id }, createdClass);
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
            if (dto == null)
            {
                return BadRequest("Class data is required.");
            }
            await _classService.UpdateClassAsync(id, dto);
            return NoContent();
        }




        /// <summary>
        /// This endpoint allows deleting an existing class based on the provided class ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the class to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass([FromRoute] Guid id)
        {
            await _classService.DeleteClassAsync(id);
            return NoContent();
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of subjects based on the provided filter parameters. It allows filtering by subject name, code, and associated class ID. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter parameters for retrieving subjects.</param>
        /// <returns>A paginated list of subjects matching the filter criteria.</returns>
        [HttpGet]
        public async Task<IActionResult> Subjects([FromQuery] SubjectFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return BadRequest("Filter parameters are required.");
            }
            PagedResultDto<SubjectResponseDto> pagedSubjects = await _subjectService.GetSubjectsAsync(filterDto);
            return Ok(pagedSubjects);
        }




        /// <summary>
        /// This endpoint allows the creation of a new subject. It accepts a SubjectCreateDto object containing the necessary information for creating a subject, such as name and code. Upon successful creation, it returns the created subject data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the subject details.</param>
        /// <returns>The created subject data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> Subjects([FromBody] SubjectCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Subject data is required.");
            }
            var createdSubject = await _subjectService.CreateSubjectAsync(dto);
            return CreatedAtAction(nameof(Subjects), new { id = createdSubject.Id }, createdSubject);
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
            if (dto == null)
            {
                return BadRequest("Subject data is required.");
            }
            await _subjectService.UpdateSubjectAsync(id, dto);
            return NoContent();
        }




        /// <summary>
        /// This endpoint allows deleting an existing subject based on the provided subject ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject([FromRoute] Guid id)
        {
            await _subjectService.DeleteSubjectAsync(id);
            return NoContent();
        }





        /// <summary>
        /// This endpoint allows the creation of a new class-subject association. It accepts a ClassSubjectCreateDto object containing the necessary information for creating the association, such as class ID and subject ID. Upon successful creation, it returns the created class-subject data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the class and subject IDs.</param>
        /// <returns>The created class-subject data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> ClassSubjects([FromBody] ClassSubjectCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("ClassSubject data is required.");
            }
            var createdClassSubject = await _classSubjectService.CreateClassSubjectAsync(dto);
            return CreatedAtAction(nameof(ClassSubjects), new { id = createdClassSubject.Id }, createdClassSubject);
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
            // Validate the input parameters
            if (classId == Guid.Empty || subjectId == Guid.Empty)
            {
                return BadRequest("ClassId and SubjectId are required.");
            }

            // Check if the ClassSubject association exists
            var classSubjects = await _classSubjectService.GetAllClassSubjectsAsync();

            // Find the specific ClassSubject association to delete
            var classSubjectToDelete = classSubjects.FirstOrDefault(cs => cs.ClassId == classId && cs.SubjectId == subjectId);

            // If the association does not exist, return a NotFound response
            if (classSubjectToDelete == null)
            {
                return NotFound("ClassSubject association not found.");
            }

            // Delete the ClassSubject association
            await _classSubjectService.DeleteClassSubjectAsync(classSubjectToDelete.Id);
            return NoContent();
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of teacher assignments based on the provided filter parameters. It allows filtering by teacher name, email, class name, and subject code. The results are returned in a paginated format.
        /// </summary>
        /// <param name="dto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of teacher assignments.</returns>
        [HttpGet]
        public async Task<IActionResult> TeacherAssignments([FromQuery] TeacherAssignmentFilterDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Filter parameters are required.");
            }
            PagedResultDto<TeacherAssignmentResponseDto> pagedTeacherAssignments = await _teacherAssignmentService.GetTeacherAssignmentsAsync(dto);
            return Ok(pagedTeacherAssignments);
        }





        /// <summary>
        /// This endpoint allows the creation of a new teacher assignment. It accepts a TeacherAssignmentCreateDto object containing the necessary information for creating the assignment, such as teacher ID and class-subject ID. Upon successful creation, it returns the created teacher assignment data along with a 201 Created status code.
        /// </summary>
        /// <param name="dto">The data transfer object containing the teacher assignment details.</param>
        /// <returns>The created teacher assignment data along with a 201 Created status code.</returns>
        [HttpPost]
        public async Task<IActionResult> TeacherAssignments([FromBody] TeacherAssignmentCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("TeacherAssignment data is required.");
            }
            var createdTeacherAssignment = await _teacherAssignmentService.CreateTeacherAssignmentAsync(dto);
            return CreatedAtAction(nameof(TeacherAssignments), new { id = createdTeacherAssignment.Id }, createdTeacherAssignment);
        }





        /// <summary>
        /// This endpoint allows deleting an existing teacher assignment based on the provided assignment ID. Upon successful deletion, it returns a 204 No Content status code.
        /// </summary>
        /// <param name="id">The ID of the teacher assignment to delete.</param>
        /// <returns>A 204 No Content status code upon successful deletion.</returns>
        [HttpDelete("TeacherAssignments/{id}")]
        public async Task<IActionResult> DeleteTeacherAssignment([FromRoute] Guid id)
        {
            await _teacherAssignmentService.DeleteTeacherAssignmentAsync(id);
            return NoContent();
        }




        /// <summary>
        /// This endpoint retrieves a paginated list of assignments based on the provided filter parameters. It allows filtering by title, class name, teacher name, and status. The results are returned in a paginated format.
        /// </summary>
        /// <param name="filterDto">The filter criteria and pagination information.</param>
        /// <returns>A paginated list of assignments.</returns>
        [HttpGet]
        public async Task<IActionResult> Assignments([FromQuery] AssignmentFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return BadRequest("Filter parameters are required.");
            }
            PagedResultDto<AssignmentResponseDto> pagedAssignments = await _assignmentService.GetAssignmentsAsync(filterDto);
            return Ok(pagedAssignments);
        }



    }
}
