using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Workflows;

/// <summary>
/// The reusable workflow demonstrates how to return data from workflows, so other workflows can use this workflow in their own process.
/// </summary>
public class ReusableWorkflow : IWorkflow<ReusableWorkflowRequest, ReusableWorkflowResponse>
{
    [Function(nameof(ReusableWorkflow))]
    public static Task<ReusableWorkflowResponse> RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        => Workflow.StartAsync<ReusableWorkflow, ReusableWorkflowRequest, ReusableWorkflowResponse>(context);

    public async Task<ReusableWorkflowResponse> OrchestrateAsync(IWorkflowExecution<ReusableWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        await execution.OrchestrationContext.CreateTimer(execution.OrchestrationContext.CurrentUtcDateTime.AddSeconds(10), CancellationToken.None);

        logger.LogInformation("Workflow done");

        return new ReusableWorkflowResponse("ReusableWorkflow");
    }
}
