using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class WorkflowWrapper<TRequest, TResponse> : IWorkflowWrapper
    where TRequest : class, IWorkflowRequest<TResponse>
{
    private readonly IDurableClient _durableClient;
    private readonly IWorkflow<TRequest, TResponse> _workflow;
    private readonly ITracingProvider _tracingProvider;

    public WorkflowWrapper(
        IDurableClient durableClient,
        IWorkflow<TRequest, TResponse> workflow,
        ITracingProvider tracingProvider)
    {
        _durableClient = durableClient;
        _workflow = workflow;
        _tracingProvider = tracingProvider;
    }

    public Type WorkflowType => _workflow.GetType();

    public async Task OrchestrateAsync(
        IDurableOrchestrationContext context,
        ILogger replaySafeLogger)
    {
        var requestWrapper = context.GetInput<WorkflowRequestWrapper<TRequest>>();

        using var _ = replaySafeLogger.BeginTracingScope(
            _tracingProvider,
            requestWrapper.Tracing,
            requestWrapper.Request.InstanceId,
            context.Name);

        var durableMediatorId = new EntityId(nameof(DurableMediatorEntity), requestWrapper.Request.InstanceId);
        var durableMediator = context.CreateEntityProxy<IDurableMediator>(durableMediatorId);

        try
        {
            var response = await _workflow.OrchestrateAsync(
                new WorkflowExecution<TRequest>(
                    requestWrapper.Request,
                    durableMediator,
                    context,
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
        finally
        {
            _durableClient.PurgeInstanceHistoryAsync(durableMediatorId.ToString());
        }
    }
}
