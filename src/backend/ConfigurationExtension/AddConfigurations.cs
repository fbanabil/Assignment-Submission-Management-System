using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Services.Interfaces;
using AssignmentSystem.Api.Services.Services;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.StartupTasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Backend.ConfigurationExtension;

public static class AddConfigurations
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddOpenApi();

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        services.AddHttpContextAccessor();

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


        // Add Cache for Token Blacklist
        services.AddDistributedMemoryCache();



        // Middleware Configuration
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();



        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });


        // Register Helpers
        services.AddSingleton<SeedHelper>();
        services.AddSingleton<IPasswordHelper, PasswordHelper>();
        services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
        services.AddScoped<ITokenBlacklistRepository, CacheTokenBlacklistRepository>();


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




        // Add Authentication and Authorization
        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        var publicKey = jwtSettingsSection.GetValue<string>("PublicKey");

        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);

        var rsaSecurityKey = new RsaSecurityKey(rsa);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettingsSection.GetValue<string>("Issuer"),
                    ValidAudience = jwtSettingsSection.GetValue<string>("Audience"),
                    IssuerSigningKey = rsaSecurityKey
                };
            });


        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
            options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
        });

    }
}
