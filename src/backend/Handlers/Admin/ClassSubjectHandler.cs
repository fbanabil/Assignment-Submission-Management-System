using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs.ClassSubjectDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class ClassSubjectHandler
    {
        private readonly IClassSubjectService _classSubjectService;

        public ClassSubjectHandler(IClassSubjectService classSubjectService)
        {
            _classSubjectService = classSubjectService;
        }

        public async Task<IActionResult> HandleCreateClassSubjectAsync(ClassSubjectCreateDto dto)
        {
            if (dto == null)
            {
                return new BadRequestObjectResult("ClassSubject data is required.");
            }
            var createdClassSubject = await _classSubjectService.CreateClassSubjectAsync(dto);
            return new CreatedAtActionResult(nameof(AdminController.ClassSubjects), "Admin", new { id = createdClassSubject.Id }, createdClassSubject);
        }

        public async Task<IActionResult> HandleDeleteClassSubjectAsync(Guid classId, Guid subjectId)
        {
            // Validate the input parameters
            if (classId == Guid.Empty || subjectId == Guid.Empty)
            {
                return new BadRequestObjectResult("ClassId and SubjectId are required.");
            }

            // Check if the ClassSubject association exists
            var classSubjects = await _classSubjectService.GetAllClassSubjectsAsync();

            // Find the specific ClassSubject association to delete
            var classSubjectToDelete = classSubjects.FirstOrDefault(cs => cs.ClassId == classId && cs.SubjectId == subjectId);

            // If the association does not exist, return a NotFound response
            if (classSubjectToDelete == null)
            {
                return new NotFoundObjectResult("ClassSubject association not found.");
            }

            // Delete the ClassSubject association
            await _classSubjectService.DeleteClassSubjectAsync(classSubjectToDelete.Id);
            return new NoContentResult();
        }
    }
}
