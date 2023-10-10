using DurableMediator.OutOfProcess;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace OutOfProcessFunctionApp.Workflows;

/// <summary>
/// The basic workflow demonstrates how to perform multiple requests in a single workflow. 
/// </summary>
[DurableTask(nameof(BasicWorkflow))]
public class BasicWorkflow : Workflow<BasicWorkflowRequest>
{
    public override async Task OrchestrateAsync(IWorkflowExecution<BasicWorkflowRequest> execution)
    {
        var logger = execution.ReplaySafeLogger;

        logger.LogInformation("Start with workflow");

        // workflows support requests without responses
        await execution.SendAsync(new CommandRequest(execution.Request.RequestId, "command"));

        // workflows support sequential requests
        await execution.SendAsync(new SimpleRequest(execution.Request.RequestId, "1"));
        await execution.SendAsync(new SimpleRequest(execution.Request.RequestId, "2"));
        await execution.SendAsync(new SimpleRequest(execution.Request.RequestId, "3"));

        // workflows support parallel requests
        await Task.WhenAll(Enumerable.Range('A', 26).Select(i =>
            execution.SendAsync(new SimpleRequest(execution.Request.RequestId, Convert.ToString((char)i)))));

        // workflows support doing parallel stuff while requests run
        var slowTask = execution.SendAsync(new SlowRequest(execution.Request.RequestId));

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
        var workflowResult = await execution.CallSubWorkflowAsync(new ReusableWorkflowRequest(execution.OrchestrationContext.NewGuid()));

        logger.LogInformation("Result from {subWorkflow}", workflowResult?.Description);

        logger.LogInformation("Workflow done");
    }
}
