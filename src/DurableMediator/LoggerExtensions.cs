using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal static class LoggerExtensions
{
    public static IDisposable BeginTracingScope(
        this ILogger logger,
        ITracingProvider tracingProvider,
        Tracing tracing,
        EntityId entityId,
        string instanceId,
        string activityName)
        => logger.BeginScope(tracingProvider.EnrichLogScope(
            tracing,
            new Dictionary<string, object?>
            {
                { "entityId", entityId },
                { "instanceId", instanceId },
                { "activityName", activityName }
            }));

    public static bool LogException(
        this ILogger logger,
        Exception ex, 
        string message)
    {
        logger.LogError(ex, message);

        return true;
    }
}
