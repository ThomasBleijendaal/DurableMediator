using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Options;

namespace DurableMediator;

internal class WorkflowStarter : IWorkflowStarter
{
    private readonly WorkflowConfiguration _config;
    private readonly ITracingProvider _tracingProvider;
    private readonly IDurableClientFactory _durableClientFactory;

    public WorkflowStarter(
        IOptions<WorkflowConfiguration> config,
        ITracingProvider tracingProvider,
        IDurableClientFactory durableClientFactory)
    {
        _config = config.Value;
        _tracingProvider = tracingProvider;
        _durableClientFactory = durableClientFactory;
    }

    public async Task<WorkflowStartResult> StartNewAsync<TResponse>(IWorkflowRequest<TResponse> input)
    {
        var client = _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

        var wrappedInput = new WorkflowRequestWrapper<IWorkflowRequest<TResponse>>(
            _tracingProvider.GetTracing(),
            input);

        await client.StartNewAsync(input.GetType().Name, input.InstanceId, wrappedInput).ConfigureAwait(false);

        return new WorkflowStartResult(input.InstanceId);
    }

    public Task<WorkflowStartResult> StartNewAsync(IWorkflowRequest input) => StartNewAsync<Unit>(input);
}
