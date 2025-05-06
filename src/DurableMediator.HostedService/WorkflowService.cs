using DurableMediator.HostedService.Extensions;
using DurableMediator.HostedService.Models;
using DurableTask.Core;
using DurableTask.Core.Serializing;

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

    public async Task<WorkflowResult?> GetWorkflowResultAsync<TWorkflowRequest>(string instanceId)
        where TWorkflowRequest : IWorkflowRequest
    {
        var state = await _taskHubClient.GetOrchestrationStateAsync(new OrchestrationInstance { InstanceId = instanceId });
        return state == null
            ? null
            : new() { State = state };
    }

    public async Task<WorkflowResult<TWorkflowResponse>?> GetWorkflowResultAsync<TWorkflowRequest, TWorkflowResponse>(string instanceId)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>
    {
        var state = await _taskHubClient.GetOrchestrationStateAsync(new OrchestrationInstance { InstanceId = instanceId });
        if (state == null)
        {
            return default;
        }

        if (state.Output == null)
        {
            return new() { State = state };
        }

        // use the default json data converter from durable task as the string result from TaskOrchestration is parsed by this converter too
        var result = JsonDataConverter.Default.Deserialize(state.Output, typeof(TWorkflowResponse));

        return new() { State = state, Result = (TWorkflowResponse?)result };
    }
}
