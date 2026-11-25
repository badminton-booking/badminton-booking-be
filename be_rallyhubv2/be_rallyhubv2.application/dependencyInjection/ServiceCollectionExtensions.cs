using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.application.services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace be_rallyhubv2.application.dependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection service)
    {
        service.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

        service.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(AssemblyReference.Assembly);

            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(CachingBehavior<,>));
            config.AddOpenBehavior(typeof(ConstraintViolationBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });
        
        service.TryAddSingleton(TimeProvider.System);
        service.AddScoped<IPostCommitActionQueue, PostCommitActionQueue>();
        
        return service;
    }
}