namespace DurableMediator;

/// <summary>
/// Entry point for workflow orchestration. This method starts new durable orchestration of the associated workflow.
/// </summary>
public interface IWorkflowStarter
{
    /// <summary>
    /// Starts a new workflow. 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<WorkflowStartResult> StartNewAsync<TResponse>(IWorkflowRequest<TResponse> input);

    /// <summary>
    /// Starts a new workflow.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<WorkflowStartResult> StartNewAsync(IWorkflowRequest input);
}
