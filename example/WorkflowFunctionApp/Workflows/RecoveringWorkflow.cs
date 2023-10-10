using DurableMediator;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace WorkflowFunctionApp.Workflows;

/// <summary>
/// The recovering workflow demonstrates how to handle requests that can 'successfully fail' and require a check if they actually succeeded before retrying.
/// </summary>
internal record RecoveringWorkflow() : IWorkflow<RecoveringWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(IWorkflowExecution<RecoveringWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.SendWithCheckAsync(
            new FailingRequest(execution.Request.FlakyResourceId),
            new CheckIfFailingRequestAppliedRequest(execution.Request.FlakyResourceId),
            CancellationToken.None,
            maxAttempts: 20);

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
