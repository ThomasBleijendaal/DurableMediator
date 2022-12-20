using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

public interface IWorkflowExecution
{
    IDurableOrchestrationContext OrchestrationContext { get; }

    /// <summary>
    /// A replay safe logger for use during the execution of the workflow.
    /// </summary>
    ILogger ReplaySafeLogger { get; }
    
    /// <summary>
    /// Executes the given request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<TResponse> SendAsync<TResponse>(
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
    Task<TResponse> SendWithRetryAsync<TResponse>(
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
    Task<TResponse> SendWithDelayAsync<TResponse>(
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
    Task<TResponse> SendWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null);

    /// <summary>
    /// Starts a sub orchestration of the given workflow and waits for the result.
    /// </summary>
    /// <typeparam name="TWorkflowResponse"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request);

    /// <summary>
    /// Triggers an orchestration of the given workflow without waiting for the result.
    /// </summary>
    /// <param name="request"></param>
    void StartWorkflow(IWorkflowRequest request);

    /// <summary>
    /// Triggers an orchestration of the given workflow without waiting for the result.
    /// </summary>
    /// <param name="request"></param>
    void StartWorkflow<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request);
}

public interface IWorkflowExecution<TRequest> : IWorkflowExecution
{
    /// <summary>
    /// The request that started the workflow execution.
    /// </summary>
    TRequest Request { get; }
}
