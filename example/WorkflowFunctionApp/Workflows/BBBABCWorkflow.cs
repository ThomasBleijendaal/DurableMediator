using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace WorkflowFunctionApp.Workflows;

internal record BBBABCWorkflow(ILogger<BBBABCWorkflow> Logger) : IWorkflow<BBBABCWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(WorkflowExecution<BBBABCWorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        var bbbResult = await execution.CallSubWorkflowAsync(new BBBWorkflowRequest(execution.Request.BbbAbcId));
        if (bbbResult == null)
        {
            logger.LogInformation("BBB workflow returned null");

            return Unit.Value;
        }

        logger.LogInformation("BBB workflow returned {id}", bbbResult.BbbId);

        logger.LogInformation("Triggering ABC");

        execution.StartWorkflow(new ABCWorkflowRequest(bbbResult.BbbId));

        logger.LogInformation("Workflow done - not waiting on ABC");

        return Unit.Value;
    }
}
