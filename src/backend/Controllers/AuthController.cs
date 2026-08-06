using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
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


        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            return StatusCode(StatusCodes.Status201Created, new { user.Id, user.FullName, user.Email, user.Role });
            
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            User? user = await _userService.AuthenticateUserAsync(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            
            string token = await _userService.GenerateJwtToken(user);
            string refreshToken = await _userService.GenerateRefreshToken();

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
    }
}
