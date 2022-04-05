using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows
{
    internal record ABCWorkflow(ILogger<ABCWorkflow> Logger) : IWorkflow<ABCWorkflowRequest, Unit>
    {
        public string Name => "ABC";

        public async Task<Unit> OrchestrateAsync(
            IDurableOrchestrationContext context, 
            ABCWorkflowRequest request, 
            EntityId entityId,
            IDurableMediator mediator)
        {
            var logger = context.CreateReplaySafeLogger(Logger);

            logger.LogInformation("Start with workflow");

            var stepAResponse = await mediator.SendAsync(new RequestA(request.Id));

            logger.LogInformation("Response from Step A was {response}", stepAResponse);

            try
            {
                await context.TryStepUntilSuccessfulAsync(
                    async () =>
                    {
                        var stepBResponse = await mediator.SendAsync(new RequestB(request.Id));

                        logger.LogInformation("Step B was {success}", stepBResponse.Success ? "successful" : "not successful");

                        return stepBResponse.Success;
                    },
                    maxRetries: 3,
                    millisecondsBetweenAttempt: 100);
            }
            catch (OrchestrationRetryException)
            {
                logger.LogWarning("Failed to do Step B - not going to do Step C");

                return Unit.Value;
            }

            await mediator.SendAsync(new RequestC(stepAResponse.Id));

            logger.LogInformation("Workflow done");

            return Unit.Value;
        }
    }
}
