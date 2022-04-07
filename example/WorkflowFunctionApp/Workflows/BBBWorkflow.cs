using DurableMediator;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

internal record BBBWorkflow(ILogger<BBBWorkflow> Logger) : IWorkflow<BBBWorkflowRequest, BBBWorkflowResponse>
{
    public string Name => "BBB";

    public async Task<BBBWorkflowResponse> OrchestrateAsync(WorkflowContext<BBBWorkflowRequest> context)
    {
        var logger = context.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        var step1 = await context.DurableMediator.SendAsync(new RequestB(context.Request.Id));

        var step2 = await context.DurableMediator.SendAsync(new RequestB(step1.Id));

        var step3 = await context.DurableMediator.SendAsync(new RequestB(step2.Id));

        logger.LogInformation("Workflow done");

        return new BBBWorkflowResponse(step3.Id);
    }
}
