using be_rallyhubv2.application.abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace be_rallyhubv2.infrastructure.caching;

public sealed class RedisCacheService(IDistributedCache cache): ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var json = await cache.GetStringAsync(key, cancellationToken);
        
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
        
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
        => cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions{AbsoluteExpirationRelativeToNow = expiration},
            cancellationToken);
    

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
        => cache.RemoveAsync(key, cancellationToken);
}