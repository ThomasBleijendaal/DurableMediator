using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly IWorkflowResolver _workflowResolver;
    private readonly ILogger<WorkflowOrchestrator> _logger;

    public WorkflowOrchestrator(
        IWorkflowResolver workflowResolver,
        ILogger<WorkflowOrchestrator> logger)
    {
        _workflowResolver = workflowResolver;
        _logger = logger;
    }

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
            await workflow.OrchestrateAsync(context, entityId, proxy).ConfigureAwait(true);
        }
        catch (Exception ex) when (LogException(ex, "Orchestration failed"))
        {
            context.SetCustomStatus(new WorkflowErrorState(ex.Message));

            throw;
        }
    }

    private bool LogException(Exception ex, string message)
    {
        _logger.LogError(ex, message);
        return true;
    }
}
