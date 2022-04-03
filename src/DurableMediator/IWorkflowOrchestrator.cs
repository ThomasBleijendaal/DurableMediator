using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

/// <summary>
/// Entry point for workflow orchestration. This method starts new durable task invocations
/// and orchestrates them.
/// 
/// This method assumes that the nameof(TWorkflow) function is present as [OrchestrationTrigger],
/// and TWorkflow can be resolved from DI.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TWorkflow"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public interface IWorkflowOrchestrator<TRequest, TWorkflow>
    where TWorkflow : class, IWorkflow<TRequest>
    where TRequest : IWorkflowRequest
{
    Task<WorkflowStartResult> StartNewAsync(TRequest input);

    Task OrchestrateAsync(IDurableOrchestrationContext context);
}
