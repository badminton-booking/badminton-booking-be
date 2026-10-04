using be_booking_badminton.API.Filters;
using be_booking_badminton.API.Services;
using be_booking_badminton.Application.Abstractions.Authentication;

namespace be_booking_badminton.API.Extensions;

public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options =>
            options.Filters.Add<ValidationActionFilter>())
            .ConfigureApiBehaviorOptions(options =>
                options.SuppressModelStateInvalidFilter = true);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHealthChecks();
        services.AddAuthenticationServices(configuration);
        services.AddCorsPolicy(configuration);
        services.AddApiRateLimiting(configuration);
        services.AddSwaggerServices();

        return services;
    }
}
