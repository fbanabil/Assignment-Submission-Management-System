
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using Backend.Helpers;

namespace Backend.StartupTasks
{
    public class SeedDataToDatabase : IHostedService
    {
        private readonly ILogger<SeedDataToDatabase> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SeedHelper _seedHelper;
        private readonly IPasswordHelper _passwordHelper;


        public SeedDataToDatabase(ILogger<SeedDataToDatabase> logger, IServiceProvider serviceProvider, SeedHelper seedHelper, IPasswordHelper passwordHelper)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _seedHelper = seedHelper;
            _passwordHelper = passwordHelper;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {

            try
            {
                await _seedHelper.SeedDataToDatabase(_serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in seed data helper");
            }

            User? adminUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Admin User",
                Email = "admin@example.com",
                PhoneNumber = "1234567890",
                PasswordHash = await _passwordHelper.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (!dbContext.Users.Any(u => u.Email == adminUser.Email))
                {
                    dbContext.Users.Add(adminUser);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Admin user created successfully.");
                }
                else
                {
                    _logger.LogInformation("Admin user already exists.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SeedDataToDatabase service is stopping.");
            return Task.CompletedTask;
        }
    }
}
