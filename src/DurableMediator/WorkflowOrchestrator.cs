using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DurableMediator;

internal class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly IWorkflowResolver _workflowResolver;
    private readonly WorkflowConfiguration _config;
    private readonly IDurableClientFactory _durableClientFactory;
    private readonly ILogger<WorkflowOrchestrator> _logger;

    public WorkflowOrchestrator(
        IWorkflowResolver workflowResolver,
        IOptions<WorkflowConfiguration> config,
        IDurableClientFactory durableClientFactory,
        ILogger<WorkflowOrchestrator> logger)
    {
        _workflowResolver = workflowResolver;
        _config = config.Value;
        _durableClientFactory = durableClientFactory;
        _logger = logger;
    }

    public async Task<WorkflowStartResult> StartNewAsync<TRequest, TResponse>(TRequest input)
        where TRequest : IWorkflowRequest<TResponse>
    {
        var client = _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

        var request = new GenericWorkflowRequest(input.InstanceId, input);

        await client.StartNewAsync(typeof(TRequest).Name, request.InstanceId, request).ConfigureAwait(false);

        return new WorkflowStartResult(request.InstanceId);
    }

    public Task<WorkflowStartResult> StartNewAsync<TRequest>(TRequest input)
        where TRequest : IWorkflowRequest
        => StartNewAsync<TRequest, Unit>(input);

    // TODO: move to internal
    public async Task OrchestrateAsync(IDurableOrchestrationContext context)
    {
        var workflow = _workflowResolver.GetWorkflow(context.Name);

        var entityId = _workflowResolver.GetEntityId(context);

        using var _ = _logger.BeginScope(new Dictionary<string, object?>
        {
            { "entityId", entityId }
        });

        var proxy = context.CreateEntityProxy<IDurableMediator>(entityId);

        try
        {
            context.SetCustomStatus(new WorkflowState(workflow.Name, null));

            await workflow.OrchestrateAsync(context, entityId, proxy).ConfigureAwait(true);
        }
        catch (Exception ex) when (LogException(ex, "Orchestration failed"))
        {
            context.SetCustomStatus(new WorkflowState(workflow.Name, ex.Message));

            throw;
        }
    }

    private bool LogException(Exception ex, string message)
    {
        _logger.LogError(ex, message);
        return true;
    }
}
