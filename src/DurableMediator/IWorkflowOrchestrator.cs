using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal interface IWorkflowOrchestrator
{ 
    Task OrchestrateAsync(IDurableOrchestrationContext context);
}
