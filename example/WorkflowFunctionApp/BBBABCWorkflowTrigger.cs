using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class BBBABCWorkflowTrigger
{
    [FunctionName(nameof(BBBABCWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bbbabc")] HttpRequestMessage req,
        [Workflow] IWorkflowStarter starter)
    {
        var start = await starter.StartNewAsync(new BBBABCWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
