using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public interface IWorkflowExecution
{
    IDurableOrchestrationContext OrchestrationContext { get; }
    EntityId EntityId { get; }
    IDurableMediator DurableMediator { get; }

    /// <summary>
    /// Executes the given request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<TResponse> ExecuteAsync<TResponse>(
        IRequest<TResponse> request);

    /// <summary>
    /// Tries the given action for maxRetries times.
    /// 
    /// If the action is not successful after maxRetries, OrchestrationRetryException is thrown;
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <param name="maxRetries"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    /// <exception cref="OrchestrationRetryException" />

    Task ExecuteWithRetryAsync(
        IRequest<IRetryResponse> request,
        CancellationToken token,
        int maxRetries = 3,
        TimeSpan delay = default);

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
        TimeSpan delay = default);
}
