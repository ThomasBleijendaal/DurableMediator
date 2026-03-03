using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;

namespace DurableMediator.OutOfProcess;

public static class DurableTaskClientExtensions
{
    public static Task<string> StartWorkflowAsync<TRequest>(this DurableTaskClient client, TRequest request)
        where TRequest : IWorkflowRequest
    {
        return client.ScheduleNewOrchestrationInstanceAsync(request.WorkflowName, request, new StartOrchestrationOptions(request.InstanceId));
    }

    public static async Task RaiseEventAsync<TWorkflowEvent>(this DurableTaskClient client, TWorkflowEvent @event)
        where TWorkflowEvent : IWorkflowEvent
    {
        if (typeof(TWorkflowEvent).GetInterfaces().FirstOrDefault(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(IWorkflowEvent<>)) is { } startableWorkflowEventType)
        {
            var method = startableWorkflowEventType.GetMethod(nameof(IWorkflowEvent<>.GetWorkflowRequest));

            try
            {
                var instance = await client.StartWorkflowAsync((IWorkflowRequest)method!.Invoke(@event, [])!);
                if (instance != @event.InstanceId)
                {
                    throw new InvalidOperationException("Instance id from event is not the same as started workflow");
                }
            }
            catch (Exception ex)
            {
                // TODO:
            }
        }

        await client.RaiseEventAsync(@event.InstanceId, @event.EventName, @event);
    }
}
