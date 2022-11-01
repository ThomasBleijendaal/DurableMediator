using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class ResilientWorkflowTrigger
{
    [FunctionName(nameof(ResilientWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resilient")] HttpRequestMessage req,
        [Workflow] IWorkflowStarter starter)
    {
        var start = await starter.StartNewAsync(new ResilientWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
