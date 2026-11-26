using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.contract.abstractions.common;
using be_rallyhubv2.contract.abstractions.messages;
using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.infrastructure.caching;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Redis;

namespace be_rallyhubv2.tests.integration.infrastructure;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder().WithImage("redis:7").Build();

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(options => options.Configuration = _container.GetConnectionString());
        services.AddSingleton<ICacheService, RedisCacheService>();
        Services = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        if (Services is not null) await Services.DisposeAsync();
        await _container.DisposeAsync();
    }
}

public sealed record SampleQuery(string Id) : IQuery<string>, ICacheable
{
    public string CacheKey => $"sample:{Id}";
    public int CacheMinutes => 5;
}

public class RedisCacheServiceTests(RedisFixture fixture) : IClassFixture<RedisFixture>
{
    private ICacheService Cache => fixture.Services.GetRequiredService<ICacheService>();

    private static string NewKey() => "test:" + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task Missing_key_returns_null()
        => Assert.Null(await Cache.GetAsync<string>(NewKey(), default));

    [Fact]
    public async Task Stored_value_can_be_read_back()
    {
        var key = NewKey();

        await Cache.SetAsync(key, new List<int> { 1, 2, 3 }, TimeSpan.FromMinutes(1), default);

        Assert.Equal(new[] { 1, 2, 3 }, await Cache.GetAsync<List<int>>(key, default));
    }

    [Fact]
    public async Task Removed_key_is_gone()
    {
        var key = NewKey();
        await Cache.SetAsync(key, "value", TimeSpan.FromMinutes(1), default);

        await Cache.RemoveAsync(key, default);

        Assert.Null(await Cache.GetAsync<string>(key, default));
    }

    [Fact]
    public async Task Value_expires()
    {
        var key = NewKey();
        await Cache.SetAsync(key, "value", TimeSpan.FromSeconds(1), default);

        await Task.Delay(TimeSpan.FromSeconds(2));

        Assert.Null(await Cache.GetAsync<string>(key, default));
    }

    [Fact]
    public async Task Caching_behavior_stores_a_successful_result_in_redis_and_serves_it_next_time()
    {
        var calls = 0;
        Task<Result<string>> Next()
        {
            calls++;
            return Task.FromResult(Result<string>.Success("value"));
        }

        var behavior = new CachingBehavior<SampleQuery, Result<string>>(Cache);
        var query = new SampleQuery(Guid.NewGuid().ToString("N"));

        await behavior.Handle(query, Next, default);
        var second = await behavior.Handle(query, Next, default);

        Assert.Equal(1, calls);
        Assert.True(second.IsSuccess);
        Assert.Equal("value", second.Value);
    }

    [Fact]
    public async Task Caching_behavior_does_not_store_failed_results()
    {
        var behavior = new CachingBehavior<SampleQuery, Result<string>>(Cache);
        var query = new SampleQuery(Guid.NewGuid().ToString("N"));

        await behavior.Handle(query, () => Task.FromResult(Result<string>.Failure(CommonErrors.Conflict())), default);

        Assert.Null(await Cache.GetAsync<Result<string>>(query.CacheKey, default));
    }
}