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

    TRequest Request { get; }

    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);

    Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowRequest, TWorkflowResponse>(TWorkflowRequest request)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>;
}
