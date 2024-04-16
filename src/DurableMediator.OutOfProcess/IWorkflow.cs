namespace DurableMediator.OutOfProcess;

public interface IWorkflow
{
}

public interface IWorkflow<TRequest> : IWorkflow
{
    public abstract Task OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}

public interface IWorkflow<TRequest, TResponse> : IWorkflow
{
    public abstract Task<TResponse> OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}
