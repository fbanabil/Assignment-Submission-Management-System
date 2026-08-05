
using AssignmentSystem.Api.Data;
using Backend.Helpers;

namespace Backend.StartupTasks
{
    public class SeedDataToDatabase : IHostedService
    {
        private readonly ILogger<SeedDataToDatabase> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SeedHelper _seedHelper;


        public SeedDataToDatabase(ILogger<SeedDataToDatabase> logger, IServiceProvider serviceProvider, SeedHelper seedHelper)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _seedHelper = seedHelper;
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
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SeedDataToDatabase service is stopping.");
            return Task.CompletedTask;
        }
    }
}
