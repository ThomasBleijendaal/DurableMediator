using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class WorkflowWrapper<TRequest, TResponse> : IWorkflowWrapper
    where TRequest : class, IWorkflowRequest<TResponse>
{
    private readonly IWorkflow<TRequest, TResponse> _workflow;
    private readonly ITracingProvider _tracingProvider;

    public WorkflowWrapper(
        IWorkflow<TRequest, TResponse> workflow,
        ITracingProvider tracingProvider)
    {
        _workflow = workflow;
        _tracingProvider = tracingProvider;
    }

    public Type WorkflowType => _workflow.GetType();

    public async Task OrchestrateAsync(
        IDurableOrchestrationContext context,
        EntityId entityId,
        IDurableMediator mediator,
        ILogger replaySafeLogger)
    {
        var requestWrapper = context.GetInput<WorkflowRequestWrapper<TRequest>>();

        using var _ = replaySafeLogger.BeginTracingScope(
            _tracingProvider,
            requestWrapper.Tracing,
            entityId,
            requestWrapper.Request.InstanceId,
            context.Name);

        try
        {
            var response = await _workflow.OrchestrateAsync(
                new WorkflowExecution<TRequest>(
                    requestWrapper.Request,
                    context,
                    entityId,
                    mediator,
                    replaySafeLogger));

            if (response is TResponse workflowResponse)
            {
                context.SetOutput(workflowResponse);
            }
        }
        catch (Exception ex) when (replaySafeLogger.LogException(ex, "Orchestration failed"))
        {
            context.SetCustomStatus(new WorkflowErrorState(ex.Message));

            throw;
        }
    }
}
