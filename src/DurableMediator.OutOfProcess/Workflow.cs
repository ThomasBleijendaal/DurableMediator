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
        var result = await OrchestrateAsync(executor);
        return result;
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

public abstract class Workflow
{
    public static async Task StartAsync<TWorkflow, TRequest>(TaskOrchestrationContext context)
        where TWorkflow : IWorkflow<TRequest>, new()
    {
        var workflow = new TWorkflow();

        var logger = context.CreateReplaySafeLogger(typeof(TWorkflow));

        logger.BeginScope(new Dictionary<string, object?> { { "instanceId", context.InstanceId } });

        var input = context.GetInput<TRequest>() ?? throw new InvalidOperationException("Input invalid");

        var executor = new OrchestratorExecutor<TRequest>(context, input, logger);
        await workflow.OrchestrateAsync(executor);
    }

    public static async Task<TResponse> StartAsync<TWorkflow, TRequest, TResponse>(TaskOrchestrationContext context)
        where TWorkflow : IWorkflow<TRequest, TResponse>, new()
    {
        var workflow = new TWorkflow();

        var logger = context.CreateReplaySafeLogger(typeof(TWorkflow));

        logger.BeginScope(new Dictionary<string, object?> { { "instanceId", context.InstanceId } });

        var input = context.GetInput<TRequest>() ?? throw new InvalidOperationException("Input invalid");

        var executor = new OrchestratorExecutor<TRequest>(context, input, logger);
        return await workflow.OrchestrateAsync(executor);
    }
}
