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
    public async Task<Unit> OrchestrateAsync(WorkflowExecution<ResilientWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.ExecuteWithRetryAsync(
            new ErrorProneRequest(execution.Request.DodgyResourceId), CancellationToken.None, 
            maxAttempts: 1);

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
