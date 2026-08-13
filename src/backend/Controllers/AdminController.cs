using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.ClassDTOs;
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

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IUserService userService, IAssignmentService assignmentService, ISubmissionService submissionService, IClassService classService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _classService = classService;
            _submissionService = submissionService;
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
    }
}
