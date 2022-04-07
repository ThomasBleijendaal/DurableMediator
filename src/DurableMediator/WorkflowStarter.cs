using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DurableMediator;

internal class WorkflowStarter : IWorkflowStarter
{
    private readonly WorkflowConfiguration _config;
    private readonly IDurableClientFactory _durableClientFactory;

    public WorkflowStarter(
        IOptions<WorkflowConfiguration> config,
        IDurableClientFactory durableClientFactory)
    {
        _config = config.Value;
        _durableClientFactory = durableClientFactory;
    }

    public async Task<WorkflowStartResult> StartNewAsync<TResponse>(IWorkflowRequest<TResponse> input)
    {
        var client = _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

        var request = new GenericWorkflowRequest(input.InstanceId, JObject.FromObject(input));

        await client.StartNewAsync(input.GetType().Name, request.InstanceId, request).ConfigureAwait(false);

        return new WorkflowStartResult(request.InstanceId);
    }

    public Task<WorkflowStartResult> StartNewAsync(IWorkflowRequest input) => StartNewAsync<Unit>(input);
}
