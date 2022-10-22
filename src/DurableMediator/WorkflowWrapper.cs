using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class WorkflowWrapper<TRequest, TResponse> : IWorkflowWrapper
    where TRequest : class, IWorkflowRequest<TResponse>
{
    private readonly IWorkflow<TRequest, TResponse> _workflow;
    private readonly ILoggerFactory _loggerFactory;

    public WorkflowWrapper(
        IWorkflow<TRequest, TResponse> workflow,
        ILoggerFactory loggerFactory)
    {
        _workflow = workflow;
        _loggerFactory = loggerFactory;
    }

    public async Task OrchestrateAsync(IDurableOrchestrationContext context, EntityId entityId, IDurableMediator mediator)
    {
        var request = context.GetInput<TRequest>();

        var response = await _workflow.OrchestrateAsync(
            new WorkflowExecution<TRequest>(
                request, 
                context,
                entityId, 
                mediator,
                _loggerFactory.CreateLogger<WorkflowExecution<TRequest>>()));

        if (response is TResponse workflowResponse)
        {
            context.SetOutput(workflowResponse);
        }
    }
}
