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
using System.Threading.Tasks;

namespace Backend.ConfigurationExtension;

public static class AddConfigurations
{
    public static async Task AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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



        // Add CORS configured for credentials (AllowCredentials requires explicit origin echo instead of wildcard *)
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.SetIsOriginAllowed(origin => true)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
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

        // Register Handlers
        services.AddScoped<Backend.Handlers.Admin.DashboardHandler>();
        services.AddScoped<Backend.Handlers.Admin.UserHandler>();
        services.AddScoped<Backend.Handlers.Admin.ClassHandler>();
        services.AddScoped<Backend.Handlers.Admin.SubjectHandler>();
        services.AddScoped<Backend.Handlers.Admin.ClassSubjectHandler>();
        services.AddScoped<Backend.Handlers.Admin.TeacherAssignmentHandler>();
        services.AddScoped<Backend.Handlers.Admin.AssignmentHandler>();
        services.AddScoped<Backend.Handlers.Admin.SubmissionHandler>();
        services.AddScoped<Backend.Handlers.Auth.AuthHandler>();
        services.AddScoped<Backend.Handlers.Auth.UserAuthHandler>();
        services.AddScoped<Backend.Handlers.Student.StudentDashboardHandler>();
        services.AddScoped<Backend.Handlers.Student.StudentAssignmentHandler>();
        services.AddScoped<Backend.Handlers.Teacher.TeacherDashboardHandler>();
        services.AddScoped<Backend.Handlers.Teacher.TeacherClassHandler>();
        services.AddScoped<Backend.Handlers.Teacher.TeacherAssignmentHandler>();
        services.AddScoped<Backend.Handlers.Teacher.TeacherSubmissionHandler>();
        services.AddScoped<Backend.Handlers.Teacher.TeacherEnrollmentHandler>();







        // Add Authentication and Authorization
        var jwtSettingsSection = configuration.GetSection("JwtSettings");

        // Load the public key from a file
        var publicKey = await File.ReadAllTextAsync(jwtSettingsSection.GetValue<string>("PublicKeyPath")!);

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
                    ClockSkew = TimeSpan.Zero, // Optional: Set clock skew to zero for immediate expiration

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
