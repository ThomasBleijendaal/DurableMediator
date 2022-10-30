using DurableMediator;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

/// <summary>
/// The resilient workflow demonstrates how to handle requests that are prone to fail.
/// </summary>
/// <param name="Logger"></param>
internal record ResilientWorkflow(ILogger<ResilientWorkflow> Logger) : IWorkflow<ResilientWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(IWorkflowExecution<ResilientWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.ExecuteAsync(new SimpleRequest(execution.OrchestrationContext.NewGuid(), "1"));

        var result = await execution.ExecuteWithRetryAsync(
            new ErrorProneRequest(execution.Request.DodgyResourceId), CancellationToken.None, 
            maxAttempts: 3);

        logger.LogInformation("Workflow done with id {id}", result.Id);

        return Unit.Value;
    }
}
