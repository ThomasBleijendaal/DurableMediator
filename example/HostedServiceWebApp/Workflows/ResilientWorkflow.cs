using DurableMediator.HostedService;
using WorkflowHandlers.Requests;

namespace HostedServiceWebApp.Workflows;

/// <summary>
/// The resilient workflow demonstrates how to handle requests that are prone to fail.
/// </summary>
public class ResilientWorkflow : IWorkflow<ResilientWorkflowRequest>
{
    public async Task OrchestrateAsync(IWorkflowExecution<ResilientWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.SendAsync(new SimpleRequest(execution.NewGuid(), "1"));

        var result = await execution.SendWithRetryAsync(
            new ErrorProneRequest(execution.Request.DodgyResourceId),
            maxAttempts: 3);

        logger.LogInformation("Workflow done with id {id}", result.Id);
    }
}
