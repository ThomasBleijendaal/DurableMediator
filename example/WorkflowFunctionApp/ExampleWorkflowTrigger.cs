using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class ExampleWorkflowTrigger
{
    [FunctionName(nameof(ExampleWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
        [Workflow] IWorkflowOrchestrator orchestrator)
    {
        var start = await orchestrator.StartNewAsync(new ExampleWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
