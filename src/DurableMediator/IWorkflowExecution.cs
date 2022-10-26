using Azure;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public interface IWorkflowExecution
{
    IDurableOrchestrationContext OrchestrationContext { get; }
    
    /// <summary>
    /// Executes the given request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<TResponse> ExecuteAsync<TResponse>(
        IRequest<TResponse> request);

    /// <summary>
    /// Tries the given action for maxAttempts times.<br /><br />
    /// If the action is not successful after maxAttempts, OrchestrationRetryException is thrown;
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    /// <exception cref="OrchestrationRetryException" />
    Task<TResponse> ExecuteWithRetryAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null);

    /// <summary>
    /// Waits for the given delay and then executes the request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    Task<TResponse> ExecuteWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        TimeSpan? delay = null);

    /// <summary>
    /// Tries the given action for maxAttempts times.<br /><br />
    /// If the action is not successful, the checkIfRequestApplied is executed after delay, 
    /// and if that results in null, it will retry the process.<br /><br />
    /// If the action is not successful after maxAttempts, OrchestrationRetryException is thrown;
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="checkIfRequestApplied"></param>
    /// <param name="token"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    /// <exception cref="OrchestrationRetryException" />
    Task<TResponse> ExecuteWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null);
}
