using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class ABCBBBWorkflowTrigger
{
    [FunctionName(nameof(ABCBBBWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "abcbbb")] HttpRequestMessage req,
        [Workflow] IWorkflowOrchestrator orchestrator)
    {
        var start = await orchestrator.StartNewAsync(new BBBWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
