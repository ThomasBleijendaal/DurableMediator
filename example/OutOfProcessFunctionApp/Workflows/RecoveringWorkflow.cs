using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace OutOfProcessFunctionApp.Workflows;

/// <summary>
/// The recovering workflow demonstrates how to handle requests that can 'successfully fail' and require a check if they actually succeeded before retrying.
/// </summary>
public class RecoveringWorkflow : IWorkflow<RecoveringWorkflowRequest>
{
    [Function(nameof(RecoveringWorkflow))]
    public static Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        => Workflow.StartAsync<RecoveringWorkflow, RecoveringWorkflowRequest>(context);

    public async Task OrchestrateAsync(IWorkflowExecution<RecoveringWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        var response = await execution.SendWithCheckAsync(
            new FailingRequest(execution.Request.FlakyResourceId),
            new CheckIfFailingRequestAppliedRequest(execution.Request.FlakyResourceId),
            maxAttempts: 20);

        logger.LogInformation("Received {@response}", response);

        logger.LogInformation("Workflow done");
    }
}
