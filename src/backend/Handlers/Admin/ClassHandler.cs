using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class ClassHandler
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassHandler> _logger;

        public ClassHandler(IClassService classService, ILogger<ClassHandler> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetClassesAsync(ClassFilterDto filterDto)
        {
            if (filterDto == null)
            {
                _logger.LogWarning("Admin ClassHandler: Filter parameters null");
                return new BadRequestObjectResult("Filter parameters are required.");
            }
            _logger.LogInformation("Admin ClassHandler: Querying classes");
            PagedResultDto<ClassResponseDto> pagedClasses = await _classService.GetClassesAsync(filterDto);
            _logger.LogInformation("Admin ClassHandler: Retrieved {Count} classes", pagedClasses.TotalCount);
            return new OkObjectResult(pagedClasses);
        }

        public async Task<IActionResult> HandleCreateClassAsync(ClassCreateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Admin ClassHandler: Class data null");
                return new BadRequestObjectResult("Class data is required.");
            }
            _logger.LogInformation("Admin ClassHandler: Creating class {ClassName}", dto.Name);
            var createdClass = await _classService.CreateClassAsync(dto);
            _logger.LogInformation("Admin ClassHandler: Created class with Id:{ClassId}", createdClass.Id);
            return new CreatedAtActionResult(nameof(AdminController.Classes), "Admin", new { id = createdClass.Id }, createdClass);
        }

        public async Task<IActionResult> HandleUpdateClassAsync(Guid id, ClassUpdateDto dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("Admin ClassHandler: Class update data null");
                return new BadRequestObjectResult("Class data is required.");
            }
            _logger.LogInformation("Admin ClassHandler: Updating class Id:{ClassId}", id);
            await _classService.UpdateClassAsync(id, dto);
            _logger.LogInformation("Admin ClassHandler: Updated class Id:{ClassId}", id);
            return new NoContentResult();
        }

        public async Task<IActionResult> HandleDeleteClassAsync(Guid id)
        {
            _logger.LogInformation("Admin ClassHandler: Deleting class Id:{ClassId}", id);
            await _classService.DeleteClassAsync(id);
            _logger.LogInformation("Admin ClassHandler: Deleted class Id:{ClassId}", id);
            return new NoContentResult();
        }
    }
}
