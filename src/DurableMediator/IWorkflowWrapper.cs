using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal interface IWorkflowWrapper
{
    Task OrchestrateAsync(IDurableOrchestrationContext context, EntityId entityId, IDurableMediator mediator);
}
