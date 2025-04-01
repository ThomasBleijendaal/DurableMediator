using DurableMediator.HostedService.Activities;
using DurableMediator.HostedService.Constants;
using DurableMediator.HostedService.Models;
using DurableTask.Core;
using MediatR;

namespace DurableMediator.HostedService.Extensions;

internal static class TaskOrchestrationContextExtensions
{
    public static Task<Unit> CallDurableMediatorAsync(this OrchestrationContext ctx, MediatorRequest input, RetryOptions? options = null)
    {
        if (options == null)
        {
            return ctx.ScheduleTask<Unit>(MediatorRequestActivity.Name, Versions.Default, input);
        }
        else
        {
            return ctx.ScheduleWithRetry<Unit>(MediatorRequestActivity.Name, Versions.Default, options, input);
        }
    }

    public static Task<MediatorResponse> CallDurableMediatorWithResponseAsync(this OrchestrationContext ctx, MediatorRequestWithResponse input, RetryOptions? options = null)
    {
        if (options == null)
        {
            return ctx.ScheduleTask<MediatorResponse>(MediatorRequestWithResponseActivity.Name, Versions.Default, input);
        }
        else
        {
            return ctx.ScheduleWithRetry<MediatorResponse>(MediatorRequestWithResponseActivity.Name, Versions.Default, options, input);
        }
    }

    public static Task<MediatorResponse> CallDurableMediatorWithCheckAndResponseAsync(this OrchestrationContext ctx, MediatorRequestWithCheckAndResponse input, RetryOptions? options = null)
    {
        if (options == null)
        {
            return ctx.ScheduleTask<MediatorResponse>(MediatorRequestWithCheckAndResponseActivity.Name, Versions.Default, input);
        }
        else
        {
            return ctx.ScheduleWithRetry<MediatorResponse>(MediatorRequestWithCheckAndResponseActivity.Name, Versions.Default, options, input);
        }
    }
}
