using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

public static class DurableOrchestrationContextExtensions
{
    /// <summary>
    /// Retries the given action for maxRetries times. When action returns true the action is assumed successful. When all attempts fail, OrchestrationRetryException is thrown.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="action"></param>
    /// <param name="maxRetries"></param>
    /// <param name="millisecondsBetweenAttempt"></param>
    /// <returns></returns>
    /// <exception cref="OrchestrationRetryException"></exception>
    public static async Task TryStepUntilSuccessfulAsync(
        this IDurableOrchestrationContext context,
        Func<Task<bool>> action,
        int maxRetries = 3,
        int millisecondsBetweenAttempt = 1000)
    {
        var attempt = 0;
        do
        {
            attempt++;

            var success = await action.Invoke();
            if (success)
            {
                return;
            }

            await context.CreateTimer(DateTime.UtcNow.AddMilliseconds(millisecondsBetweenAttempt * attempt), CancellationToken.None);
        }
        while (attempt < maxRetries);

        throw new OrchestrationRetryException();
    }

    /// <summary>
    /// Starts a sub orchestration of the given workflow and waits for the result.
    /// </summary>
    /// <typeparam name="TWorkflowRequest"></typeparam>
    /// <typeparam name="TWorkflow"></typeparam>
    /// <typeparam name="TWorkflowResult"></typeparam>
    /// <param name="context"></param>
    /// <param name="replaySafeLogger"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Task<TWorkflowResult?> RunSubWorkflowAsync<TWorkflowRequest, TWorkflow, TWorkflowResult>(
        this IDurableOrchestrationContext context,
        ILogger replaySafeLogger,
        TWorkflowRequest request)
        where TWorkflowRequest : IWorkflowRequest
        where TWorkflow : IWorkflow<TWorkflowRequest>, IWorkflowResult<TWorkflowResult>
    {
        replaySafeLogger.LogInformation("Starting sub orchestration with {instanceId}", request.InstanceId);

        return context.CallSubOrchestratorAsync<TWorkflowResult?>(typeof(TWorkflow).Name, request.InstanceId, request);
    }

    /// <summary>
    /// Triggers an orchestration of the given workflow without waiting for the result.
    /// </summary>
    /// <typeparam name="TWorkflowRequest"></typeparam>
    /// <typeparam name="TWorkflow"></typeparam>
    /// <param name="context"></param>
    /// <param name="replaySafeLogger"></param>
    /// <param name="request"></param>
    public static void TriggerWorkflow<TWorkflowRequest, TWorkflow>(
        this IDurableOrchestrationContext context,
        ILogger replaySafeLogger,
        TWorkflowRequest request)
        where TWorkflowRequest : IWorkflowRequest
        where TWorkflow : IWorkflow<TWorkflowRequest>
    {
        replaySafeLogger.LogInformation("Starting orchestration with {instanceId}", request.InstanceId);

        context.StartNewOrchestration(typeof(TWorkflow).Name, request, request.InstanceId);
    }
}
