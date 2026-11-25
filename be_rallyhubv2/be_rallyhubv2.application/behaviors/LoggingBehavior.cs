using be_rallyhubv2.contract.abstractions.messages;
using be_rallyhubv2.contract.abstractions.results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace be_rallyhubv2.application.behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    TimeProvider timeProvider) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    
    private const int SlowThresholdMilliseconds = 3000;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;

        if (request is ISensitiveRequest)
        {
            logger.LogInformation("Handling {Request}", name);
        }
        else
        {
            logger.LogInformation("Handling {Request} {@payload}", name, request); 
        }

        var startedAt = timeProvider.GetTimestamp();
        var response = await next();
        var elapsed = (long)timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;

        if (response.IsFailure)
        {
            var codes = string.Join(", ", response.Errors.Select(error => error.Code));
            logger.LogWarning("Handled {Request} in {Elapsed} milliseconds with errors: {Errors}", name, elapsed, codes);
        }
        else
        {
            logger.LogWarning("Handled {Request} in {Elapsed} milliseconds", name, elapsed);
        }

        if (elapsed > SlowThresholdMilliseconds)
        {
            logger.LogWarning("Slow request {Request} took {Elapsed}ms", name, elapsed);
        }
        
        return response;
        
    }
}