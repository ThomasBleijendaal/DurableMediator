using Microsoft.DurableTask;

namespace DurableMediator.OutOfProcess;

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
