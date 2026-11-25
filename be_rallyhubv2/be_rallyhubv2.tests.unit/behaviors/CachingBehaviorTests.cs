using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.contract.abstractions.common;
using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.tests.unit.behaviors;

public class CachingBehaviorTests
{
    private readonly FakeCacheService _cache = new();

    private CachingBehavior<TestQuery, Result<string>> Create() => new(_cache);

    [Fact]
    public async Task Miss_calls_next_and_stores_the_result()
    {
        var result = await Create().Handle(new TestQuery("a"), () => Task.FromResult(Result<string>.Success("value")), default);

        Assert.Equal("value", result.Value);
        Assert.True(_cache.Cache.ContainsKey("test:a"));
    }

    [Fact]
    public async Task Hit_returns_cached_value_without_calling_next()
    {
        var calls = 0;
        Task<Result<string>> Next()
        {
            calls++;
            return Task.FromResult(Result<string>.Success("value"));
        }

        await Create().Handle(new TestQuery("a"), Next, default);
        var second = await Create().Handle(new TestQuery("a"), Next, default);

        Assert.Equal(1, calls);
        Assert.True(second.IsSuccess);
        Assert.Equal("value", second.Value);
    }

    [Fact]
    public async Task Failed_result_is_not_cached()
    {
        await Create().Handle(new TestQuery("a"), () => Task.FromResult(Result<string>.Failure(CommonErrors.Conflict())), default);

        Assert.Empty(_cache.Cache);
    }
}