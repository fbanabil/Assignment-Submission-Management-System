using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Admin
{
    public class UserHandler
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserHandler> _logger;

        public UserHandler(IUserService userService, ILogger<UserHandler> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleGetUsersAsync(UserFilterDto filterDto)
        {
            if (filterDto == null)
            {
                _logger.LogWarning("Admin UserHandler: Filter parameters null");
                return new BadRequestObjectResult("Filter parameters are required.");
            }

            _logger.LogInformation("Admin UserHandler: Querying users");
            PagedResultDto<UserResponseDto> pagedUsers = await _userService.GetUsersAsync(filterDto);
            _logger.LogInformation("Admin UserHandler: Retrieved {Count} users", pagedUsers.TotalCount);
            return new OkObjectResult(pagedUsers);
        }
    }
}
