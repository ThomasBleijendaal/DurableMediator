using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal interface IWorkflowWrapper
{
    string Name { get; }

    Task OrchestrateAsync(IDurableOrchestrationContext context, EntityId entityId, IDurableMediator mediator);
}
