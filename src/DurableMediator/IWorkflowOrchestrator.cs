using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

/// <summary>
/// Entry point for workflow orchestration. This method starts new durable orchestration of the associated workflow.
/// </summary>
public interface IWorkflowOrchestrator
{
    Task<WorkflowStartResult> StartNewAsync<TRequest, TResponse>(TRequest input)
        where TRequest : IWorkflowRequest<TResponse>;

    Task<WorkflowStartResult> StartNewAsync<TRequest>(TRequest input)
        where TRequest : IWorkflowRequest;

    Task OrchestrateAsync(IDurableOrchestrationContext context);
}
