using System.Security.Cryptography;
using System.Text.Json;
using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.contract.abstractions.messages;
using be_rallyhubv2.domain.abstractions.repositories;
using Microsoft.Extensions.Logging;

namespace be_rallyhubv2.tests.unit.behaviors;

public sealed record TestCommand(string Name): ICommand<string>;

public sealed record TestVoidCommand : ICommand;

public sealed record SecretCommand(string Password) : ICommand, ISensitiveRequest;

public sealed record TestQuery(string Key): IQuery<string>, ICacheable
{
    public string CacheKey => $"test:{Key}";
    public int CacheMinutes => 5;
}

public sealed class FakeUnitOfWork: IUnitOfWork
{
    public List<string> Calls { get; } = new();
    public Exception? ThrowOnSave { set; get; }
    
    public Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        Calls.Add("begin");
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        Calls.Add("save");
        if(ThrowOnSave is not null) throw ThrowOnSave;
        return Task.FromResult(1);
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        Calls.Add("commit");
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        Calls.Add("rollback");
        return Task.CompletedTask;
    }
}

public sealed class FakeCacheService : ICacheService
{
    public Dictionary<string, string> Cache { get; } = new();
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    => Task.FromResult(Cache.TryGetValue(key, out var json) ? JsonSerializer.Deserialize<T>(json) : default);
    
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        Cache[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        Cache.Remove(key);
        return Task.CompletedTask;
    }
}

public sealed class ListLogger<T> : ILogger<T>
{

    public List<(LogLevel Level, string Message)> Entries { get; } = new();
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
        Func<TState, Exception?, string> formatter)
    => Entries.Add((logLevel, formatter(state, exception)));

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;
}

public sealed class ManualTimeProvider : TimeProvider
{
    private long _tickets;
    
    public void Advance(TimeSpan amount) => _tickets += amount.Ticks;
    public override long GetTimestamp() => _tickets;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
}
