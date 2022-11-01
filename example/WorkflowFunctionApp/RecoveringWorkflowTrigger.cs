using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class RecoveringWorkflowTrigger
{
    [FunctionName(nameof(RecoveringWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recovering")] HttpRequestMessage req,
        [Workflow] IWorkflowStarter starter)
    {
        var start = await starter.StartNewAsync(new RecoveringWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
