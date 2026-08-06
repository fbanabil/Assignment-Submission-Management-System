namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Backend.Helpers;
using Backend.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHelper _passwordHelper;

    public UserService(AppDbContext context, IPasswordHelper passwordHelper, ILogger<UserService> logger)
    {
        _context = context;
        _passwordHelper = passwordHelper;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync() =>
        await _context.Users.ToListAsync();

    public async Task<User?> GetUserByIdAsync(Guid id) =>
        await _context.Users.FindAsync(id);




    // This method creates a new user in the database using the provided UserCreateDto.
    public async Task<User> CreateUserAsync(UserCreateDto dto)
    {
        bool exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);

        if (exists)
        {
            throw new BadRequestException("A user with this email already exists.");
        }


        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = await _passwordHelper.HashPassword(dto.Password),
            Role = dto.Role,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };


        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new user.");
            throw new Exception("An error occurred while creating a new user. Please try again later.");
        }

        return user;
    }




    public async Task UpdateUserAsync(Guid id, UserUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return;

        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Role != null) user.Role = dto.Role.Value;
        if (dto.IsActive != null) user.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}