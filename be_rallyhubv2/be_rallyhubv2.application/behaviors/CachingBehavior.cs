using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.contract.abstractions.messages;
using be_rallyhubv2.contract.abstractions.results;
using MediatR;

namespace be_rallyhubv2.application.behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(ICacheService cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheable
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }
        
        var response = await next();

        if (response.IsSuccess)
        {
            await cache.SetAsync(request.CacheKey, response, TimeSpan.FromMinutes(request.CacheMinutes) ,cancellationToken);
        }
        
        return response;
    }
}