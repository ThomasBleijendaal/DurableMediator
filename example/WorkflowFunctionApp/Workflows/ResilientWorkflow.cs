using DurableMediator;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace WorkflowFunctionApp.Workflows;

/// <summary>
/// The resilient workflow demonstrates how to handle requests that are prone to fail.
/// </summary>
internal record ResilientWorkflow() : IWorkflow<ResilientWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(IWorkflowExecution<ResilientWorkflowRequest> execution)
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
