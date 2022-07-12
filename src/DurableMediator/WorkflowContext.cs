using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public record WorkflowContext<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    ISubWorkflowOrchestrator SubOrchestrator,
    EntityId EntityId,
    IDurableMediator DurableMediator)
{
    /// <summary>
    /// Tries the given action for maxRetries times. When action returns true the action is assumed successful and the Task completes.
    /// 
    /// If the action is not successful after maxRetries, OrchestrationRetryException is thrown;
    /// </summary>
    /// <param name="action"></param>
    /// <param name="token"></param>
    /// <param name="maxRetries"></param>
    /// <param name="millisecondsBetweenAttempt"></param>
    /// <returns></returns>
    /// <exception cref="OrchestrationRetryException" />
    public async Task TryAsync(
        Func<Task<bool>> action,
        CancellationToken token,
        int maxRetries = 3,
        int millisecondsBetweenAttempt = 1000)
    {
        var attempt = 0;
        do
        {
            attempt++;

            if (await action.Invoke())
            {
                return;
            }

            await OrchestrationContext.CreateTimer(
                DateTime.UtcNow.AddMilliseconds(millisecondsBetweenAttempt * attempt), 
                token);
        }
        while (attempt < maxRetries);

        throw new OrchestrationRetryException();
    }
}
