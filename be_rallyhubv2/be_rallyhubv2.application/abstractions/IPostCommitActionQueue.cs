namespace be_rallyhubv2.application.abstractions;

public interface IPostCommitActionQueue
{
    void Enqueue(Func<CancellationToken, Task> action);
    Task RunAllAsync(CancellationToken cancellationToken);
}