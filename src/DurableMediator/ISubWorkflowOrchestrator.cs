namespace DurableMediator;

public interface ISubWorkflowOrchestrator
{
    /// <summary>
    /// Starts a sub orchestration of the given workflow and waits for the result.
    /// </summary>
    /// <typeparam name="TWorkflowRequest"></typeparam>
    /// <typeparam name="TWorkflowResponse"></typeparam>
    /// <param name="context"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request);

    /// <summary>
    /// Triggers an orchestration of the given workflow without waiting for the result.
    /// </summary>
    /// <typeparam name="TWorkflowRequest"></typeparam>
    /// <param name="context"></param>
    /// <param name="request"></param>
    void StartWorkflow(IWorkflowRequest request);
}
