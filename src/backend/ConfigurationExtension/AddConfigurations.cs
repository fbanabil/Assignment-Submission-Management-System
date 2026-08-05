using AssignmentSystem.Api.Data;
using Backend.Helpers;
using Backend.StartupTasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.ConfigurationExtension;

public static class AddConfigurations
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddOpenApi();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


        services.AddSingleton<SeedHelper>();
        services.AddHostedService<SeedDataToDatabase>();
    }
}
