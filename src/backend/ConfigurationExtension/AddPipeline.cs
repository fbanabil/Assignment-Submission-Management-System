using AssignmentSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.ConfigurationExtension
{
    public class AddPipeline
    {
        public static void AddPipelineConfiguration(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }


            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options=>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment System API V1");
                    options.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            //app.UseCors("AllowAll");



            app.UseMiddleware<TokenBlacklistMiddleware>();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
        }
    }
}
