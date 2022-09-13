using DurableMediator;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

internal record BBBWorkflow(ILogger<BBBWorkflow> Logger) : IWorkflow<BBBWorkflowRequest, BBBWorkflowResponse>
{
    public async Task<BBBWorkflowResponse> OrchestrateAsync(WorkflowExecution<BBBWorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        var step1 = await execution.ExecuteAsync(new RequestB(execution.Request.BbbId));

        var step2 = await execution.ExecuteAsync(new RequestB(step1.Id));

        var step3 = await execution.ExecuteAsync(new RequestB(step2.Id));

        logger.LogInformation("Workflow done");

        return new BBBWorkflowResponse(step3.Id);
    }
}
