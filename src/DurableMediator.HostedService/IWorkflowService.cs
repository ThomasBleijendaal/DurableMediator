namespace DurableMediator.HostedService;

public interface IWorkflowService
{
    Task<string> StartWorkflowAsync<TWorkflowRequest>(TWorkflowRequest workflowRequest)
        where TWorkflowRequest : IWorkflowRequest;

    Task<TWorkflowResponse?> GetWorkflowResultAsync<TWorkflowRequest, TWorkflowResponse>(string instanceId)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>;
}
