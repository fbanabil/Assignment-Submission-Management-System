using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Auth
{
    public class UserAuthHandler
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserAuthHandler> _logger;

        public UserAuthHandler(IUserService userService, ILogger<UserAuthHandler> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleCreateUserAsync(UserCreateDto dto)
        {
            _logger.LogInformation("UserAuthHandler: Creating user for Email:{Email}, Role:{Role}", dto?.Email, dto?.Role);
            var user = await _userService.CreateUserAsync(dto);
            _logger.LogInformation("UserAuthHandler: User created with Id:{UserId}", user.Id);
            return new ObjectResult(new { user.Id, user.FullName, user.Email, user.Role })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<IActionResult> HandleUpdateUserAsync(UserUpdateDto userUpdateDto, Guid id)
        {
            _logger.LogInformation("UserAuthHandler: Updating user UserId:{UserId}", id);
            if (id != userUpdateDto.Id)
            {
                _logger.LogWarning("UserAuthHandler: Route ID {RouteId} does not match Body ID {BodyId}", id, userUpdateDto.Id);
                return new ObjectResult(new { message = "User ID in the route does not match the ID in the request body." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Check if the user exists
            var user = await _userService.GetUserByIdAsync(userUpdateDto.Id);
            if (user == null)
            {
                _logger.LogWarning("UserAuthHandler: User {UserId} not found", id);
                return new NotFoundObjectResult(new { message = "User not found." });
            }

            // Update the user details
            await _userService.UpdateUserAsync(userUpdateDto.Id, userUpdateDto);
            _logger.LogInformation("UserAuthHandler: User {UserId} updated successfully", id);
            return new OkObjectResult(new { message = "User updated successfully." });
        }

        public async Task<IActionResult> HandleDeleteUserAsync(Guid id)
        {
            _logger.LogInformation("UserAuthHandler: Deleting user UserId:{UserId}", id);
            // Check if the user exists
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("UserAuthHandler: User {UserId} not found for deletion", id);
                return new NotFoundObjectResult(new { message = "User not found." });
            }

            // Delete the user
            await _userService.DeleteUserAsync(id);
            _logger.LogInformation("UserAuthHandler: User {UserId} deleted successfully", id);
            return new OkObjectResult(new { message = "User deleted successfully." });
        }
    }
}
