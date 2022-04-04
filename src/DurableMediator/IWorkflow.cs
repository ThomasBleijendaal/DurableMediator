using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public interface IWorkflow<TRequest, TResponse>
    where TRequest : IWorkflowRequest<TResponse>
{
    string Name { get; }

    Task<TResponse> OrchestrateAsync(IDurableOrchestrationContext context, TRequest request, EntityId entityId, IDurableMediator mediator);
}

internal class WorkflowWrapper<TRequest, TResponse> : IWorkflow
    where TRequest : IWorkflowRequest<TResponse>
{
    private readonly IWorkflow<TRequest, TResponse> _workflow;

    public WorkflowWrapper(
        IWorkflow<TRequest, TResponse> workflow)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow), $"Workflow of type IWorkflow<{typeof(TRequest).Name}, {typeof(TResponse)}> not found");
    }

    public string Name => _workflow.Name;

    public Task OrchestrateAsync(IDurableOrchestrationContext context, EntityId entityId, IDurableMediator mediator)
    {
        var request = context.GetInput<TRequest>();

        return _workflow.OrchestrateAsync(context, request, entityId, mediator);
    }
}

internal interface IWorkflow
{
    string Name { get; }

    Task OrchestrateAsync(IDurableOrchestrationContext context, EntityId entityId, IDurableMediator mediator);
}
