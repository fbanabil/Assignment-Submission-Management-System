namespace Backend.ConfigurationExtension
{
    public class AddPipeline
    {
        public static void AddPipelineConfiguration(WebApplication app)
        {
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


            app.MapControllers();
        }
    }
}
