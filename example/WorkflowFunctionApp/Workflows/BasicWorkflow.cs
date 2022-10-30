using DurableMediator;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

/// <summary>
/// The basic workflow demonstrates how to perform multiple requests in a single workflow. 
/// </summary>
/// <param name="Logger"></param>
internal record BasicWorkflow(ILogger<BasicWorkflow> Logger) : IWorkflow<BasicWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(IWorkflowExecution<BasicWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        // workflows support sequential requests
        await execution.ExecuteAsync(new SimpleRequest(execution.Request.RequestId, "1"));
        await execution.ExecuteAsync(new SimpleRequest(execution.Request.RequestId, "2"));
        await execution.ExecuteAsync(new SimpleRequest(execution.Request.RequestId, "3"));

        // workflows support parallel requests
        await Task.WhenAll(
            execution.ExecuteAsync(new SimpleRequest(execution.Request.RequestId, "A")),
            execution.ExecuteAsync(new SimpleRequest(execution.Request.RequestId, "B")),
            execution.ExecuteAsync(new SimpleRequest(execution.Request.RequestId, "C")));

        // workflows support doing parallel stuff while requests run
        var slowTask = execution.ExecuteAsync(new SlowRequest(execution.Request.RequestId));

        do
        {
            await execution.OrchestrationContext.CreateTimer(execution.OrchestrationContext.CurrentUtcDateTime.AddSeconds(1), CancellationToken.None);

            if (slowTask.IsCompleted)
            {
                logger.LogInformation("Slow task done");

                break;
            }
            else
            {
                logger.LogInformation("Slow task still pending");
            }
        }
        while (true);

        // workflows can trigger other workflows and wait for their completion
        var workflowResult = await execution.CallSubWorkflowAsync(new ReusableWorkflowRequest(Guid.NewGuid()));

        logger.LogInformation("Result from {subWorkflow}", workflowResult?.Description);

        // workflows can also trigger other workflows and not wait their completion
        execution.StartWorkflow(new ReusableWorkflowRequest(Guid.NewGuid()));

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
