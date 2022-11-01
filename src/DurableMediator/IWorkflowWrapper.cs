using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal interface IWorkflowWrapper
{
    Type WorkflowType { get; }

    Task OrchestrateAsync(
        IDurableOrchestrationContext context, 
        ILogger replaySafeLogger);
}
