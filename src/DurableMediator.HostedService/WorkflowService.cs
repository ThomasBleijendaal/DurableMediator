using System.Text.Json;
using DurableMediator.HostedService.Extensions;
using DurableTask.Core;

namespace DurableMediator.HostedService;

internal class WorkflowService : IWorkflowService
{
    private readonly TaskHubClient _taskHubClient;

    public WorkflowService(TaskHubClient taskHubClient)
    {
        _taskHubClient = taskHubClient;
    }

    public async Task<string> StartWorkflowAsync<TWorkflowRequest>(TWorkflowRequest workflowRequest)
        where TWorkflowRequest : IWorkflowRequest
    {
        return await _taskHubClient.StartWorkflowAsync(workflowRequest);
    }

    public async Task<TWorkflowResponse?> GetWorkflowResultAsync<TWorkflowRequest, TWorkflowResponse>(string instanceId)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>
    {
        var state = await _taskHubClient.GetOrchestrationStateAsync(new OrchestrationInstance { InstanceId = instanceId });

        if (state.Output == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<TWorkflowResponse>(state.Output);
    }
}
