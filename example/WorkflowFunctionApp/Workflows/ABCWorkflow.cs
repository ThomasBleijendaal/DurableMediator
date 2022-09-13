using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

internal record ABCWorkflow(ILogger<ABCWorkflow> Logger) : IWorkflow<ABCWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(WorkflowExecution<ABCWorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        var stepAResponse = await execution.ExecuteWithDelayAsync(new RequestA(execution.Request.AbcId), CancellationToken.None, TimeSpan.FromSeconds(3));

        logger.LogInformation("Response from Step A was {response}", stepAResponse);

        try
        {
            await execution.ExecuteWithRetryAsync(new RequestB(execution.Request.AbcId), CancellationToken.None, 3, TimeSpan.FromSeconds(10));
        }
        catch (OrchestrationRetryException)
        {
            logger.LogWarning("Failed to do Step B - not going to do Step C");

            return Unit.Value;
        }

        await execution.ExecuteAsync(new RequestC(stepAResponse.Id));

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
