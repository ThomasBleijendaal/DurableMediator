using DurableMediator.HostedService.Models;

namespace DurableMediator.HostedService;

public interface IWorkflowService
{
    Task<string> StartWorkflowAsync<TWorkflowRequest>(TWorkflowRequest workflowRequest)
        where TWorkflowRequest : IWorkflowRequest;

    Task<WorkflowResult?> GetWorkflowResultAsync<TWorkflowRequest>(string instanceId)
        where TWorkflowRequest : IWorkflowRequest;

    Task<WorkflowResult<TWorkflowResponse>?> GetWorkflowResultAsync<TWorkflowRequest, TWorkflowResponse>(string instanceId)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>;
}
