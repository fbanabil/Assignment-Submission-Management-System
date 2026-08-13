using Backend.DTOs.UserDTOs;
using Backend.Handlers.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthHandler _authHandler;
        private readonly UserAuthHandler _userAuthHandler;

        public AuthController(AuthHandler authHandler, UserAuthHandler userAuthHandler)
        {
            _authHandler = authHandler;
            _userAuthHandler = userAuthHandler;
        }




        /// <summary>
        /// This endpoint creates a new user in the system. It accepts a UserCreateDto object containing the user's details, such as full name, email, phone number, password, and role. The method calls the CreateUserAsync method of the IUserService to create the user and returns a 201 Created status code along with the newly created user's ID, full name, email, and role.
        /// </summary>
        /// <param name="dto">The UserCreateDto object containing the user's details.</param>
        /// <returns>An IActionResult representing the result of the create user operation.</returns>
        [HttpPost("/api/admin/users")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
            => await _userAuthHandler.HandleCreateUserAsync(dto);




        /// <summary>
        /// This endpoint authenticates a user by verifying their email and password. It accepts a UserLoginDto object containing the user's email and password. The method calls the AuthenticateUserAsync method of the IUserService to validate the credentials. If the credentials are valid, it generates a JWT token and a refresh token, sets the refresh token as an HttpOnly cookie, and returns the JWT token in the response. If the credentials are invalid, it returns an Unauthorized status code with an error message.
        /// </summary>
        /// <param name="dto">The UserLoginDto object containing the user's email and password.</param>
        /// <returns>An IActionResult representing the result of the login operation.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
            => await _authHandler.HandleLoginAsync(dto);




        /// <summary>
        /// This endpoint refreshes the JWT token using a valid refresh token. It checks for the presence of the refresh token in the request cookies. If the refresh token is missing or invalid, it returns an Unauthorized status code with an error message. If the refresh token is valid, it generates a new JWT token and a new refresh token, updates the refresh token in the database, sets the new refresh token as an HttpOnly cookie, and returns the new JWT token in the response.
        /// </summary>
        /// <returns>An IActionResult representing the result of the refresh token operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
            => await _authHandler.HandleRefreshTokenAsync();




        /// <summary>
        /// This endpoint logs out the user by invalidating the refresh token and JWT token. It checks for the presence of the refresh token in the request cookies. If the refresh token is missing or invalid, it returns an Unauthorized status code with an error message. If the refresh token is valid, it deletes the refresh token cookie, invalidates both the refresh token and JWT token in the database, and returns a success message indicating that the user has been logged out successfully.
        /// </summary>
        /// <returns>An IActionResult representing the result of the logout operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
            => await _authHandler.HandleLogoutAsync();




        /// <summary>
        /// This endpoint updates the details of an existing user. It accepts a UserUpdateDto object containing the user's ID and the updated details such as full name, email, phone number, role, and active status. The method first checks if the user exists by calling GetUserByIdAsync. If the user is not found, it returns a NotFound status code with an error message. If the user exists, it calls UpdateUserAsync to update the user's details and returns an Ok status code with a success message.
        /// </summary>
        /// <param name="userUpdateDto">The UserUpdateDto containing the updated user details.</param>
        /// <returns>An IActionResult representing the result of the update operation.</returns>
        [HttpPut("/api/Admin/Users/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto userUpdateDto, [FromRoute] Guid id)
            => await _userAuthHandler.HandleUpdateUserAsync(userUpdateDto, id);




        /// <summary>
        /// This endpoint deletes an existing user from the system. It accepts a user ID as a parameter and checks if the user exists by calling GetUserByIdAsync. If the user is not found, it returns a NotFound status code with an error message. If the user exists, it calls DeleteUserAsync to remove the user from the system and returns an Ok status code with a success message.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An IActionResult representing the result of the delete operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
            => await _userAuthHandler.HandleDeleteUserAsync(id);
    }
}
