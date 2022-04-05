using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal static class DurableMediatorFunction
{
    public static Task RunAsync(IDurableEntityContext ctx) => ctx.DispatchAsync<DurableMediatorEntity>();
}
