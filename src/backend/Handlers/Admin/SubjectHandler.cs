using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class SubjectHandler
    {
        private readonly ISubjectService _subjectService;
        private readonly ILogger<SubjectHandler> _logger;

        public SubjectHandler(ISubjectService subjectService, ILogger<SubjectHandler> logger)
        {
            _subjectService = subjectService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetSubjectsAsync(SubjectFilterDto filterDto)
        {
            if (filterDto == null)
            {
                _logger.LogWarning("Admin SubjectHandler: Filter parameters null");
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            _logger.LogInformation("Admin SubjectHandler: Querying subjects");
            PagedResultDto<SubjectResponseDto> pagedSubjects = await _subjectService.GetSubjectsAsync(filterDto);
            _logger.LogInformation("Admin SubjectHandler: Retrieved {Count} subjects", pagedSubjects.TotalCount);
            return new OkObjectResult(pagedSubjects);
        }

        public async Task<IActionResult> HandleCreateSubjectAsync(SubjectCreateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Admin SubjectHandler: Subject data null");
                return new BadRequestObjectResult("Subject data is required.");
            }
            _logger.LogInformation("Admin SubjectHandler: Creating subject Code:{Code}", dto.Code);
            var createdSubject = await _subjectService.CreateSubjectAsync(dto);
            _logger.LogInformation("Admin SubjectHandler: Created subject Id:{SubjectId}", createdSubject.Id);
            return new CreatedAtActionResult(nameof(AdminController.Subjects), "Admin", new { id = createdSubject.Id }, createdSubject);
        }

        public async Task<IActionResult> HandleUpdateAsync(Guid id, SubjectUpdateDto dto)
        {
            // Validation logic can go here
            if (dto == null)
            {
                _logger.LogWarning("Admin SubjectHandler: Update data null");
                return new BadRequestObjectResult("Subject data is required.");
            }

            _logger.LogInformation("Admin SubjectHandler: Updating subject Id:{SubjectId}", id);
            await _subjectService.UpdateSubjectAsync(id, dto);
            _logger.LogInformation("Admin SubjectHandler: Updated subject Id:{SubjectId}", id);
            return new NoContentResult(); // Returns 204 No Content
        }

        public async Task<IActionResult> HandleDeleteSubjectAsync(Guid id)
        {
            _logger.LogInformation("Admin SubjectHandler: Deleting subject Id:{SubjectId}", id);
            await _subjectService.DeleteSubjectAsync(id);
            _logger.LogInformation("Admin SubjectHandler: Deleted subject Id:{SubjectId}", id);
            return new NoContentResult();
        }
    }
}
