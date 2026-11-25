using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.contract.abstractions.common;
using be_rallyhubv2.contract.abstractions.results;
using Microsoft.Extensions.Logging;

namespace be_rallyhubv2.tests.unit.behaviors;

public class LoggingBehaviorTests
{
    private readonly ListLogger<LoggingBehavior<TestCommand, Result<string>>> _logger = new();
    private readonly ManualTimeProvider _time = new();

    private LoggingBehavior<TestCommand, Result<string>> Create() => new(_logger, _time);

    [Fact]
    public async Task Returns_the_same_result_from_next()
    {
        var expected = Result<string>.Success("ok");

        var actual = await Create().Handle(new TestCommand("a"), () => Task.FromResult(expected), default);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task Failed_result_logs_a_warning_with_error_codes()
    {
        var failed = Result<string>.Failure(CommonErrors.Conflict());

        await Create().Handle(new TestCommand("a"), () => Task.FromResult(failed), default);

        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("Common.Conflict"));
    }

    [Fact]
    public async Task Slow_request_logs_a_slow_warning()
    {
        await Create().Handle(new TestCommand("a"), () =>
        {
            _time.Advance(TimeSpan.FromSeconds(4));
            return Task.FromResult(Result<string>.Success("ok"));
        }, default);

        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("Slow request"));
    }

    [Fact]
    public async Task Fast_request_has_no_slow_warning()
    {
        await Create().Handle(new TestCommand("a"), () =>
        {
            _time.Advance(TimeSpan.FromMilliseconds(100));
            return Task.FromResult(Result<string>.Success("ok"));
        }, default);

        Assert.DoesNotContain(_logger.Entries, e => e.Message.Contains("Slow request"));
    }

    [Fact]
    public async Task Sensitive_request_payload_is_not_logged()
    {
        var logger = new ListLogger<LoggingBehavior<SecretCommand, Result>>();
        var behavior = new LoggingBehavior<SecretCommand, Result>(logger, _time);

        await behavior.Handle(new SecretCommand("p@ssw0rd-123"), () => Task.FromResult(Result.Success()), default);

        Assert.DoesNotContain(logger.Entries, e => e.Message.Contains("p@ssw0rd-123"));
    }

    [Fact]
    public async Task Normal_request_payload_is_logged()
    {
        await Create().Handle(new TestCommand("visible-name"), () => Task.FromResult(Result<string>.Success("ok")), default);

        Assert.Contains(_logger.Entries, e => e.Message.Contains("visible-name"));
    }
}
