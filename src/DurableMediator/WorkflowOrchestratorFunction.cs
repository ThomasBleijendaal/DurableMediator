using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal static class WorkflowOrchestratorFunction
{
    public static Task OrchestarateAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext ctx,
        [Workflow] IWorkflowOrchestrator orchestrator)
        => orchestrator.OrchestrateAsync(ctx);
}
