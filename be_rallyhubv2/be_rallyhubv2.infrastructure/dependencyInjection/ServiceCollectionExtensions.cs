using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.infrastructure.authentication;
using be_rallyhubv2.infrastructure.caching;
using be_rallyhubv2.infrastructure.files;
using be_rallyhubv2.infrastructure.mail;
using be_rallyhubv2.infrastructure.passwordHasher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace be_rallyhubv2.infrastructure.dependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<JwtOptions>(JwtOptions.SectionName);
        services.AddValidatedOptions<PasswordHashingOptions>(PasswordHashingOptions.SectionName);
        services.AddValidatedOptions<MailOptions>(MailOptions.SectionName);
        services.AddValidatedOptions<CloudinaryOptions>(CloudinaryOptions.SectionName);

        var redisConnectionString = configuration.GetConnectionString("Redis")
                                    ?? throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings.Redis");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "rallyhub:";
        });
    
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IFileStorage, CloudinaryFileStorage>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        
        return services;

    }

    private static void AddValidatedOptions<TOptions>(this IServiceCollection services, string sectionName)
        where TOptions : class
        => services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)//
            .ValidateDataAnnotations()
            .ValidateOnStart();
    
}