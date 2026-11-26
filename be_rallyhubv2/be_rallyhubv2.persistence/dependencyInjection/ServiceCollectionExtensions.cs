using System.Reflection;
using be_rallyhubv2.domain.abstractions.repositories;
using be_rallyhubv2.persistence.interceptors;
using be_rallyhubv2.persistence.repositories;
using be_rallyhubv2.persistence.seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be_rallyhubv2.persistence.dependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });
        
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddRepositoriesFromAssembly(AssemblyReference.Assembly);

        services.AddScoped<ReferenceDataSeeder>();
        services.AddHostedService<DatabaseInitializationService>();

        return services;
        
    }

    public static IServiceCollection AddRepositoriesFromAssembly(this IServiceCollection services,
        Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                           && type.Name.EndsWith("Repository"));

        foreach (var implementation in implementations)
        {
            var contract = implementation.GetInterfaces()
                .FirstOrDefault(@interface => @interface.Name == "I" + implementation.Name);
            
            if(contract is not null)
                services.AddScoped(contract, implementation);
        }
        
        return services;
        
    }
    
}