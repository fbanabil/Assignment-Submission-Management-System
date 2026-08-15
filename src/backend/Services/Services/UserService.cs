namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
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
    }



    /// <summary>
    /// This method retrieves all users from the database asynchronously. It uses the ToListAsync method of the DbSet to fetch all User entities and returns them as an IEnumerable<User>. This allows for efficient retrieval of user data without blocking the calling thread.
    /// </summary>
    /// <returns>An IEnumerable<User> containing all users in the database.</returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("UserService: Fetching all users");
        return await _context.Users.ToListAsync();
    }




    /// <summary>
    /// This method retrieves a user from the database based on the provided user ID. It uses the FindAsync method of the DbSet to locate the user. If the user is found, it returns the User object; otherwise, it returns null.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The User object if found; otherwise, null.</returns>
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("UserService: Fetching user by Id:{UserId}", id);
        return await _context.Users.FindAsync(id);
    }




    /// <summary>
    /// This method creates a new user in the database based on the provided UserCreateDto. It first checks if a user with the same email already exists. If so, it throws a BadRequestException. If not, it hashes the password, creates a new User entity, and saves it to the database. If any database update error occurs, it logs the error and throws a general exception.
    /// </summary>
    /// <param name="dto">The UserCreateDto containing the details of the user to create.</param>
    /// <returns>The created User object.</returns>
    /// <exception cref="BadRequestException">Throws a BadRequestException if a user with the same email already exists.</exception>
    /// <exception cref="Exception">Throws a general exception if an error occurs while creating the user.</exception>
    public async Task<User> CreateUserAsync(UserCreateDto dto)
    {
        _logger.LogInformation("UserService: Creating user with Email:{Email}, Role:{Role}", dto.Email, dto.Role);
        // Check if a user with the same email already exists
        bool exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
        {
            _logger.LogWarning("UserService: User creation failed - email {Email} already exists", dto.Email);
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
            RollNo = dto.Role == UserRole.Student ? dto.RollNo : null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Save the new user to the database
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UserService: Successfully created user Id:{UserId}", user.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new user.");
            throw new InternalServerErrorException("An error occurred while creating a new user. Please try again later.");
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
        _logger.LogInformation("UserService: Updating user Id:{UserId}", id);
        // Retrieve the user by ID
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            _logger.LogWarning("UserService: User Id:{UserId} not found for update", id);
            return;
        }
        
        // Update the user's properties with the values from the DTO (if they are not null)
        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Role != null) user.Role = dto.Role.Value;
        if (dto.IsActive != null) user.IsActive = dto.IsActive.Value;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.RollNo != null) user.RollNo = dto.RollNo;
        if (user.Role != UserRole.Student) user.RollNo = null;

        // Save the changes to the database
        await _context.SaveChangesAsync();
        _logger.LogInformation("UserService: Updated user Id:{UserId}", id);
    }




    /// <summary>
    /// This method deletes a user from the database based on the provided user ID. It first retrieves the user by ID. If the user is found, it removes the user from the DbSet and saves the changes to the database. If the user is not found, it simply returns without making any changes.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task DeleteUserAsync(Guid id)
    {
        _logger.LogInformation("UserService: Deleting user Id:{UserId}", id);
        // Retrieve the user by ID
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            // Delete the user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UserService: Deleted user Id:{UserId}", id);
        }
        else
        {
            _logger.LogWarning("UserService: User Id:{UserId} not found for deletion", id);
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
        _logger.LogInformation("UserService: Authenticating user Email:{Email}", email);
        // Find the user by email
        User? user = _context.Users.FirstOrDefault(u => u.Email == email);
        // If the user is found, verify the password
        if (user != null)
        {
            // Verify the password using the password helper
            bool isPasswordValid = await _passwordHelper.VerifyPassword(password, user.PasswordHash);
            if(isPasswordValid)
            {
                _logger.LogInformation("UserService: User Email:{Email} authenticated successfully", email);
                return user;
            }
            _logger.LogWarning("UserService: Invalid password for Email:{Email}", email);
        }
        else
        {
            _logger.LogWarning("UserService: User not found for Email:{Email}", email);
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
        _logger.LogInformation("UserService: Generating JWT token for UserId:{UserId}", user.Id);
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
        _logger.LogInformation("UserService: Generating refresh token for UserId:{UserId}", user.Id);
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
                ExpiresAt = DateTime.UtcNow.AddDays(15), // Set expiration as needed
                IsUsed = false,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation("UserService: Created refresh token for UserId:{UserId}", user.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new refresh token.");
            throw new InternalServerErrorException("An error occurred while creating a new refresh token. Please try again later.");
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
        _logger.LogInformation("UserService: Retrieving user by refresh token");
        // Hash the provided refresh token
        string hashedToken = await _authenticationHelper.HashTokenAsync(refreshToken);
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hashedToken && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow);

        // If the token is not found or is invalid, return null
        if (token == null)
        {
            _logger.LogWarning("UserService: Refresh token is invalid or expired");
            return null;
        }

        // Mark the token as used
        token.IsUsed = true;


        // Assuming you have a way to link refresh tokens to users, e.g., a UserId property in RefreshToken
        User? user = await _context.Users.FindAsync(token.UserId);

        if (user == null)
        {
            throw new UnauthorizedException("User associated with the refresh token not found.");
        }


        // IsUsed = true for all refresh tokens associated with the same user
        await _context.RefreshTokens.Where(rt => rt.UserId == user!.Id && !rt.IsUsed).ForEachAsync(rt => rt.IsUsed = true);


        // Mark the token as used and save changes
        await _context.SaveChangesAsync();
        _logger.LogInformation("UserService: Refresh token validated and marked as used for UserId:{UserId}", user.Id);

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
                throw new BadRequestException("Refresh token not found or already invalidated.");
            }

                // Add the JWT token to the blacklist
                await _tokenBlacklistRepository.AddToBlacklistAsync(jwtToken, TimeSpan.FromDays(1));
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            _logger.LogError(ex, "An error occurred while invalidating tokens.");
            throw new InternalServerErrorException("An error occurred while invalidating tokens. Please try again later.");
        }
    }



    /// <summary>
    /// This method retrieves a summary of user statistics, including total users, active users, inactive users, new users this month, and a breakdown of users by role. It uses LINQ queries to count the relevant user data and groups users by their roles to create a list of UserRoleSummaryDto objects. The resulting UserSummaryDto object is returned.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the UserSummaryDto object.</returns>
    public async Task<UserSummaryDto> GetUserSummaryAsync()
    {
        // Retrieve user statistics and create a UserSummaryDto object
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





    /// <summary>
    /// This method retrieves a paginated list of users based on the provided filter criteria in the UserFilterDto. It applies various filters such as ID, name, email, phone number, role, and active status to the query. The results are then paginated based on the specified page number and page size. The method returns a PagedResultDto containing the filtered user data along with pagination information.
    /// </summary>
    /// <param name="filterDto">The filter criteria for retrieving users.</param>
    /// <returns>A PagedResultDto containing the filtered user data and pagination information.</returns>
    public async Task<PagedResultDto<UserResponseDto>> GetUsersAsync(UserFilterDto filterDto)
    {
        // Set default values for page number and page size if they are not provided or invalid
        filterDto.PageNumber = (filterDto.PageNumber <= 0) ? 1 : filterDto.PageNumber;
        filterDto.PageSize = (filterDto.PageSize <= 0) ? 10 : filterDto.PageSize;


        // Create a queryable collection of users from the database
        var query = _context.Users.AsQueryable();


        // Apply filters based on the provided filter criteria in the UserFilterDto
        if(!string.IsNullOrEmpty(filterDto.Name))
        {
            query = query.Where(u => EF.Functions.ILike(u.FullName, $"%{filterDto.Name}%"));
        }

        if(!string.IsNullOrEmpty(filterDto.Email))
        {
            query = query.Where(u => EF.Functions.ILike(u.Email, $"%{filterDto.Email}%"));
        }

        if(!string.IsNullOrEmpty(filterDto.PhoneNumber))
        {
            query = query.Where(u => EF.Functions.ILike(u.PhoneNumber, $"%{filterDto.PhoneNumber}%"));
        }

        if(!string.IsNullOrEmpty(filterDto.RollNo))
        {
            query = query.Where(u => u.RollNo != null && EF.Functions.ILike(u.RollNo, $"%{filterDto.RollNo}%"));
        }

        if(filterDto.Role.HasValue)
        {
            query = query.Where(u => u.Role == filterDto.Role.Value);
        }

        if(filterDto.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filterDto.IsActive.Value);
        }

        bool isDesc = filterDto.SortOrder == SortOrder.Desc;
        string sortBy = filterDto.SortBy?.ToLower().Trim() ?? "createdat";

        query = sortBy switch
        {
            "name" or "fullname" => isDesc ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
            "email" => isDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "rollno" => isDesc ? query.OrderByDescending(u => u.RollNo) : query.OrderBy(u => u.RollNo),
            "role" => isDesc ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "isactive" => isDesc ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
            _ => isDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        // Create a PagedResultDto to hold the paginated results
        PagedResultDto<UserResponseDto> result = new PagedResultDto<UserResponseDto>
        {
            TotalCount = await query.CountAsync(),
            PageNumber = filterDto.PageNumber,
            PageSize = filterDto.PageSize,
            Items = await query
                .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RollNo = u.RollNo,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync()
        };
        return result;
    }




    /// <summary>
    /// This method retrieves the name and email of a teacher based on the provided user and optional ID. It first checks if the user is authorized to access the teacher's information by comparing the user ID claim with the provided ID. If the user is not authorized, it throws an UnauthorizedAccessException. If the teacher is found in the database, it returns a tuple containing the teacher's name and email; otherwise, it throws a KeyNotFoundException.
    /// </summary>
    /// <param name="user">A ClaimsPrincipal representing the currently authenticated user.</param>
    /// <param name="id">An optional GUID representing the teacher's ID.</param>
    /// <returns>A tuple containing the teacher's name and email.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authorized to access the teacher's information.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the teacher is not found in the database.</exception>
    public async Task<(string TeacherName, string TeacherEmail, Guid TeacherId)> GetTeacherNameAndEmail(ClaimsPrincipal user, Guid? id)
    {
        var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedException("User ID claim not found.");
        var userRoles = _contextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList() ?? new List<string>();
        
        if(userIdClaim != null && id.HasValue && id.Value != Guid.Empty && id.ToString() != userIdClaim.Value)
        {
            throw new ForbiddenException("You are not authorized to access this teacher's information.");
        }

        Guid teacherId = Guid.Parse(userIdClaim!.Value);

        var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == teacherId && u.Role == UserRole.Teacher);
        if (teacher == null)
        {
            throw new NotFoundException("Teacher not found.");
        }

        return (teacher.FullName, teacher.Email, teacherId);
    }





    /// <summary>
    /// This method retrieves the user ID, email, and roles from the claims of the provided ClaimsPrincipal. It extracts the user ID and email claims, and collects all role claims into a list. If any of the required claims are missing, it throws an UnauthorizedAccessException. The method returns a tuple containing the user ID, email, and a list of roles.
    /// </summary>
    /// <param name="user">A ClaimsPrincipal representing the currently authenticated user.</param>
    /// <returns>A tuple containing the user ID, email, and a list of roles.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when any of the required claims are missing.</exception>
    public async Task<(Guid UserId, string Email, List<string> Roles)> GetUserIdAndEmailFromClaims()
    {
        var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedException("User ID claim not found.");
        var userRoles = _contextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList() ?? new List<string>();
        var userEmailClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email) ?? throw new UnauthorizedException("User email claim not found.");

        if (userIdClaim == null)
        {
            throw new UnauthorizedException("User ID claim not found.");
        }

        Guid userId = Guid.Parse(userIdClaim.Value);

        return (userId, userEmailClaim.Value, userRoles);
    }
}