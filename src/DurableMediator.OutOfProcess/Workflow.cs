using Microsoft.DurableTask;

namespace DurableMediator.OutOfProcess;

public abstract class Workflow<TRequest, TResponse> : TaskOrchestrator<TRequest, TResponse>
    where TRequest : IWorkflowRequest<TResponse>
{
    public override sealed async Task<TResponse> RunAsync(TaskOrchestrationContext context, TRequest input)
    {
        var executor = new OrchestratorExecutor<TRequest, TResponse>(context, input, context.CreateReplaySafeLogger(GetType()));
        return await OrchestrateAsync(executor);
    }

    public abstract Task<TResponse> OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}
