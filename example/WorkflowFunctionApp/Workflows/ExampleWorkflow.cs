using DurableMediator;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows
{
    internal record ExampleWorkflow(ILogger<ExampleWorkflow> Logger) : IWorkflow<ExampleWorkflowRequest>
    {
        public string Name => "Example";

        public async Task OrchestrateAsync(
            IDurableOrchestrationContext context, 
            ExampleWorkflowRequest request, 
            EntityId entityId,
            IDurableMediator mediator)
        {
            var logger = context.CreateReplaySafeLogger(Logger);

            logger.LogInformation("Start with workflow");

            var stepAResponse = await mediator.SendAsync(new RequestA(request.Id));

            logger.LogInformation("Response from Step A was {response}", stepAResponse);

            await context.TryStepUntilSuccessfulAsync(
                async () =>
                {
                    var stepBResponse = await mediator.SendAsync(new RequestB(request.Id));

                    logger.LogInformation("Step B was {success}", stepBResponse.Success ? "successful" : "not successful");

                    return stepBResponse.Success;
                },
                maxRetries: 10,
                millisecondsBetweenAttempt: 100);

            await mediator.SendAsync(new RequestC(stepAResponse.Id));

            logger.LogInformation("Workflow done");
        }
    }
}
