using be_rallyhubv2.application.services;

namespace be_rallyhubv2.tests.unit.behaviors;

public class PostCommitActionQueueTests
{
    [Fact]
    public async Task A_failing_action_does_not_stop_the_others_or_throw()
    {
        var logger = new ListLogger<PostCommitActionQueue>();
        var queue = new PostCommitActionQueue(logger);
        var secondRan = false;

        queue.Enqueue(_ => throw new InvalidOperationException("boom"));
        queue.Enqueue(_ =>
        {
            secondRan = true;
            return Task.CompletedTask;
        });

        await queue.RunAllAsync(default);

        Assert.True(secondRan);
        Assert.Single(logger.Entries);
    }

    [Fact]
    public async Task Actions_run_only_once()
    {
        var queue = new PostCommitActionQueue(new ListLogger<PostCommitActionQueue>());
        var count = 0;
        queue.Enqueue(_ =>
        {
            count++;
            return Task.CompletedTask;
        });

        await queue.RunAllAsync(default);
        await queue.RunAllAsync(default);

        Assert.Equal(1, count);
    }
}