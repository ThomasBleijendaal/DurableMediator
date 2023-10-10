using DurableMediator.OutOfProcess;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace OutOfProcessFunctionApp.Workflows;

/// <summary>
/// The resilient workflow demonstrates how to handle requests that are prone to fail.
/// </summary>
[DurableTask(nameof(ResilientWorkflow))]
public class ResilientWorkflow : Workflow<ResilientWorkflowRequest>
{
    public override async Task OrchestrateAsync(IWorkflowExecution<ResilientWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.SendAsync(new SimpleRequest(execution.OrchestrationContext.NewGuid(), "1"));

        var result = await execution.SendWithRetryAsync(
            new ErrorProneRequest(execution.Request.DodgyResourceId),
            maxAttempts: 3);

        logger.LogInformation("Workflow done with id {id}", result.Id);
    }
}
