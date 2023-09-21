using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.DependencyInjection;

namespace DurableMediator.OutOfProcess;

public static class GeneratedDurableTaskExtensions
{
    public const string DurableMediatorName = "DurableMediator";
    public const string DurableMediatorWithResponseName = "DurableMediatorWithResponse";

    public static Task<Unit> CallDurableMediatorAsync(this TaskOrchestrationContext ctx, MediatorRequest input, TaskOptions? options = null)
    {
        return ctx.CallActivityAsync<Unit>(DurableMediatorName, input, options);
    }

    [Function(DurableMediatorName)]
    public static async Task<Unit> DurableMediatorAsync([ActivityTrigger] MediatorRequest input, string instanceId, FunctionContext executionContext)
    {
        ITaskActivity activity = ActivatorUtilities.CreateInstance<DurableMediator>(executionContext.InstanceServices);
        TaskActivityContext context = new GeneratedActivityContext(DurableMediatorName, instanceId);
        var result = await activity.RunAsync(context, input);
        return (Unit)result!;
    }

    public static Task<MediatorResponse> CallDurableMediatorWithResponseAsync(this TaskOrchestrationContext ctx, MediatorRequestWithResponse input, TaskOptions? options = null)
    {
        return ctx.CallActivityAsync<MediatorResponse>(DurableMediatorWithResponseName, input, options);
    }

    [Function(DurableMediatorWithResponseName)]
    public static async Task<MediatorResponse> DurableMediatorWithResponseAsync([ActivityTrigger] MediatorRequestWithResponse input, string instanceId, FunctionContext executionContext)
    {
        ITaskActivity activity = ActivatorUtilities.CreateInstance<DurableMediatorWithResponse>(executionContext.InstanceServices);
        TaskActivityContext context = new GeneratedActivityContext(DurableMediatorWithResponseName, instanceId);
        var result = await activity.RunAsync(context, input);
        return (MediatorResponse)result!;
    }

    private sealed class GeneratedActivityContext : TaskActivityContext
    {
        public GeneratedActivityContext(TaskName name, string instanceId)
        {
            Name = name;
            InstanceId = instanceId;
        }

        public override TaskName Name { get; }

        public override string InstanceId { get; }
    }
}
