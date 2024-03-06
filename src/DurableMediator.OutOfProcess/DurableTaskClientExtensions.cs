using Microsoft.DurableTask.Client;

namespace DurableMediator.OutOfProcess;

public static class DurableTaskClientExtensions
{
    public static Task<string> StartWorkflowAsync<TRequest>(this DurableTaskClient client, TRequest request)
        where TRequest : IWorkflowRequest
    {
        return client.ScheduleNewOrchestrationInstanceAsync(request.WorkflowName, request);
    }
}
