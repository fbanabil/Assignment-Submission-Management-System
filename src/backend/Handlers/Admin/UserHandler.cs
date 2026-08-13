using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Admin
{
    public class UserHandler
    {
        private readonly IUserService _userService;

        public UserHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> HandleGetUsersAsync(UserFilterDto filterDto)
        {
            if (filterDto == null)
            {
                return new BadRequestObjectResult("Filter parameters are required.");
            }

            PagedResultDto<UserResponseDto> pagedUsers = await _userService.GetUsersAsync(filterDto);
            return new OkObjectResult(pagedUsers);
        }
    }
}
