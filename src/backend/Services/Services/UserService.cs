namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.UserDTOs;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHelper _passwordHelper;
    private readonly IAuthenticationHelper _authenticationHelper;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ITokenBlacklistRepository _tokenBlacklistRepository;

    public UserService(AppDbContext context, IPasswordHelper passwordHelper, ILogger<UserService> logger, IAuthenticationHelper authenticationHelper, IHttpContextAccessor contextAccessor, ITokenBlacklistRepository tokenBlacklistRepository)
    {
        _context = context;
        _passwordHelper = passwordHelper;
        _logger = logger;
        _authenticationHelper = authenticationHelper;
        _contextAccessor = contextAccessor;
        _tokenBlacklistRepository = tokenBlacklistRepository;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }



    /// <summary>
    /// This method retrieves all users from the database asynchronously. It uses the ToListAsync method of the DbSet to fetch all User entities and returns them as an IEnumerable<User>. This allows for efficient retrieval of user data without blocking the calling thread.
    /// </summary>
    /// <returns>An IEnumerable<User> containing all users in the database.</returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync() =>
        await _context.Users.ToListAsync();




    /// <summary>
    /// This method retrieves a user from the database based on the provided user ID. It uses the FindAsync method of the DbSet to locate the user. If the user is found, it returns the User object; otherwise, it returns null.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The User object if found; otherwise, null.</returns>
    public async Task<User?> GetUserByIdAsync(Guid id) =>
        await _context.Users.FindAsync(id);




    /// <summary>
    /// This method creates a new user in the database based on the provided UserCreateDto. It first checks if a user with the same email already exists. If so, it throws a BadRequestException. If not, it hashes the password, creates a new User entity, and saves it to the database. If any database update error occurs, it logs the error and throws a general exception.
    /// </summary>
    /// <param name="dto">The UserCreateDto containing the details of the user to create.</param>
    /// <returns>The created User object.</returns>
    /// <exception cref="BadRequestException">Throws a BadRequestException if a user with the same email already exists.</exception>
    /// <exception cref="Exception">Throws a general exception if an error occurs while creating the user.</exception>
    public async Task<User> CreateUserAsync(UserCreateDto dto)
    {
        // Check if a user with the same email already exists
        bool exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
        {
            throw new BadRequestException("A user with this email already exists.");
        }

        // Create a new User entity
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

        // Save the new user to the database
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



    /// <summary>
    /// This method updates an existing user's details in the database based on the provided UserUpdateDto. It first retrieves the user by ID. If the user is found, it updates the user's properties with the values from the DTO (if they are not null) and saves the changes to the database. If the user is not found, it simply returns without making any changes.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="dto">The UserUpdateDto containing the updated user details.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task UpdateUserAsync(Guid id, UserUpdateDto dto)
    {
        // Retrieve the user by ID
        var user = await _context.Users.FindAsync(id);
        if (user == null) return;
        
        // Update the user's properties with the values from the DTO (if they are not null)
        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Role != null) user.Role = dto.Role.Value;
        if (dto.IsActive != null) user.IsActive = dto.IsActive.Value;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

        // Save the changes to the database
        await _context.SaveChangesAsync();
    }




    /// <summary>
    /// This method deletes a user from the database based on the provided user ID. It first retrieves the user by ID. If the user is found, it removes the user from the DbSet and saves the changes to the database. If the user is not found, it simply returns without making any changes.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task DeleteUserAsync(Guid id)
    {
        // Retrieve the user by ID
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            // Delete the user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }




    /// <summary>
    /// This method authenticates a user by checking if the provided email and password match a user in the database. If the credentials are valid, it returns the corresponding User object; otherwise, it returns null.
    /// </summary>
    /// <param name="email">The email of the user to authenticate.</param>
    /// <param name="password">The password of the user to authenticate.</param>
    /// <returns>The authenticated User object if credentials are valid; otherwise, null.</returns>
    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        // Find the user by email
        User? user = _context.Users.FirstOrDefault(u => u.Email == email);
        // If the user is found, verify the password
        if (user != null)
        {
            // Verify the password using the password helper
            bool isPasswordValid = await _passwordHelper.VerifyPassword(password, user.PasswordHash);
            if(isPasswordValid)
            {
                return user;
            }
        }
        return null;
    }




    /// <summary>
    /// This method generates a JWT token for the given user. It creates a UserPayload object containing the user's ID, full name, email, and roles, and then uses the IAuthenticationHelper to create a JWT token based on this payload. The generated token is returned as a string.
    /// </summary>
    /// <param name="user">The user for whom to generate the JWT token.</param>
    /// <returns>The generated JWT token as a string.</returns>
    public async Task<string> GenerateJwtToken(User user)
    {
        // Create a UserPayload object with the user's details
        UserPayload payload = new UserPayload(UserId: user.Id.ToString(), FullName: user.FullName, Email: user.Email, Roles: new List<string> { user.Role.ToString() });

        // Generate a JWT token using the authentication helper
        string token = await _authenticationHelper.CreateJwtToken(payload);
        return token;
    }



    /// <summary>
    /// This method generates a new refresh token for the user. It creates a random refresh token, hashes it, and stores it in the database with an expiration date and usage status. If any database update error occurs, it logs the error and throws a general exception. The generated refresh token is returned as a string.
    /// </summary>
    /// <returns>The generated refresh token as a string.</returns>
    /// <exception cref="Exception">Throws a general exception if an error occurs while creating the refresh token.</exception>
    public async Task<string> GenerateRefreshToken(User user)
    {
        // Generate a new refresh token
        string refreshToken = await _authenticationHelper.CreateRefreshTokenAsync();
        string hashedRefreshToken = await _authenticationHelper.HashTokenAsync(refreshToken);

        // Store the hashed refresh token in the database with an expiration date and usage status
        try
        {
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = hashedRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Set expiration as needed
                IsUsed = false,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new refresh token.");
            throw new Exception("An error occurred while creating a new refresh token. Please try again later.");
        }

        return refreshToken;
    }




    /// <summary>
    /// This method retrieves a user based on the provided refresh token. It first hashes the refresh token and checks if it exists in the database and is valid (not used and not expired). If a valid token is found, it marks the token as used and retrieves the associated user. It also validates that the user ID from the current HTTP context matches the user associated with the refresh token. If any validation fails, it throws an UnauthorizedAccessException. Finally, it saves changes to the database and returns the user.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for retrieving the user.</param>
    /// <returns>The user associated with the provided refresh token, or null if the token is invalid or expired.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user ID from the current HTTP context does not match the user associated with the refresh token.</exception>
    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        // Hash the provided refresh token
        string hashedToken = await _authenticationHelper.HashTokenAsync(refreshToken);
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hashedToken && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow);

        // If the token is not found or is invalid, return null
        if (token == null)
        {
            return null;
        }

        // Mark the token as used
        token.IsUsed = true;
        
        // Assuming you have a way to link refresh tokens to users, e.g., a UserId property in RefreshToken
        User? user = await _context.Users.FindAsync(token.UserId);

        // Validate that the user ID from the current HTTP context matches the user associated with the refresh token
        var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID claim not found.");
        if (Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            if(user == null || user.Id != userId)
            {
                throw new UnauthorizedAccessException("Invalid refresh token for the current user.");
            }
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid user ID claim.");
        }


        // Mark the token as used and save changes
        await _context.SaveChangesAsync();

        return user;
    }





    /// <summary>
    /// This method invalidates both the provided JWT token and the refresh token. It first hashes the refresh token and checks if it exists in the database. If found, it marks the refresh token as used and saves the changes. Then, it adds the JWT token to a blacklist for a specified duration (1 day in this case). If any errors occur during this process, they are logged, and a general exception is thrown.
    /// </summary>
    /// <param name="jwtToken">The JWT token to invalidate and add to the blacklist.</param>
    /// <param name="refreshTokenFromCookie">The refresh token to invalidate.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task InvalidateRefreshTokenAndJwtToken(string jwtToken, string refreshTokenFromCookie)
    {
        try
        {
            // Invalidate the refresh token
            string hashedRefreshToken = await _authenticationHelper.HashTokenAsync(refreshTokenFromCookie);
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hashedRefreshToken);

            // If the refresh token is found, mark it as used
            if (refreshToken != null)
            {
                refreshToken.IsUsed = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Refresh token not found or already invalidated.");
                throw new Exception("Refresh token not found or already invalidated.");
            }

                // Add the JWT token to the blacklist
                await _tokenBlacklistRepository.AddToBlacklistAsync(jwtToken, TimeSpan.FromDays(1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while invalidating tokens.");
            throw new Exception("An error occurred while invalidating tokens. Please try again later.");
        }
    }



    /// <summary>
    /// This method retrieves a summary of user statistics, including total users, active users, inactive users, new users this month, and a breakdown of users by role. It uses LINQ queries to count the relevant user data and groups users by their roles to create a list of UserRoleSummaryDto objects. The resulting UserSummaryDto object is returned.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the UserSummaryDto object.</returns>
    public async Task<UserSummaryDto> GetUserSummaryAsync()
    {
        var userSummary = new UserSummaryDto
        {
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
            InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
            NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            RoleBreakdown = await _context.Users
                .GroupBy(u => u.Role)
                .Select(g => new UserRoleSummaryDto
                {
                    Role = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync()
        };
        return userSummary;
    }
}