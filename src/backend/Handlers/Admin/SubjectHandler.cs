using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class SubjectHandler
    {
        private readonly ISubjectService _subjectService;

        public SubjectHandler(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        public async Task<IActionResult> HandleGetSubjectsAsync(SubjectFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            PagedResultDto<SubjectResponseDto> pagedSubjects = await _subjectService.GetSubjectsAsync(filterDto);
            return new OkObjectResult(pagedSubjects);
        }

        public async Task<IActionResult> HandleCreateSubjectAsync(SubjectCreateDto dto)
        {
            if (dto == null)
            {
                return new BadRequestObjectResult("Subject data is required.");
            }
            var createdSubject = await _subjectService.CreateSubjectAsync(dto);
            return new CreatedAtActionResult(nameof(AdminController.Subjects), "Admin", new { id = createdSubject.Id }, createdSubject);
        }

        public async Task<IActionResult> HandleUpdateAsync(Guid id, SubjectUpdateDto dto)
        {
            // Validation logic can go here
            if (dto == null) return new BadRequestObjectResult("Subject data is required.");

            await _subjectService.UpdateSubjectAsync(id, dto);
            return new NoContentResult(); // Returns 204 No Content
        }

        public async Task<IActionResult> HandleDeleteSubjectAsync(Guid id)
        {
            await _subjectService.DeleteSubjectAsync(id);
            return new NoContentResult();
        }
    }
}
