using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public interface IWorkflow<TRequest>
    where TRequest : IWorkflowRequest
{
    string Name { get; }

    Task OrchestrateAsync(IDurableOrchestrationContext context, TRequest request, EntityId entityId, IDurableMediator mediator);
}

