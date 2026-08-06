using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Services.Interfaces;
using AssignmentSystem.Api.Services.Services;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.StartupTasks;
using FluentValidation;
using FluentValidation.AspNetCore;
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

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


        // Middleware Configuration
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();



        // Register Helpers
        services.AddSingleton<SeedHelper>();
        services.AddSingleton<IPasswordHelper, PasswordHelper>();




        // Register Startup Tasks
        services.AddHostedService<SeedDataToDatabase>();


        // Register Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IClassSubjectService, ClassSubjectService>();
        services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
        services.AddScoped<IStudentEnrollmentService, StudentEnrollmentService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
    }
}
