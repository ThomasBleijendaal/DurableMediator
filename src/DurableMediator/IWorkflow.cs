using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public interface IWorkflow<TRequest, TResponse>
    where TRequest : IWorkflowRequest<TResponse>
{
    string Name { get; }

    Task<TResponse> OrchestrateAsync(IDurableOrchestrationContext context, TRequest request, EntityId entityId, IDurableMediator mediator);
}
