using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace WorkflowFunctionApp.Workflows;

internal record BBBABCWorkflow(ILogger<BBBABCWorkflow> Logger) : IWorkflow<BBBABCWorkflowRequest, Unit>
{
    public string Name => "ABCBBB";

    public async Task<Unit> OrchestrateAsync(WorkflowContext<BBBABCWorkflowRequest> context)
    {
        var logger = context.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        var bbbResult = await context.SubOrchestrator.CallSubWorkflowAsync(new BBBWorkflowRequest(context.Request.Id));
        if (bbbResult == null)
        {
            logger.LogInformation("BBB workflow returned null");

            return Unit.Value;
        }

        logger.LogInformation("BBB workflow returned {id}", bbbResult.Id);

        logger.LogInformation("Triggering ABC");

        context.SubOrchestrator.StartWorkflow(new ABCWorkflowRequest(bbbResult.Id));

        logger.LogInformation("Workflow done - not waiting on ABC");

        return Unit.Value;
    }
}
