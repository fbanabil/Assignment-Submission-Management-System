using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class ClassHandler
    {
        private readonly IClassService _classService;

        public ClassHandler(IClassService classService)
        {
            _classService = classService;
        }

        public async Task<IActionResult> HandleGetClassesAsync(ClassFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            PagedResultDto<ClassResponseDto> pagedClasses = await _classService.GetClassesAsync(filterDto);
            return new OkObjectResult(pagedClasses);
        }

        public async Task<IActionResult> HandleCreateClassAsync(ClassCreateDto dto)
        {
            if (dto == null)
            {
                return new BadRequestObjectResult("Class data is required.");
            }
            var createdClass = await _classService.CreateClassAsync(dto);
            return new CreatedAtActionResult(nameof(AdminController.Classes), "Admin", new { id = createdClass.Id }, createdClass);
        }

        public async Task<IActionResult> HandleUpdateClassAsync(Guid id, ClassUpdateDto dto)
        {
            if (dto == null)
            {
                return new BadRequestObjectResult("Class data is required.");
            }
            await _classService.UpdateClassAsync(id, dto);
            return new NoContentResult();
        }

        public async Task<IActionResult> HandleDeleteClassAsync(Guid id)
        {
            await _classService.DeleteClassAsync(id);
            return new NoContentResult();
        }
    }
}
