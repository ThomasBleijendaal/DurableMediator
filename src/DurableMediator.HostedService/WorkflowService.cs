using DurableMediator.HostedService.Constants;
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
        var instance = await _taskHubClient.CreateOrchestrationInstanceAsync(workflowRequest.WorkflowName, Versions.Default, workflowRequest.InstanceId, workflowRequest);

        return instance.InstanceId;
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

    public async Task RaiseEventAsync<TWorkflowEvent>(TWorkflowEvent @event) where TWorkflowEvent : IWorkflowEvent
    {
        if (typeof(TWorkflowEvent).GetInterfaces().FirstOrDefault(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(IWorkflowEvent<>)) is { } startableWorkflowEventType)
        {
            var method = startableWorkflowEventType.GetMethod(nameof(IWorkflowEvent<>.GetWorkflowRequest));
            var workflowRequest = (IWorkflowRequest)method!.Invoke(@event, [])!;

            var state = await _taskHubClient.GetOrchestrationStateAsync(new OrchestrationInstance { InstanceId = @event.InstanceId });

            if (state == null || !(state.OrchestrationStatus == OrchestrationStatus.Running || state.OrchestrationStatus == OrchestrationStatus.Pending))
            {
                await _taskHubClient.CreateOrchestrationInstanceWithRaisedEventAsync(
                    workflowRequest.WorkflowName,
                    Versions.Default,
                    workflowRequest.InstanceId,
                    workflowRequest,
                    @event.EventName,
                    @event);

                return;
            }
        }

        await _taskHubClient.RaiseEventAsync(new OrchestrationInstance { InstanceId = @event.InstanceId }, @event.EventName, @event);
    }
}
