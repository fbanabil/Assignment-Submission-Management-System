namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.UserDTOs;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(UserCreateDto dto);
    Task UpdateUserAsync(Guid id, UserUpdateDto dto);
    Task DeleteUserAsync(Guid id);
}