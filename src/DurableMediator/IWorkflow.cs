namespace DurableMediator;

public interface IWorkflow<TRequest, TResponse>
    where TRequest : IWorkflowRequest<TResponse>
{
    Task<TResponse> OrchestrateAsync(WorkflowContext<TRequest> context);
}
