using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal interface IWorkflowWrapper
{
    Type WorkflowType { get; }

    Task OrchestrateAsync(
        IDurableOrchestrationContext context, 
        EntityId entityId, 
        IDurableMediator mediator,
        ILogger replaySafeLogger);
}
