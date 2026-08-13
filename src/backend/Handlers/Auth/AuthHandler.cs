using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Handlers.Auth
{
    public class AuthHandler
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHandler(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> HandleLoginAsync(UserLoginDto dto)
        {
            // Validate the user credentials
            User? user = await _userService.AuthenticateUserAsync(dto.Email, dto.Password);
            if (user == null)
            {
                return new UnauthorizedObjectResult(new { message = "Invalid email or password." });
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

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            return new OkObjectResult(new { token });
        }

        public async Task<IActionResult> HandleRefreshTokenAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || !httpContext.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                return new UnauthorizedObjectResult(new { message = "Refresh token is missing." });
            }

            // Validate the refresh token and get the associated user
            User? user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return new UnauthorizedObjectResult(new { message = "Invalid refresh token." });
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
            httpContext.Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            return new OkObjectResult(new { token = newToken });
        }

        public async Task<IActionResult> HandleLogoutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || !httpContext.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                return new UnauthorizedObjectResult(new { message = "Refresh token is missing." });
            }

            // Validate the refresh token and get the associated user
            User? user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return new UnauthorizedObjectResult(new { message = "Invalid refresh token." });
            }
            // Invalidate the refresh token in the database
            httpContext.Response.Cookies.Delete("refreshToken");

            // Invalidate the JWT token in the database
            string? jwtToken = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            string? refreshTokenFromCookie = httpContext.Request.Cookies["refreshToken"];

            // Check if the JWT token or refresh token is missing
            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(refreshTokenFromCookie))
            {
                return new BadRequestObjectResult(new { message = "JWT token or refresh token is missing." });
            }

            // Invalidate the JWT token and refresh token in the database
            await _userService.InvalidateRefreshTokenAndJwtToken(jwtToken, refreshTokenFromCookie);

            return new OkObjectResult(new { message = "Logged out successfully." });
        }
    }
}
