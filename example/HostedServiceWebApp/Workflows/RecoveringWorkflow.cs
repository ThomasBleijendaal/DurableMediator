using DurableMediator.HostedService;
using WorkflowHandlers.Requests;

namespace HostedServiceWebApp.Workflows;

/// <summary>
/// The recovering workflow demonstrates how to handle requests that can 'successfully fail' and require a check if they actually succeeded before retrying.
/// </summary>
public class RecoveringWorkflow : IWorkflow<RecoveringWorkflowRequest>
{
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
