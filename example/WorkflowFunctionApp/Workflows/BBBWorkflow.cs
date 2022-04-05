using DurableMediator;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.Workflows
{
    internal record BBBWorkflow(ILogger<BBBWorkflow> Logger) : IWorkflow<BBBWorkflowRequest, Unit>
    {
        public string Name => "BBB";

        public async Task<Unit> OrchestrateAsync(
            IDurableOrchestrationContext context, 
            BBBWorkflowRequest request, 
            EntityId entityId,
            IDurableMediator mediator)
        {
            var logger = context.CreateReplaySafeLogger(Logger);

            logger.LogInformation("Start with workflow");

            await mediator.SendAsync(new RequestB(request.Id));

            await mediator.SendAsync(new RequestB(request.Id));

            await mediator.SendAsync(new RequestB(request.Id));

            logger.LogInformation("Workflow done");

            return Unit.Value;
        }
    }
}
