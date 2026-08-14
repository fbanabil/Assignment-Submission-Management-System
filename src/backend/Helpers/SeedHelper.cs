using AssignmentSystem.Api.Data;
using Backend.StartupTasks;
using System.Text.Json;

namespace Backend.Helpers
{
    public class SeedHelper
    {
        private readonly ILogger<SeedHelper> _logger;
        private readonly IConfiguration _configuration;
        public SeedHelper(ILogger<SeedHelper> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task SeedDataToDatabase(IServiceProvider serviceProvider)
        {
            _logger.LogInformation("Seeding data to the database...");
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    dbContext.Database.EnsureCreated();


                    // Read filenames from "SeedData" directory of the project. Path in configuration
                    string directoryPath = _configuration.GetValue<string>("SeedDataDirectory")??string.Empty;

                    List<string> fileNames = new List<string>();

                    if (Directory.Exists(directoryPath))
                    {
                        fileNames = Directory.GetFiles(directoryPath, "*.json").ToList();
                    }
                    else
                    {
                        _logger.LogWarning($"Seed data directory '{directoryPath}' does not exist.");
                    }

                    if (fileNames.Count > 0)
                    {
                        var orderedFiles = fileNames.OrderBy(f => Path.GetFileName(f)).ToList();

                        foreach (var jsonDataPath in orderedFiles)
                        {
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(jsonDataPath);
                            string seedEntityName = System.Text.RegularExpressions.Regex.Replace(fileNameWithoutExt, @"^\d+", "");

                            Type? targetType = typeof(AssignmentSystem.Api.Models.Entities.User).Assembly
                                .GetType($"AssignmentSystem.Api.Models.Entities.{seedEntityName}");

                            if (targetType == null)
                            {
                                targetType = AppDomain.CurrentDomain.GetAssemblies()
                                    .Select(a => a.GetType($"AssignmentSystem.Api.Models.Entities.{seedEntityName}"))
                                    .FirstOrDefault(t => t != null);
                            }

                            if (targetType != null)
                            {
                                var jsonData = File.ReadAllText(jsonDataPath);
                                var dataList = JsonSerializer.Deserialize(jsonData, typeof(List<>).MakeGenericType(targetType));

                                if (dataList != null)
                                {
                                    foreach (var item in (IEnumerable<object>)dataList)
                                    {
                                        bool exists = dbContext.Find(targetType, item.GetType().GetProperty("Id")?.GetValue(item)) != null;

                                        if (!exists)
                                        {
                                            // Check if item is of type User
                                            if (item is AssignmentSystem.Api.Models.Entities.User userItem)
                                            {
                                                // get passwordhelper service
                                                using (var passwordHelperScope = serviceProvider.CreateScope())
                                                {
                                                    var passwordHelper = passwordHelperScope.ServiceProvider.GetRequiredService<IPasswordHelper>();
                                                    userItem.PasswordHash = Task.Run(() => passwordHelper.HashPassword(userItem.PasswordHash)).Result;
                                                }
                                            }

                                            dbContext.Add(item);
                                        }
                                    }

                                    dbContext.SaveChanges();
                                    _logger.LogInformation($"Seeded data for {seedEntityName} from {jsonDataPath}");
                                }
                                else
                                {
                                    _logger.LogWarning($"No data found in {jsonDataPath} for {seedEntityName}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Type not found for {seedEntityName} (path: {jsonDataPath})");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No seed data files found in the directory.");
                    }

                }
                _logger.LogInformation("Data seeding completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding data to the database.");
            }
            return Task.CompletedTask;
        }
    }
}
