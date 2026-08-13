using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthController(ILogger<AdminController> logger, IConfiguration configuration, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }




        /// <summary>
        /// This endpoint creates a new user in the system. It accepts a UserCreateDto object containing the user's details, such as full name, email, phone number, password, and role. The method calls the CreateUserAsync method of the IUserService to create the user and returns a 201 Created status code along with the newly created user's ID, full name, email, and role.
        /// </summary>
        /// <param name="dto">The UserCreateDto object containing the user's details.</param>
        /// <returns>An IActionResult representing the result of the create user operation.</returns>
        [HttpPost("/api/admin/users")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            return StatusCode(StatusCodes.Status201Created, new { user.Id, user.FullName, user.Email, user.Role });
        }






        /// <summary>
        /// This endpoint authenticates a user by verifying their email and password. It accepts a UserLoginDto object containing the user's email and password. The method calls the AuthenticateUserAsync method of the IUserService to validate the credentials. If the credentials are valid, it generates a JWT token and a refresh token, sets the refresh token as an HttpOnly cookie, and returns the JWT token in the response. If the credentials are invalid, it returns an Unauthorized status code with an error message.
        /// </summary>
        /// <param name="dto">The UserLoginDto object containing the user's email and password.</param>
        /// <returns>An IActionResult representing the result of the login operation.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            // Validate the user credentials
            User? user = await _userService.AuthenticateUserAsync(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }


            // Generate JWT token and refresh token
            string token = await _userService.GenerateJwtToken(user);
            string refreshToken = await _userService.GenerateRefreshToken(user);

            // Set refreshToken as HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Set to true in production for HTTPS
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            return Ok(new { token });
        }




        /// <summary>
        /// This endpoint refreshes the JWT token using a valid refresh token. It checks for the presence of the refresh token in the request cookies. If the refresh token is missing or invalid, it returns an Unauthorized status code with an error message. If the refresh token is valid, it generates a new JWT token and a new refresh token, updates the refresh token in the database, sets the new refresh token as an HttpOnly cookie, and returns the new JWT token in the response.
        /// </summary>
        /// <returns>An IActionResult representing the result of the refresh token operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            // Check if the refresh token is present in the request cookies
            if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing." });
            }

            // Validate the refresh token and get the associated user
            User? user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            // Generate a new JWT token and refresh token
            string newToken = await _userService.GenerateJwtToken(user);
            string newRefreshToken = await _userService.GenerateRefreshToken(user);
            // Update the refresh token in the database
            // Set new refreshToken as HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Set to true in production for HTTPS
            };
            Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);
            
            return Ok(new { token = newToken });
        }





        /// <summary>
        /// This endpoint logs out the user by invalidating the refresh token and JWT token. It checks for the presence of the refresh token in the request cookies. If the refresh token is missing or invalid, it returns an Unauthorized status code with an error message. If the refresh token is valid, it deletes the refresh token cookie, invalidates both the refresh token and JWT token in the database, and returns a success message indicating that the user has been logged out successfully.
        /// </summary>
        /// <returns>An IActionResult representing the result of the logout operation.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Check if the refresh token is present in the request cookies
            if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing." });
            }

            // Validate the refresh token and get the associated user
            User? user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }
            // Invalidate the refresh token in the database
            Response.Cookies.Delete("refreshToken");

            // Invalidate the JWT token in the database
            string? jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            string? refreshTokenFromCookie = Request.Cookies["refreshToken"];


            // Check if the JWT token or refresh token is missing
            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(refreshTokenFromCookie))
            {
                return BadRequest(new { message = "JWT token or refresh token is missing." });
            }

            // Invalidate the JWT token and refresh token in the database
            await _userService.InvalidateRefreshTokenAndJwtToken(jwtToken, refreshTokenFromCookie);

            return Ok(new { message = "Logged out successfully." });
        }





        /// <summary>
        /// This endpoint updates the details of an existing user. It accepts a UserUpdateDto object containing the user's ID and the updated details such as full name, email, phone number, role, and active status. The method first checks if the user exists by calling GetUserByIdAsync. If the user is not found, it returns a NotFound status code with an error message. If the user exists, it calls UpdateUserAsync to update the user's details and returns an Ok status code with a success message.
        /// </summary>
        /// <param name="userUpdateDto">The UserUpdateDto containing the updated user details.</param>
        /// <returns>An IActionResult representing the result of the update operation.</returns>
        [HttpPut("/api/Admin/Users/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto userUpdateDto, [FromRoute] Guid id)
        {
            if(id != userUpdateDto.Id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User ID in the route does not match the ID in the request body." });
            }

            // Check if the user exists
            var user = await _userService.GetUserByIdAsync(userUpdateDto.Id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            // Update the user details
            await _userService.UpdateUserAsync(userUpdateDto.Id, userUpdateDto);
            return Ok(new { message = "User updated successfully." });

        }



        /// <summary>
        /// This endpoint deletes an existing user from the system. It accepts a user ID as a parameter and checks if the user exists by calling GetUserByIdAsync. If the user is not found, it returns a NotFound status code with an error message. If the user exists, it calls DeleteUserAsync to remove the user from the system and returns an Ok status code with a success message.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An IActionResult representing the result of the delete operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // Check if the user exists
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Delete the user
            await _userService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
