using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Auth
{
    public class UserAuthHandler
    {
        private readonly IUserService _userService;

        public UserAuthHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> HandleCreateUserAsync(UserCreateDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            return new ObjectResult(new { user.Id, user.FullName, user.Email, user.Role })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<IActionResult> HandleUpdateUserAsync(UserUpdateDto userUpdateDto, Guid id)
        {
            if (id != userUpdateDto.Id)
            {
                return new ObjectResult(new { message = "User ID in the route does not match the ID in the request body." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Check if the user exists
            var user = await _userService.GetUserByIdAsync(userUpdateDto.Id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found." });
            }

            // Update the user details
            await _userService.UpdateUserAsync(userUpdateDto.Id, userUpdateDto);
            return new OkObjectResult(new { message = "User updated successfully." });
        }

        public async Task<IActionResult> HandleDeleteUserAsync(Guid id)
        {
            // Check if the user exists
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found." });
            }

            // Delete the user
            await _userService.DeleteUserAsync(id);
            return new OkObjectResult(new { message = "User deleted successfully." });
        }
    }
}
