using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.application.dependencyInjection;
using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.domain.abstractions.repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace be_rallyhubv2.tests.unit.behaviors;

public class PipelineOrderTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
        services.AddSingleton<ICacheService, FakeCacheService>();
        return services.BuildServiceProvider();
    }

    private static Type[] BehaviorsOf<TRequest, TResponse>(ServiceProvider provider)
        where TRequest : notnull
        => provider.GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Select(behavior => behavior.GetType().GetGenericTypeDefinition())
            .ToArray();

    [Fact]
    public void Command_goes_through_logging_validation_constraint_transaction_in_that_order()
    {
        using var provider = BuildProvider();

        var behaviors = BehaviorsOf<TestCommand, Result<string>>(provider);

        Assert.Equal(
            new[]
            {
                typeof(LoggingBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(ConstraintViolationBehavior<,>),
                typeof(TransactionBehavior<,>)
            },
            behaviors);
    }

    [Fact]
    public void Cacheable_query_goes_through_logging_validation_caching_only()
    {
        using var provider = BuildProvider();

        var behaviors = BehaviorsOf<TestQuery, Result<string>>(provider);

        Assert.Equal(
            new[] { typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>), typeof(CachingBehavior<,>) },
            behaviors);
    }
}