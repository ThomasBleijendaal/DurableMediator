using DurableMediator.HostedService.Constants;
using DurableTask.Core;

namespace DurableMediator.HostedService.Extensions;

public static class DurableTaskClientExtensions
{
    public static async Task<string> StartWorkflowAsync<TRequest>(this TaskHubClient client, TRequest request)
        where TRequest : IWorkflowRequest
    {
        var instance = await client.CreateOrchestrationInstanceAsync(request.WorkflowName, Versions.Default, request.InstanceId, request);

        return instance.InstanceId;
    }
}
