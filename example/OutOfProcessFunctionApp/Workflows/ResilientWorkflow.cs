using DurableMediator.OutOfProcess;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace OutOfProcessFunctionApp.Workflows;

/// <summary>
/// The resilient workflow demonstrates how to handle requests that are prone to fail.
/// </summary>
[DurableTask(nameof(ResilientWorkflow))]
public class ResilientWorkflow : Workflow<ResilientWorkflowRequest, Unit>
{
    public override async Task<Unit> OrchestrateAsync(IWorkflowExecution<ResilientWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.SendAsync(new SimpleRequest(execution.OrchestrationContext.NewGuid(), "1"));

        var result = await execution.SendWithRetryAsync(
            new ErrorProneRequest(execution.Request.DodgyResourceId), CancellationToken.None,
            maxAttempts: 3);

        logger.LogInformation("Workflow done with id {id}", result.Id);

        return Unit.Value;
    }
}
