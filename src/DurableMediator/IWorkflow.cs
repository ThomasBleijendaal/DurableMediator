namespace DurableMediator;

public interface IWorkflow<TRequest, TResponse>
    where TRequest : IWorkflowRequest<TResponse>
{
    string Name { get; }

    Task<TResponse> OrchestrateAsync(WorkflowContext<TRequest> context);
}
