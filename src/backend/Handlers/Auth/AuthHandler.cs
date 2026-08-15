using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Auth
{
    public class AuthHandler
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthHandler> _logger;

        public AuthHandler(IUserService userService, IHttpContextAccessor httpContextAccessor, ILogger<AuthHandler> logger)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> HandleLoginAsync(UserLoginDto dto)
        {
            _logger.LogInformation("AuthHandler: Attempting login for Email:{Email}", dto?.Email);
            // Validate the user credentials
            User? user = await _userService.AuthenticateUserAsync(dto!.Email, dto.Password);
            if (user == null)
            {
                _logger.LogWarning("AuthHandler: Invalid login credentials for Email:{Email}", dto?.Email);
                throw new BadRequestException("Invalid email or password.");
            }

            // Generate JWT token and refresh token
            string token = await _userService.GenerateJwtToken(user);
            string refreshToken = await _userService.GenerateRefreshToken(user);

            var httpContext = _httpContextAccessor.HttpContext;
            // Set refreshToken as HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.None,
                Path = "/",
                Secure = httpContext?.Request.IsHttps ?? false
            };

            httpContext?.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            _logger.LogInformation("AuthHandler: User {UserId} logged in successfully", user.Id);
            return new OkObjectResult(new { token });
        }

        public async Task<IActionResult> HandleRefreshTokenAsync()
        {
            _logger.LogInformation("AuthHandler: Attempting token refresh");
            var httpContext = _httpContextAccessor.HttpContext;
            var refreshToken = httpContext?.Request.Cookies["refreshToken"];
            if (httpContext == null || refreshToken == null)
            {
                _logger.LogWarning("AuthHandler: Refresh token missing in cookies");
                return new UnauthorizedObjectResult(new { message = "Refresh token is missing." });
            }

            // Validate the refresh token and get the associated user
            User? user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                _logger.LogWarning("AuthHandler: Invalid refresh token");
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
                SameSite = SameSiteMode.None,
                Path = "/",
                Secure = httpContext.Request.IsHttps
            };
            httpContext.Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            _logger.LogInformation("AuthHandler: Token refreshed successfully for User {UserId}", user.Id);
            return new OkObjectResult(new { token = newToken });
        }

        public async Task<IActionResult> HandleLogoutAsync()
        {
            _logger.LogInformation("AuthHandler: Attempting logout");
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || !httpContext.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                _logger.LogWarning("AuthHandler: Refresh token missing during logout");
                return new UnauthorizedObjectResult(new { message = "Refresh token is missing." });
            }

            // Validate the refresh token and get the associated user
            User? user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                _logger.LogWarning("AuthHandler: Invalid refresh token during logout");
                return new UnauthorizedObjectResult(new { message = "Invalid refresh token." });
            }
            // Invalidate the refresh token in the database
            httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                Path = "/",
                SameSite = SameSiteMode.None,
                Secure = httpContext.Request.IsHttps
            });

            // Invalidate the JWT token in the database
            string? jwtToken = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            string? refreshTokenFromCookie = httpContext.Request.Cookies["refreshToken"];

            // Check if the JWT token or refresh token is missing
            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(refreshTokenFromCookie))
            {
                _logger.LogWarning("AuthHandler: JWT or refresh token missing for logout invalidation");
                return new BadRequestObjectResult(new { message = "JWT token or refresh token is missing." });
            }

            // Invalidate the JWT token and refresh token in the database
            await _userService.InvalidateRefreshTokenAndJwtToken(jwtToken, refreshTokenFromCookie);

            _logger.LogInformation("AuthHandler: User {UserId} logged out successfully", user.Id);
            return new OkObjectResult(new { message = "Logged out successfully." });
        }
    }
}
