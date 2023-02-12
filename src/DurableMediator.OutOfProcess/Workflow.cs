using MediatR;
using Microsoft.DurableTask;

namespace DurableMediator.OutOfProcess;

public abstract class Workflow<TRequest, TResponse> : TaskOrchestrator<TRequest, TResponse>
    where TRequest : IWorkflowRequest<TResponse>
{
    public sealed override async Task<TResponse> RunAsync(TaskOrchestrationContext context, TRequest input)
    {
        var logger = context.CreateReplaySafeLogger(GetType());

        logger.BeginScope(new Dictionary<string, object?> { { "instanceId", context.InstanceId } });

        var executor = new OrchestratorExecutor<TRequest>(context, input, logger);
        return await OrchestrateAsync(executor);
    }

    public abstract Task<TResponse> OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}

public abstract class Workflow<TRequest> : TaskOrchestrator<TRequest, Unit>
    where TRequest : IWorkflowRequest
{
    public sealed override async Task<Unit> RunAsync(TaskOrchestrationContext context, TRequest input)
    {
        var logger = context.CreateReplaySafeLogger(GetType());

        logger.BeginScope(new Dictionary<string, object?> { { "instanceId", context.InstanceId } });

        var executor = new OrchestratorExecutor<TRequest>(context, input, logger);
        await OrchestrateAsync(executor);
        return Unit.Value;
    }

    public abstract Task OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}
