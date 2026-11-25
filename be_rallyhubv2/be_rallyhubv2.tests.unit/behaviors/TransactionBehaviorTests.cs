using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.application.services;
using be_rallyhubv2.contract.abstractions.common;
using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.tests.unit.behaviors;

public class TransactionBehaviorTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly PostCommitActionQueue _queue = new(new ListLogger<PostCommitActionQueue>());

    private TransactionBehavior<TestCommand, Result<string>> Create() => new(_unitOfWork, _queue);

    [Fact]
    public async Task Success_saves_commits_and_runs_post_commit_actions()
    {
        var ran = false;

        await Create().Handle(new TestCommand("a"), () =>
        {
            _queue.Enqueue(_ =>
            {
                ran = true;
                return Task.CompletedTask;
            });
            return Task.FromResult(Result<string>.Success("ok"));
        }, default);

        Assert.Equal(new[] { "begin", "save", "commit" }, _unitOfWork.Calls);
        Assert.True(ran);
    }

    [Fact]
    public async Task Failed_result_rolls_back_without_saving_and_skips_post_commit_actions()
    {
        var ran = false;

        var result = await Create().Handle(new TestCommand("a"), () =>
        {
            _queue.Enqueue(_ =>
            {
                ran = true;
                return Task.CompletedTask;
            });
            return Task.FromResult(Result<string>.Failure(CommonErrors.Conflict()));
        }, default);

        Assert.True(result.IsFailure);
        Assert.Equal(new[] { "begin", "rollback" }, _unitOfWork.Calls);
        Assert.False(ran);
    }

    [Fact]
    public async Task Exception_in_handler_rolls_back_and_rethrows()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Create().Handle(new TestCommand("a"), () => throw new InvalidOperationException(), default));

        Assert.Equal(new[] { "begin", "rollback" }, _unitOfWork.Calls);
    }

    [Fact]
    public async Task Exception_while_saving_rolls_back_and_rethrows()
    {
        _unitOfWork.ThrowOnSave = new InvalidOperationException();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Create().Handle(new TestCommand("a"), () => Task.FromResult(Result<string>.Success("ok")), default));

        Assert.Equal(new[] { "begin", "save", "rollback" }, _unitOfWork.Calls);
    }
}