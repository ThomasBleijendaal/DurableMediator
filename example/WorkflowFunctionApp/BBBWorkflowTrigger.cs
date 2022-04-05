using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class BBBWorkflowTrigger
{
    [FunctionName(nameof(BBBWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bbb")] HttpRequestMessage req,
        [Workflow] IWorkflowOrchestrator orchestrator)
    {
        // TODO: fix generics
        var start = await orchestrator.StartNewAsync<BBBWorkflowRequest, BBBWorkflowResponse>(new BBBWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
