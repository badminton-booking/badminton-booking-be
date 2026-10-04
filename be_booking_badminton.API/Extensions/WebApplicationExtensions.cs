using be_booking_badminton.API.Middleware;

namespace be_booking_badminton.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerServices();
        }

        app.UseHttpsRedirection();
        app.UseCors(CorsExtensions.FrontendPolicy);
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
