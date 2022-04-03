using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DurableMediator;

public class WorkflowOrchestrator<TRequest, TWorkflow> : IWorkflowOrchestrator<TRequest, TWorkflow>
    where TWorkflow : class, IWorkflow<TRequest>
    where TRequest : IWorkflowRequest
{
    private readonly TWorkflow _workflow;
    private readonly WorkflowConfiguration _config;
    private readonly IDurableClientFactory _durableClientFactory;
    private readonly ILogger<WorkflowOrchestrator<TRequest, TWorkflow>> _logger;

    public WorkflowOrchestrator(
        TWorkflow workflow,
        IOptions<WorkflowConfiguration> config,
        IDurableClientFactory durableClientFactory,
        ILogger<WorkflowOrchestrator<TRequest, TWorkflow>> logger)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        _config = config.Value;
        _durableClientFactory = durableClientFactory;
        _logger = logger;
    }

    public async Task<WorkflowStartResult> StartNewAsync(TRequest input)
    {
        var client = _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

        await client.StartNewAsync(typeof(TWorkflow).Name, input.InstanceId, input);

        return new WorkflowStartResult(input.InstanceId);
    }

    public async Task OrchestrateAsync(IDurableOrchestrationContext context)
    {
        var request = context.GetInput<TRequest>();

        var entityId = new EntityId(nameof(DurableMediatorEntity), request.InstanceId);

        using var _ = _logger.BeginScope(new Dictionary<string, object?>
        {
            { "entityId", entityId }
        });

        var proxy = context.CreateEntityProxy<IDurableMediator>(entityId);

        try
        {
            context.SetCustomStatus(new WorkflowState(_workflow.Name, request.AffectedEntityId, null));

            await _workflow.OrchestrateAsync(context, request, entityId, proxy);
        }
        catch (Exception ex) when (LogException(ex, "Orchestration failed"))
        {
            context.SetCustomStatus(new WorkflowState(_workflow.Name, request.AffectedEntityId, ex.Message));

            throw;
        }
    }

    private bool LogException(Exception ex, string message)
    {
        _logger.LogError(ex, message);
        return true;
    }
}
