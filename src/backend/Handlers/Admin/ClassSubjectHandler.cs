using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs.ClassSubjectDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class ClassSubjectHandler
    {
        private readonly IClassSubjectService _classSubjectService;
        private readonly ILogger<ClassSubjectHandler> _logger;

        public ClassSubjectHandler(IClassSubjectService classSubjectService, ILogger<ClassSubjectHandler> logger)
        {
            _classSubjectService = classSubjectService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleCreateClassSubjectAsync(ClassSubjectCreateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Admin ClassSubjectHandler: ClassSubject data null");
                return new BadRequestObjectResult("ClassSubject data is required.");
            }
            _logger.LogInformation("Admin ClassSubjectHandler: Creating ClassSubject ClassId:{ClassId}, SubjectId:{SubjectId}", dto.ClassId, dto.SubjectId);
            var createdClassSubject = await _classSubjectService.CreateClassSubjectAsync(dto);
            _logger.LogInformation("Admin ClassSubjectHandler: Created ClassSubject Id:{Id}", createdClassSubject.Id);
            return new CreatedAtActionResult(nameof(AdminController.ClassSubjects), "Admin", new { id = createdClassSubject.Id }, createdClassSubject);
        }

        public async Task<IActionResult> HandleDeleteClassSubjectAsync(Guid classId, Guid subjectId)
        {
            // Validate the input parameters
            if (classId == Guid.Empty || subjectId == Guid.Empty)
            {
                _logger.LogWarning("Admin ClassSubjectHandler: ClassId or SubjectId empty");
                return new BadRequestObjectResult("ClassId and SubjectId are required.");
            }

            _logger.LogInformation("Admin ClassSubjectHandler: Deleting ClassSubject ClassId:{ClassId}, SubjectId:{SubjectId}", classId, subjectId);
            // Check if the ClassSubject association exists
            var classSubjects = await _classSubjectService.GetAllClassSubjectsAsync();

            // Find the specific ClassSubject association to delete
            var classSubjectToDelete = classSubjects.FirstOrDefault(cs => cs.ClassId == classId && cs.SubjectId == subjectId);

            // If the association does not exist, return a NotFound response
            if (classSubjectToDelete == null)
            {
                _logger.LogWarning("Admin ClassSubjectHandler: Association not found for ClassId:{ClassId}, SubjectId:{SubjectId}", classId, subjectId);
                return new NotFoundObjectResult("ClassSubject association not found.");
            }

            // Delete the ClassSubject association
            await _classSubjectService.DeleteClassSubjectAsync(classSubjectToDelete.Id);
            _logger.LogInformation("Admin ClassSubjectHandler: Deleted ClassSubject Id:{Id}", classSubjectToDelete.Id);
            return new NoContentResult();
        }
    }
}
