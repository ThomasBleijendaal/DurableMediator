namespace DurableMediator;

/// <summary>
/// Entry point for workflow orchestration. This method starts new durable orchestration of the associated workflow.
/// </summary>
public interface IWorkflowStarter
{
    Task<WorkflowStartResult> StartNewAsync<TResponse>(IWorkflowRequest<TResponse> input);

    Task<WorkflowStartResult> StartNewAsync(IWorkflowRequest input);
}
