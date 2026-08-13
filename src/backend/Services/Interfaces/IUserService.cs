namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs;
using Backend.DTOs.UserDTOs;
using System.Security.Claims;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(UserCreateDto dto);
    Task UpdateUserAsync(Guid id, UserUpdateDto dto);
    Task DeleteUserAsync(Guid id);
    Task<User?> AuthenticateUserAsync(string email, string password);
    Task<string> GenerateJwtToken(User user);
    Task<string> GenerateRefreshToken(User user);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task InvalidateRefreshTokenAndJwtToken(string jwtToken, string refreshTokenFromCookie);
    Task<UserSummaryDto> GetUserSummaryAsync();
    Task<PagedResultDto<UserResponseDto>> GetUsersAsync(UserFilterDto filterDto);
    Task<(string TeacherName, string TeacherEmail, Guid TeacherId)> GetTeacherNameAndEmail(ClaimsPrincipal user, Guid? id);
    Task<(Guid UserId, string Email, List<string> Roles)> GetUserIdAndEmailFromClaims();
}