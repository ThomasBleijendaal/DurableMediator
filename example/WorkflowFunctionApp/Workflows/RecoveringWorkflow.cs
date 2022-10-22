using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

/// <summary>
/// The recovering workflow demonstrates how to handle requests that can 'successfully fail' and require a check if they actually succeeded before retrying.
/// </summary>
/// <param name="Logger"></param>
internal record RecoveringWorkflow(ILogger<RecoveringWorkflow> Logger) : IWorkflow<RecoveringWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(WorkflowExecution<RecoveringWorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        await execution.ExecuteWithCheckAsync(
            new FailingRequest(execution.Request.FlakyResourceId),
            new CheckIfFailingRequestAppliedRequest(execution.Request.FlakyResourceId),
            CancellationToken.None,
            maxAttempts: 20);

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
