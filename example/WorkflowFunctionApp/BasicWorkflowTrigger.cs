using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal static class BasicWorkflowTrigger
{
    [FunctionName(nameof(BasicWorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basic")] HttpRequestMessage req,
        [Workflow] IWorkflowStarter starter)
    {
        var start = await starter.StartNewAsync(new BasicWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
