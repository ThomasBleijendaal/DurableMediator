using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class ABCWorkflowTrigger
{
    [FunctionName(nameof(ABCWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "abc")] HttpRequestMessage req,
        [Workflow] IWorkflowStarter starter)
    {
        var start = await starter.StartNewAsync(new ABCWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
