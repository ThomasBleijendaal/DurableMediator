using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal class WorkflowWrapper<TRequest, TResponse> : IWorkflowWrapper
    where TRequest : class, IWorkflowRequest<TResponse>
{
    private readonly IWorkflow<TRequest, TResponse> _workflow;

    public WorkflowWrapper(
        IWorkflow<TRequest, TResponse> workflow)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow), $"Workflow of type IWorkflow<{typeof(TRequest).Name}, {typeof(TResponse)}> not found");
    }

    public string Name => _workflow.Name;

    public async Task OrchestrateAsync(IDurableOrchestrationContext context, EntityId entityId, IDurableMediator mediator)
    {
        var request = context.GetInput<TRequest>();

        var response = await _workflow.OrchestrateAsync(
            new WorkflowContext<TRequest>(
                request, 
                context,
                new SubWorkflowOrchestrator(context),
                entityId, 
                mediator));

        if (response is TResponse workflowResponse)
        {
            context.SetOutput(workflowResponse);
        }
    }
}
