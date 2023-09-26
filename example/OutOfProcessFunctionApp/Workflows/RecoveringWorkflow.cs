using DurableMediator.OutOfProcess;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace OutOfProcessFunctionApp.Workflows;

/// <summary>
/// The recovering workflow demonstrates how to handle requests that can 'successfully fail' and require a check if they actually succeeded before retrying.
/// </summary>
[DurableTask(nameof(RecoveringWorkflow))]
public class RecoveringWorkflow : Workflow<RecoveringWorkflowRequest, Unit>
{
    public override async Task<Unit> OrchestrateAsync(IWorkflowExecution<RecoveringWorkflowRequest> execution)
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
