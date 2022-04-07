using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator.Functions;

internal static class WorkflowOrchestratorFunction
{
    public static Task OrchestarateAsync(IDurableOrchestrationContext ctx, IWorkflowOrchestrator orchestrator) 
        => orchestrator.OrchestrateAsync(ctx);
}
