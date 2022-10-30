using DurableMediator;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.Workflows;

/// <summary>
/// The reusable workflow demonstrates how to return data from workflows, so other workflows can use this workflow in their own process.
/// </summary>
/// <param name="Logger"></param>
internal record ReusableWorkflow(ILogger<ReusableWorkflow> Logger) : IWorkflow<ReusableWorkflowRequest, ReusableWorkflowResponse>
{
    public async Task<ReusableWorkflowResponse> OrchestrateAsync(IWorkflowExecution<ReusableWorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        await execution.OrchestrationContext.CreateTimer(execution.OrchestrationContext.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None);

        logger.LogInformation("Workflow done");

        return new ReusableWorkflowResponse("ReusableWorkflow");
    }
}
