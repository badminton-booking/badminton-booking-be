using be_booking_badminton.Application.Abstractions.Authentication;
using be_booking_badminton.Application.Abstractions.Communication;
using be_booking_badminton.Application.Abstractions.Media;
using be_booking_badminton.Application.Abstractions.Persistence;
using be_booking_badminton.Infrastructure.Authentication;
using be_booking_badminton.Infrastructure.Communication;
using be_booking_badminton.Infrastructure.Media;
using be_booking_badminton.Infrastructure.Persistence;
using be_booking_badminton.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be_booking_badminton.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Features:Database"))
        {
            AddPersistence(services, configuration);
        }

        if (configuration.GetValue<bool>("Features:Jwt"))
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<ITokenService, JwtTokenService>();
        }

        if (configuration.GetValue<bool>("Features:Mail"))
        {
            services.AddOptions<MailOptions>()
                .Bind(configuration.GetSection(MailOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        if (configuration.GetValue<bool>("Features:Cloudinary"))
        {
            services.AddOptions<CloudinaryOptions>()
                .Bind(configuration.GetSection(CloudinaryOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<IFileStorage, CloudinaryFileStorage>();
        }

        return services;
    }

    private static void AddPersistence(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required when Features:Database is enabled.");
        }

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<AppDbContext>());
    }
}
