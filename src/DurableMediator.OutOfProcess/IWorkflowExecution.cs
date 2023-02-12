using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

public interface IWorkflowExecution<TRequest>
{
    TaskOrchestrationContext OrchestrationContext { get; }

    /// <summary>
    /// A replay safe logger for use during the execution of the workflow.
    /// </summary>
    ILogger ReplaySafeLogger { get; }

    /// <summary>
    /// The original request that started the workflow.
    /// </summary>
    TRequest Request { get; }

    /// <summary>
    /// Executes the given request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task SendAsync(IRequest request);

    /// <summary>
    /// Executes the given request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);

    /// <summary>
    /// Tries the given action for maxAttempts times.<br /><br />
    /// If the action is not successful after maxAttempts, TaskFailedException is thrown;
    /// </summary>
    /// <param name="request"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    /// <exception cref="TaskFailedException" />
    Task SendWithRetryAsync(
        IRequest request,
        int maxAttempts = 3,
        TimeSpan? delay = null);

    /// <summary>
    /// Tries the given action for maxAttempts times.<br /><br />
    /// If the action is not successful after maxAttempts, TaskFailedException is thrown;
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    /// <exception cref="TaskFailedException" />
    Task<TResponse> SendWithRetryAsync<TResponse>(
        IRequest<TResponse> request,
        int maxAttempts = 3,
        TimeSpan? delay = null);

    /// <summary>
    /// Waits for the given delay and then executes the request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="delay"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task SendWithDelayAsync(
        IRequest request,
        TimeSpan? delay,
        CancellationToken token);

    /// <summary>
    /// Waits for the given delay and then executes the request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="delay"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<TResponse> SendWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        TimeSpan? delay,
        CancellationToken token);

    /// <summary>
    /// Tries the given action for maxAttempts times.<br /><br />
    /// If the action is not successful, the checkIfRequestApplied is executed after delay, 
    /// and if that results in null, it will retry the process.<br /><br />
    /// If the action is not successful after maxAttempts, TaskFailedException is thrown;
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="checkIfRequestApplied"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    /// <exception cref="TaskFailedException" />
    Task<TResponse> SendWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        int maxAttempts = 3,
        TimeSpan? delay = null);

    /// <summary>
    /// Starts a sub orchestration of the given workflow and waits for the result.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="TaskFailedException" />
    Task CallSubWorkflowAsync(IWorkflowRequest request);

    /// <summary>
    /// Starts a sub orchestration of the given workflow and waits for the result.
    /// </summary>
    /// <typeparam name="TWorkflowResponse"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="TaskFailedException" />
    Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request);
}
