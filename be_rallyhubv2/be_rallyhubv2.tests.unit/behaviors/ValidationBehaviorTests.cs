using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.contract.abstractions.results;
using FluentValidation;

namespace be_rallyhubv2.tests.unit.behaviors;

public class ValidationBehaviorTests
{
    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("Test.NameRequired");
        }
    }

    [Fact]
    public async Task Without_validators_calls_next()
    {
        var behavior = new ValidationBehavior<TestCommand, Result<string>>(Array.Empty<IValidator<TestCommand>>());

        var result = await behavior.Handle(new TestCommand(""), () => Task.FromResult(Result<string>.Success("ok")), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Invalid_request_throws_and_does_not_call_next()
    {
        var called = false;
        var behavior = new ValidationBehavior<TestCommand, Result<string>>(new[] { new TestCommandValidator() });

        var ex = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new TestCommand(""),
            () =>
            {
                called = true;
                return Task.FromResult(Result<string>.Success("ok"));
            },
            default));

        Assert.False(called);
        Assert.Equal("Test.NameRequired", ex.Errors.Single().ErrorCode);
    }

    [Fact]
    public async Task Valid_request_calls_next()
    {
        var behavior = new ValidationBehavior<TestCommand, Result<string>>(new[] { new TestCommandValidator() });

        var result = await behavior.Handle(new TestCommand("abc"), () => Task.FromResult(Result<string>.Success("ok")), default);

        Assert.True(result.IsSuccess);
    }
}