using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.domain.exceptions;

namespace be_rallyhubv2.tests.unit.behaviors;

public class ConstraintViolationBehaviorTests
{
    [Fact]
    public async Task Mapped_constraint_becomes_a_failed_result_with_value()
    {
        var behavior = new ConstraintViolationBehavior<TestCommand, Result<string>>();

        var result = await behavior.Handle(
            new TestCommand("a"),
            () => throw new ConstraintViolationException(ConstraintViolationKind.Unique, "ux_users_email"),
            default);

        Assert.True(result.IsFailure);
        Assert.Equal("Identity.EmailTaken", result.Errors[0].Code);
        Assert.Equal(ErrorType.Conflict, result.Errors[0].Type);
    }

    [Fact]
    public async Task Mapped_constraint_also_works_for_command_without_value()
    {
        var behavior = new ConstraintViolationBehavior<TestVoidCommand, Result>();

        var result = await behavior.Handle(
            new TestVoidCommand(),
            () => throw new ConstraintViolationException(ConstraintViolationKind.Exclusion, "ex_booking_details_court_time_overlap"),
            default);

        Assert.Equal("Booking.SlotTaken", result.Errors[0].Code);
    }

    [Fact]
    public async Task Concurrency_conflict_becomes_a_failed_result()
    {
        var behavior = new ConstraintViolationBehavior<TestCommand, Result<string>>();

        var result = await behavior.Handle(
            new TestCommand("a"), () => throw new ConcurrencyConflictException(), default);

        Assert.Equal("Common.ConcurrencyConflict", result.Errors[0].Code);
    }

    [Fact]
    public async Task Unmapped_constraint_is_rethrown()
    {
        var behavior = new ConstraintViolationBehavior<TestCommand, Result<string>>();

        await Assert.ThrowsAsync<ConstraintViolationException>(() => behavior.Handle(
            new TestCommand("a"),
            () => throw new ConstraintViolationException(ConstraintViolationKind.Unique, "ux_unknown"),
            default));
    }

    [Fact]
    public async Task No_exception_passes_through()
    {
        var behavior = new ConstraintViolationBehavior<TestCommand, Result<string>>();

        var result = await behavior.Handle(new TestCommand("a"), () => Task.FromResult(Result<string>.Success("ok")), default);

        Assert.True(result.IsSuccess);
    }
}