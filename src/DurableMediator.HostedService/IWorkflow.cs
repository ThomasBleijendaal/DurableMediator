namespace DurableMediator.HostedService;

public interface IWorkflow
{
}

public interface IWorkflow<TRequest> : IWorkflow
    where TRequest : IWorkflowRequest
{
    public abstract Task OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}

public interface IWorkflow<TRequest, TResponse> : IWorkflow
    where TRequest : IWorkflowRequest<TResponse>
{
    public abstract Task<TResponse> OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}
