using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows;

internal record ABCWorkflow(ILogger<ABCWorkflow> Logger) : IWorkflow<ABCWorkflowRequest, Unit>
{
    public async Task<Unit> OrchestrateAsync(WorkflowContext<ABCWorkflowRequest> context)
    {
        var logger = context.OrchestrationContext.CreateReplaySafeLogger(Logger);

        logger.LogInformation("Start with workflow");

        var stepAResponse = await context.SendAsync(new RequestA(context.Request.AbcId));

        logger.LogInformation("Response from Step A was {response}", stepAResponse);

        try
        {
            await context.SendWithRetryAsync(new RequestB(context.Request.AbcId), CancellationToken.None, 3, 10_000);


            //await context.TryAsync(
            //    async () =>
            //    {
            //        var stepBResponse = await context.DurableMediator.SendAsync(new RequestB(context.Request.AbcId));

            //        logger.LogInformation("Step B was {success}", stepBResponse.Success ? "successful" : "not successful");

            //        return stepBResponse.Success;
            //    },
            //    CancellationToken.None,
            //    maxRetries: 3,
            //    millisecondsBetweenAttempt: 10_000);
        }
        catch (OrchestrationRetryException)
        {
            logger.LogWarning("Failed to do Step B - not going to do Step C");

            return Unit.Value;
        }

        await context.SendAsync(new RequestC(stepAResponse.Id));

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
