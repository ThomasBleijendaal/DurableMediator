using MediatR;
using Microsoft.DurableTask;

namespace DurableMediator.OutOfProcess;

internal static class TaskOrchestrationContextExtensions
{
    public static Task<Unit> CallDurableMediatorAsync(this TaskOrchestrationContext ctx, MediatorRequest input, TaskOptions? options = null)
    {
        return ctx.CallActivityAsync<Unit>(DurableMediatorFunction.DurableMediatorName, input, options);
    }

    public static Task<MediatorResponse> CallDurableMediatorWithResponseAsync(this TaskOrchestrationContext ctx, MediatorRequestWithResponse input, TaskOptions? options = null)
    {
        return ctx.CallActivityAsync<MediatorResponse>(DurableMediatorFunction.DurableMediatorWithResponseName, input, options);
    }

    public static Task<MediatorResponse> CallDurableMediatorWithCheckAndResponseAsync(this TaskOrchestrationContext ctx, MediatorRequestWithCheckAndResponse input, TaskOptions? options = null)
    {
        return ctx.CallActivityAsync<MediatorResponse>(DurableMediatorFunction.DurableMediatorWithCheckAndResponseName, input, options);
    }
}
