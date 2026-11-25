using System.Xml;
using be_rallyhubv2.application.abstractions;
using Microsoft.Extensions.Logging;

namespace be_rallyhubv2.application.services;

public sealed class PostCommitActionQueue(ILogger<PostCommitActionQueue> logger): IPostCommitActionQueue
{
    
    private readonly Queue<Func<CancellationToken, Task>> _actions = new();

    public void Enqueue(Func<CancellationToken, Task> action) => _actions.Enqueue(action);

    public async Task RunAllAsync(CancellationToken cancellationToken)
    {
        while (_actions.TryDequeue(out var action))
        {
            try
            {
                await action(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A post-commit action failed");
            }
        }

    }
}