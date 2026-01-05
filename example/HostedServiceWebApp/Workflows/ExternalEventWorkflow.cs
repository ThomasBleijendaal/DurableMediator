using DurableMediator.HostedService;
using WorkflowHandlers.Requests;

namespace HostedServiceWebApp.Workflows;

public class ExternalEventWorkflow : IWorkflow<ExternalEventWorkflowRequest>
{
    public async Task OrchestrateAsync(IWorkflowExecution<ExternalEventWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Started with the workflow");

        var event1and2Task = Event1And2Async(execution, logger);

        var event34and5Task = Event34And5Async(execution, logger);

        await Task.WhenAll(event1and2Task, event34and5Task);

        logger.LogInformation("Done with the workflow");
    }

    private static async Task Event1And2Async(IWorkflowExecution<ExternalEventWorkflowRequest> execution, ILogger logger)
    {
        var event1 = await execution.WaitForEventAsync<ExternalEvent>("external-event-1", TimeSpan.FromSeconds(1));

        if (event1 is null)
        {
            logger.LogInformation("Event 1 did not fire in time");

            await execution.SendAsync(new SimpleRequest(execution.Request.RequestId, "Event 1 did not fire"));
        }
        else
        {
            logger.LogInformation("Event 1 - {id}", event1.EventId);

            var event2 = await execution.WaitForEventAsync<ExternalEvent>("external-event-2", TimeSpan.FromSeconds(10));

            if (event2 is null)
            {
                logger.LogInformation("Event 2 did not fire in time");

                await execution.SendAsync(new SimpleRequest(execution.Request.RequestId, "Event 2 did not fire"));
            }
            else
            {
                logger.LogInformation("Event 2 - {id}", event2.EventId);

                await execution.SendAsync(new SlowRequest(new Guid(
                    event1.EventId.ToByteArray()
                        .Zip(event2.EventId.ToByteArray())
                        .Select(x => (byte)(x.First + x.Second))
                        .ToArray())));
            }
        }
    }

    private static async Task Event34And5Async(IWorkflowExecution<ExternalEventWorkflowRequest> execution, ILogger logger)
    {
        var event3 = await execution.WaitForEventAsync<ExternalEvent>("external-event-3", TimeSpan.FromSeconds(60));

        logger.LogInformation("Event 3 - {id}", event3?.EventId);

        var event4 = execution.WaitForEventAsync<ExternalEvent>("external-event-4", Timeout.InfiniteTimeSpan);
        var event5 = execution.WaitForEventAsync<ExternalEvent>("external-event-5", Timeout.InfiniteTimeSpan);

        await Task.WhenAll(event4, event5);

        logger.LogInformation("Event 4 & 5 - {id1} - {id2}", event4.Result!.EventId, event5.Result!.EventId);
    }
}
